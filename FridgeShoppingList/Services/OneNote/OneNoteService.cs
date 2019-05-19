using FridgeShoppingList.Helpers;
using FridgeShoppingList.Models;
using FridgeShoppingList.Services.SettingsServices;
using HtmlAgilityPack;
using Microsoft.Identity.Client;
using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;
using Optional;
using Optional.Unsafe;
using System;
using System.Collections.Generic;
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
        Task<bool> UpdateShoppingListContent(IEnumerable<OneNoteCheckboxNode> localList, DateTimeOffset lastLocalUpdate);
        Task<Option<IEnumerable<DeletePageResult>>> DeleteShoppingListPages();
        Task Logout();
    }

    public class OneNoteService : IOneNoteService
    {
        private const string ShoppingListPageFilter = "tolower(title) eq 'shopping list' and tolower(parentSection/displayName) eq 'foodd'";
        private static readonly string[] ControlAppPagesScopes = new string[] { "Notes.ReadWrite" };
        private const string AzureClientID = "84f0ce39-3578-4586-87ed-741a0d00f5ae";

        private readonly IPublicClientApplication _authClient;
        private static MSGraph.GraphServiceClient _graphClient;
        private readonly SettingsService _settingsService;

        private AuthenticationResult _authToken;
        private string _cachedPageId;

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
                    // do sad things
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
            Option<MSGraph.OnenotePage> pageResult = await GetFooddPage();
            if (!pageResult.HasValue)
            {
                // create the page, return none
                return Option.None<IEnumerable<OneNoteCheckboxNode>>();
            }

            var page = pageResult.ValueOrFailure();

            using (page.Content)
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.Load(page.Content);
                var checkboxNodes = htmlDoc.DocumentNode
                    .SelectNodes("//p")
                    .Select(x => new OneNoteCheckboxNode(x));
                return Option.Some(checkboxNodes);
            }
        }

        public async Task<bool> UpdateShoppingListContent(IEnumerable<OneNoteCheckboxNode> localList, DateTimeOffset lastLocalUpdate)
        {
            if (!_initialized) { await Initialize(); }

            // First get latest updates from the server, and check modified time
            var getPageResult = await GetFooddPage();
            if (!getPageResult.HasValue)
            {
                return false;
            }

            MSGraph.OnenotePage latestServerPage = getPageResult.ValueOrFailure();

            if (latestServerPage.LastModifiedDateTime.HasValue 
                && latestServerPage.LastModifiedDateTime.Value > lastLocalUpdate)
            {
                // No updates necessary, we're done!
                return true;
            }

            var request = _graphClient.Me.Onenote
                .Pages[await GetFooddPageId()]
                .Content
                .Request()
                .GetHttpRequestMessage();

            request.Content = new StringContent(JsonConvert.SerializeObject(localList.Select(x => 
            {
                return new OneNoteChangeObject
                {
                    // TODO: For each element we send up, check it against the server. If it exists, update it. If not, append it.
                    Action = OneNoteChangeAction.Replace,
                    Target = x.GeneratedId ?? OneNoteChangeTarget.DataId(x.DataId),
                    HtmlContent = x.ToHtmlContent(),
                    Position = OneNoteChangePosition.After
                };
            })), Encoding.UTF8, Constants.JsonApplicationMediaType);
            request.Method = new HttpMethod("PATCH");            
            var response = await _graphClient.HttpProvider.SendAsync(request);

            if (response.StatusCode != (System.Net.HttpStatusCode)204)
            {
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

        private async Task<Option<MSGraph.OnenotePage>> GetFooddPage()
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

            _cachedPageId = page.Id;

            // The pagesCollection doesn't return everything, but a call to an individual page with an ID will.
            Stream pageContent = await _graphClient.Me.Onenote.Pages[page.Id]
                .Content
                .Request(new[] { new MSGraph.QueryOption("includeIDs", "true") })
                .GetAsync();
            if (page == null || pageContent == null)
            {
                return Option.None<MSGraph.OnenotePage>();
            }

            page.Content = pageContent;
            return Option.Some(page);
        }

        private async Task<string> GetFooddPageId()
        {
            if (_cachedPageId != null)
            {
                return _cachedPageId;
            }

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

            _cachedPageId = page.Id;
            return page.Id;
        }
    }
}