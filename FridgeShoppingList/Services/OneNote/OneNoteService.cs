using FridgeShoppingList.Models;
using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Windows.Storage.Streams;
using FridgeShoppingList.Helpers;
using HtmlAgilityPack;
using FridgeShoppingList.ViewModels.ControlViewModels;
using Windows.ApplicationModel;
using Windows.Security.Credentials;
using FridgeShoppingList.Services.SettingsServices;
using DynamicData;
using System.IO;
using FridgeShoppingList.Extensions;
using Microsoft.Practices.ServiceLocation;
using Optional;
using Microsoft.OneDrive.Sdk.Authentication;
using Graph = Microsoft.Graph;
using Windows.Web.Http.Filters;
using Optional.Unsafe;

namespace FridgeShoppingList.Services
{
    public interface IOneNoteService
    {
        event EventHandler<bool> ConnectedStatusChanged;
        bool ConnectedStatus { get; }
        Task<Option<IEnumerable<OneNoteCheckboxNode>>> GetShoppingListPageContent();
        Task<bool> UpdateShoppingListContent(IEnumerable<OneNoteCheckboxNode> localList);
        Task<Option<IEnumerable<DeletePageResult>>> DeleteShoppingListPages();
        Task Logout();
    }

    public class OneNoteService : IOneNoteService
    {
        private const string OneNoteBaseUrl = "https://www.onenote.com/api/v1.0/me/";
        private const string GetTokenUrl = "https://login.live.com/oauth20_token.srf";
        private const string ShoppingListPageSearchString = OneNoteBaseUrl + "notes/pages?filter=tolower(title)%20eq%20'shopping%20list'%20and%20tolower(parentSection%2Fname)%20eq%20'foodd'";
        private const string OneDriveRedirectUri = "https://login.live.com/oauth20_desktop.srf";
        private static readonly string[] ControlAppPagesScopes = new string[] { "office.onenote_update_by_app", "wl.offline_access" };
        private const string OneDriveClientId = "00000000481CBFE3";
        private const string CheckboxDivId = "checkbox-div";

        private static readonly HttpClient _httpClient;
        private readonly SettingsService _settingsService;

        private MsaAuthenticationProvider _msaAuthProvider;
        private CredentialVault _credentials = new CredentialVault(OneDriveClientId);
        private string CachedPageId => _settingsService.OneNotePageId;

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

        static OneNoteService()
        {
            var httpBaseFilter = new HttpBaseProtocolFilter();
            httpBaseFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
            _httpClient = new HttpClient(httpBaseFilter);
        }

