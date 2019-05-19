using FridgeShoppingList.Helpers;
using FridgeShoppingList.Models;
using FridgeShoppingList.Services.SettingsServices;
using HtmlAgilityPack;
using Microsoft.Identity.Client;
using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;
using Optional;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MSGraph = Microsoft.Graph;

namespace FridgeShoppingList.Services
{
    public interface IOneNoteService
    {
        Task Initialize();
        event EventHandler<bool> ConnectedStatusChanged;
        bool ConnectedStatus { get; }
        Task<Option<IEnumerable<OneNoteCheckboxNode>>> GetShoppingListPageContent();
        Task<bool> UpdateShoppingListContent(IEnumerable<OneNoteCheckboxNode> localList);
        Task<Option<IEnumerable<DeletePageResult>>> DeleteShoppingListPages();
        Task Logout();
    }

    public class OneNoteService : IOneNoteService
    {
        private const string ShoppingListPageFilter = "tolower(title) eq 'shopping list' and tolower(parentSection/displayName) eq 'foodd'";
        private static readonly string[] ControlAppPagesScopes = new string[] { "Notes.ReadWrite" };
        private const string AzureClientID = "84f0ce39-3578-4586-87ed-741a0d00f5ae"; // TODO: Put this into an external file somewhere.

        private readonly IPublicClientApplication _authClient;
        private static MSGraph.GraphServiceClient _graphClient;
        private readonly SettingsService _settingsService;

        private AuthenticationResult _authToken;

        private bool _connectedStatus;
        public bool ConnectedStatus
        {
            get { return _connectedStatus; }
            private set
            {
                if (_connectedStatus != value)
                {
                    _connectedStatus = value;
                    ConnectedStatusChanged?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<bool> ConnectedStatusChanged;

        public OneNoteService(SettingsService settings)
        {
            _settingsService = settings;
            _authClient = PublicClientApplicationBuilder.Create(AzureClientID).Build();
        }

        bool _initialized;
        public async Task Initialize()
        {
            if (!_initialized)
            {
                IEnumerable<IAccount> accounts = await _authClient.GetAccountsAsync();
                if (!accounts.Any())
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                    {
                        try
                        {
                            _authToken = await _authClient.AcquireTokenInteractive(ControlAppPagesScopes).ExecuteAsync();
                        }
                        catch (MsalException ex)
                        {
                            // one instance of these is just if the user cancelled auth
                            // swallow it, we'll handle the failure further down under our _authToken == null check
                        }
                    });
                }
                else
                {
                    _authToken = await _authClient.AcquireTokenSilent(ControlAppPagesScopes, accounts.First()).ExecuteAsync();
                }

                if (_authToken == null)
                {
                    // TODO: Somehow do error things
                }

                _graphClient = new MSGraph.GraphServiceClient(new MSGraph.DelegateAuthenticationProvider(requestMessage =>
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", _authToken.AccessToken);
                    return Task.CompletedTask;
                }));

                _initialized = true;
                ConnectedStatus = true;
            }
        }

        public async Task<Option<IEnumerable<OneNoteCheckboxNode>>> GetShoppingListPageContent()
        {
            if (!_initialized) { await Initialize(); }
            MSGraph.OnenotePage page = await GetFooddPageAndContent();
            if (page == null)
            {
                //TODO: create the page before returning


                return Option.None<IEnumerable<OneNoteCheckboxNode>>();
            }

            var content = ParseOneNoteContent(page);
            if (content == null)
            {
                return Option.None<IEnumerable<OneNoteCheckboxNode>>();
            }

            return Option.Some(content);
        }

        public async Task<bool> UpdateShoppingListContent(IEnumerable<OneNoteCheckboxNode> localList)
        {
            if (!_initialized) { await Initialize(); }

            // 1. Check to see if we need to update
            MSGraph.OnenotePage latestServerPage = await GetFooddPageAndContent();
            if (latestServerPage == null)
            {
                return false;
            }

            var lastLocalUpdate = _settingsService.LastLocalUpdate;
            if (latestServerPage.LastModifiedDateTime.HasValue
                && latestServerPage.LastModifiedDateTime.Value > lastLocalUpdate)
            {
                // If the remote page is more recent, don't send up anything. Remote always wins.
                return true;
            }

            // 2. We need to update, so generate the list of ChangeObjects.
            IEnumerable<OneNoteCheckboxNode> existingContent = ParseOneNoteContent(latestServerPage);
            var listToSend = localList
                .Select(local =>
                {
                    bool exists = existingContent.Any(remote => remote.GeneratedId == local.GeneratedId);
                    return new OneNoteChangeObject
                    {
                        // TODO: Need to make it so that if we're appending, the Target is actually the parent <div>'s GeneratedID.
                        Action = exists ? OneNoteChangeAction.Replace : OneNoteChangeAction.Append,
                        HtmlContent = local.ToHtmlContent(),
                        Position = OneNoteChangePosition.After,
                        Target = exists ? local.GeneratedId : local.DataId,
                    };
                });

            // 3. Prepare and send the change request.
            HttpRequestMessage updateRequest = _graphClient.Me.Onenote
                .Pages[latestServerPage.Id]
                .Content
                .Request()
                .GetHttpRequestMessage();

            updateRequest.Content = new StringContent(
                JsonConvert.SerializeObject(listToSend),
                Encoding.UTF8,
                Constants.JsonApplicationMediaType
            );
            updateRequest.Method = new HttpMethod("PATCH");
            var response = await _graphClient.HttpProvider.SendAsync(updateRequest);

            if (response.StatusCode != (System.Net.HttpStatusCode)204)
            {
                Debug.WriteLine($"Failed to update remote OneNote content: HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()} ");
                return false;
            }

            return true;
        }

        public async Task<Option<IEnumerable<DeletePageResult>>> DeleteShoppingListPages()
        {
            if (!_initialized) { await Initialize(); }
            throw new NotImplementedException();
        }

        public async Task Logout()
        {
            if (!_initialized) { await Initialize(); }
            throw new NotImplementedException();
            ConnectedStatus = false;
        }

        private async Task<MSGraph.OnenotePage> GetFooddPageAndContent()
        {
            var page = await GetFooddPage();
            if (page == null)
            {
                return null;
            }

            page = await GetFooddPageContent(page);
            return page;
        }

        private async Task<MSGraph.OnenotePage> GetFooddPage()
        {
            var allPages = new List<MSGraph.IOnenotePagesCollectionPage>();
            MSGraph.IOnenotePagesCollectionPage pageOfPages = null;
            do
            {
                pageOfPages = await _graphClient.Me.Onenote.Pages
                    .Request()
                    .Filter(ShoppingListPageFilter)
                    .GetAsync();
                allPages.Add(pageOfPages);
            } while (pageOfPages?.NextPageRequest != null);

            var page = allPages
                .SelectMany(x => x.CurrentPage)
                .FirstOrDefault();
            
            return page;
        }

        private async Task<MSGraph.OnenotePage> GetFooddPageContent(MSGraph.OnenotePage page)
        {
            // The pagesCollection doesn't return everything, but a call to an individual page with an ID will.
            Stream pageContent = await _graphClient.Me.Onenote.Pages[page.Id]
                .Content
                .Request(new[] { new MSGraph.QueryOption("includeIDs", "true") })
                .GetAsync();
            if (page == null || pageContent == null)
            {
                return null;
            }

            page.Content = pageContent;
            return page;
        }

        private IEnumerable<OneNoteCheckboxNode> ParseOneNoteContent(MSGraph.OnenotePage page)
        {
            using (page.Content)
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.Load(page.Content);
                return htmlDoc.DocumentNode
                    .SelectNodes("//p")
                    .Select(x => new OneNoteCheckboxNode(x));
            }
        }
    }
}