        public OneNoteService(SettingsService settings)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue(Constants.JsonApplicationMediaType));
            _settingsService = settings;
        }

        bool _initialized;
        private async Task Initialize()
        {
            if (!_initialized)
            {
                _msaAuthProvider = new MsaAuthenticationProvider(OneDriveClientId, OneDriveRedirectUri, ControlAppPagesScopes, _credentials);
                bool success = await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                {
                    try
                    {
                        await _msaAuthProvider.RestoreMostRecentFromCacheOrAuthenticateUserAsync();
                        return true;
                    }
                    catch (Graph.ServiceException ex) when (ex.Error.Code == OAuthConstants.ErrorCodes.AuthenticationCancelled)
                    {
                        // Swallow it in this case. The user just cancelling is fine.
                        System.Diagnostics.Debug.WriteLine("Authentication cancelled by user.");
                        return false;
                    }
                });
                if (!success)
                {
                    return;
                }
                _httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", _msaAuthProvider.CurrentAccountSession.AccessToken);

                ConnectedStatus = true;
                var validatedPageId = await ValidatePageId(_settingsService.OneNotePageId);
                _settingsService.OneNotePageId = validatedPageId.ValueOr(_settingsService.OneNotePageId);
                if (validatedPageId.HasValue)
                {
                    _initialized = true;
                }
            }
        }

        /// <summary>
        /// Returns the shopping list page ID if it exists. If not, it creates the page and returns the ID.
        /// </summary>
        /// <returns></returns>
        private async Task<Option<string>> ValidatePageId(string pageId)
        {
            string cachedPageId = _settingsService.OneNotePageId;

            // Validate the ID. If we get a 404, it's a dud, and the cache should be cleared.
            //TODO: To correctly get 404s for deleted pages, we might need to hit the /content endpoint
            var getResponse = await MakeOneNoteRequest($"{OneNoteBaseUrl}notes/pages/{cachedPageId}", HttpMethod.Get, validatesPageId: false);
            if (getResponse == null)
            {
                return Option.None<string>();
            }

            if (getResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return await CreateShoppingListPage();
            }
            else if (getResponse.IsSuccessStatusCode)
            {
                var deserializedGetResponse = JsonConvert.DeserializeObject<OneNoteGetPageMetadataResponse>
                    (await getResponse.Content.ReadAsStringAsync());
                return deserializedGetResponse.Id.Some();
            }
            else // Any other failure status code
            {
                return Option.None<string>();
            }
        }

        //Return false if we're unable to create the page, and it either doesn't exist, or we don't know if it exists.
        private async Task<Option<string>> CreateShoppingListPage()
        {
            var pagesResponse = await _httpClient.GetAsync(new Uri(ShoppingListPageSearchString));
            if (pagesResponse == null || !pagesResponse.IsSuccessStatusCode)
            {
                return Option.None<string>();
            }

            string shoppingListPageHtml = (@"<!DOCTYPE html>
    <html>
    <head>
        <title>Shopping List</title>
        <meta name=""created"" content=" + DateTime.Now.ToString("o") + @" />
    </head>
    <body>
        <div data-id=""" + CheckboxDivId + @""">
            <p>Checklist</p>
        </div>
    </body>
    </html>");
            var createResponse = await _httpClient.PostAsync(new Uri($"{OneNoteBaseUrl}notes/pages?sectionName=FOODD"),
                new HttpStringContent(shoppingListPageHtml, UnicodeEncoding.Utf8, Constants.HtmlTextMediaType));
            if (createResponse == null || !createResponse.IsSuccessStatusCode)
            {
                return Option.None<string>();
            }

            //TODO: Grab the ID of the newly-created page and save it in _fooddPageId here
            var deserializedCreateResponse = JsonConvert.DeserializeObject<OneNotePostPageResponse>(await createResponse.Content.ReadAsStringAsync());
            return deserializedCreateResponse.Id.Some();
        }

        /// <summary>
        /// Retrieves the HTML content of the shopping list page, if successful. If unsuccessful, returns None.
        /// </summary>
        /// <returns></returns>
        public async Task<Option<IEnumerable<OneNoteCheckboxNode>>> GetShoppingListPageContent()
        {
            await Initialize();
            if (!_initialized)
            {
                return Option.None<IEnumerable<OneNoteCheckboxNode>>();
            }

            var response = await MakeOneNoteRequest(($"{OneNoteBaseUrl}notes/pages/{CachedPageId}/content?includeIDs=true"), HttpMethod.Get);
            if (response.IsSuccessStatusCode)
            {
                var responseHtml = new HtmlDocument();
                responseHtml.LoadHtml(await response.Content.ReadAsStringAsync());
                var meta = responseHtml.DocumentNode.Descendants("meta");
                var firstDiv = responseHtml.DocumentNode.Descendants("div").FirstOrDefault();
                if (firstDiv != null)
                {
                    return firstDiv
                        .Descendants("p")
                        .Where(x => x.Attributes["data-tag"]?.Value == "to-do"
                                    || x.Attributes["data-tag"]?.Value == "to-do:completed")
                        .Select(x => new OneNoteCheckboxNode(x))
                        .Some();
                }
                return Option.None<IEnumerable<OneNoteCheckboxNode>>();
            }
            else
            {
                return Option.None<IEnumerable<OneNoteCheckboxNode>>();
            }
        }

        public async Task<bool> UpdateShoppingListContent(IEnumerable<OneNoteCheckboxNode> nodesToSend)
        {
            await Initialize();
            if (!_initialized)
            {
                return false;
            }

            // If we have nothing locally, updating the remote list should always be considered a success
            if (nodesToSend.Count() == 0)
            {
                return true;
            }

            var latestNodes = (await GetShoppingListPageContent())
                   .Match(
                       some: x => x,
                       none: () => null
                   );

            if (latestNodes == null)
            {
                return false;
            }

            List<OneNoteChangeObject> processedList = ProcessUploadList(nodesToSend, latestNodes).ToList();
            string listAsJson = JsonConvert.SerializeObject(processedList, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var response = await MakeOneNoteRequest(
                $"{OneNoteBaseUrl}notes/pages/{CachedPageId}/content",
                HttpMethod.Patch,
                new HttpStringContent(listAsJson, UnicodeEncoding.Utf8, Constants.JsonApplicationMediaType));

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Attempt to delete all pages named "Shopping List" under the FOODD section in OneNote.
        /// Returns true if successful or if no pages were found to be deleted.
        /// True should be interpreted as "no Shopping List pages remain".
        /// </summary>
        /// <returns></returns>
        public async Task<Option<IEnumerable<DeletePageResult>>> DeleteShoppingListPages()
        {
            var pagesResponse = await _httpClient.GetAsync(new Uri(ShoppingListPageSearchString));
            if (pagesResponse == null || !pagesResponse.IsSuccessStatusCode)
            {
                return Option.None<IEnumerable<DeletePageResult>>(); ;
            }

            List<DeletePageResult> deletionResults = new List<DeletePageResult>();
            var deserializedResponse = JsonConvert.DeserializeObject<OneNoteGetPagesResponse>(await pagesResponse.Content.ReadAsStringAsync());
            if (deserializedResponse.Data.Any())
            {
                foreach (var page in deserializedResponse.Data)
                {
                    string pageId = page.Id;
                    var response = await MakeOneNoteRequest($"{OneNoteBaseUrl}notes/pages/{pageId}", HttpMethod.Delete, validatesPageId: false);
                    if (response == null || !response.IsSuccessStatusCode)
                    {
                        deletionResults.Add(new DeletePageResult { PageId = pageId, Result = DeleteResult.Success });
                    }
                    else
                    {
                        DeleteResult result = response == null ? DeleteResult.FailureByNetwork
                                            : response.StatusCode == HttpStatusCode.NotFound ? DeleteResult.FailureBy404
                                            : DeleteResult.FailureUnspecified;
                        deletionResults.Add(new DeletePageResult { PageId = pageId, Result = result });
                    }
                }
                return deletionResults.AsEnumerable().Some();
            }
            else
            {
                return Option.None<IEnumerable<DeletePageResult>>(); ;
            }
        }

        /// <summary>
        /// Disconnects from the OneNote API, and deletes all cached keys from the credential vault.
        /// </summary>
        /// <returns></returns>
        public async Task Logout()
        {
            _settingsService.OneNotePageId = null;
            await _msaAuthProvider.SignOutAsync();
            ConnectedStatus = false;
            _initialized = false;
        }

        private async Task<HttpResponseMessage> MakeOneNoteRequest(string url, HttpMethod method, IHttpContent content = null, bool validatesPageId = true)
        {
            if (validatesPageId)
            {
                // Making the assumption that the ID this method got passed is the same as the one in the cache.
                // If this turns out to be an invalid assumption, we'll have to do some string parsing, or pass 
                // in the ID separately or something.
                string oldId = CachedPageId;
                var validatedPageId = await ValidatePageId(CachedPageId);
                if (!validatedPageId.HasValue)
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
                string newId = validatedPageId.ValueOrFailure();
                if (newId != oldId)
                {
                    // Hopefully we never have a scenario where the oldId isn't actually in the request URL...
                    // If we DO, that's two reasons to parse out the ID
                    url = url.Replace(oldId, newId);
                }
            }

            var message = new HttpRequestMessage(method, new Uri(url));
            message.Content = content;

            var response = await _httpClient.SendRequestAsync(message);

            // For now, if we get an auth failure, only try to re-auth and make a new request once. 
            // If this starts happening a lot, maybe we can add exponential back-off or something.
            if (response?.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _msaAuthProvider.AuthenticateUserAsync();
                response = await _httpClient.SendRequestAsync(message);
                return response;
            }
            else
            {
                return response;
            }
        }

        //TODO: Where we match on content, parse out the item number counts.
        private static IEnumerable<OneNoteChangeObject> ProcessUploadList(IEnumerable<OneNoteCheckboxNode> localNodes, IEnumerable<OneNoteCheckboxNode> latestCheckboxNodes)
        {
            // Process local nodes and append or replace as necessary
            foreach (OneNoteCheckboxNode uploadingNode in localNodes)
            {
                var latestNode = latestCheckboxNodes.FirstOrDefault(x => x.DataId == uploadingNode.DataId && uploadingNode.DataId != null);

                // Item exists in both lists, and has a data-id
                if (latestNode != null)
                {
                    uploadingNode.IsChecked = latestNode.IsChecked;
                    uploadingNode.GeneratedId = latestNode.GeneratedId;
                    yield return new OneNoteChangeObject
                    {
                        Target = OneNoteChangeTarget.GeneratedId(latestNode.GeneratedId),
                        Action = OneNoteChangeAction.Replace,
                        HtmlContent = uploadingNode.ToHtmlContent()
                    };
                }
                else
                {
                    // Item not found. Check to see if we can do a name-match.
                    var stringMatchedLatest = latestCheckboxNodes.FirstOrDefault(x => x.Content == uploadingNode.Content);
                    if (stringMatchedLatest != null)
                    {
                        uploadingNode.IsChecked = stringMatchedLatest.IsChecked;
                        uploadingNode.GeneratedId = stringMatchedLatest.GeneratedId;
                        yield return new OneNoteChangeObject
                        {
                            Target = OneNoteChangeTarget.GeneratedId(stringMatchedLatest.GeneratedId),
                            Action = OneNoteChangeAction.Replace,
                            HtmlContent = uploadingNode.ToHtmlContent()
                        };
                    }
                    else
                    {
                        // No item with a matching data-id OR name was found. Safe to just append it.
                        yield return new OneNoteChangeObject
                        {
                            Target = OneNoteChangeTarget.DataId(CheckboxDivId),
                            Action = OneNoteChangeAction.Append,
                            Position = OneNoteChangePosition.After,
                            HtmlContent = uploadingNode.ToHtmlContent()
                        };
                    }
                }
            }

            // Look for any nodes we have serverside that we don't have locally. Mark those as deleted nodes in the list we're about to upload.
            foreach (var latestServerNode in latestCheckboxNodes)
            {
                bool hasLocalCounterpart = localNodes.Any(
                    x => (x.DataId == latestServerNode.DataId && latestServerNode.DataId != null)
                        || (x.Content == latestServerNode.Content));
                if (!hasLocalCounterpart)
                {
                    yield return new OneNoteChangeObject
                    {
                        Target = OneNoteChangeTarget.GeneratedId(latestServerNode.GeneratedId),
                        Action = OneNoteChangeAction.Replace,
                        HtmlContent = "<!-- Undocumented trick: a REPLACE command to OneNote with an HTML comment as the content effectively deletes that element.-->"
                    };
                }
            }
        }
    }
}
