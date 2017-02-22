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

namespace FridgeShoppingList.Services
{
    public interface IOneNoteService
    {
        event EventHandler<bool> ConnectedStatusChanged;
        bool ConnectedStatus { get; }
        Task<Option<List<OneNoteCheckboxNode>>> GetShoppingListPageContent();
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

        private string _fooddPageId;
        private MsaAuthenticationProvider _msaAuthProvider;

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
            _httpClient = new HttpClient();
        }

        public OneNoteService()
        {            
            _httpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue(Constants.JsonApplicationMediaType));
        }

        bool _initialized;
        private async Task Initialize()
        {
            if (!_initialized)
            {               
                _msaAuthProvider = new MsaAuthenticationProvider(OneDriveClientId, OneDriveRedirectUri, ControlAppPagesScopes);
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
                _initialized = await CreateShoppingListPage();
            }
        }

        //Return false if we're unable to create the page, and it either doesn't exist, or we don't know if it exists.
        private async Task<bool> CreateShoppingListPage()
        {
            var pagesResponse = await _httpClient.GetAsync(new Uri(ShoppingListPageSearchString));
            if (pagesResponse == null || !pagesResponse.IsSuccessStatusCode)
            {
                return false;
            }
            var deserializedResponse = JsonConvert.DeserializeObject<OneNoteODataResponse>(await pagesResponse.Content.ReadAsStringAsync());
            if (deserializedResponse.Data.Any())
            {
                _fooddPageId = deserializedResponse.Data.First().Id;
                return true;
            }
            else
            {

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
                    return false;
                }

                //TODO: Grab the ID of the newly-created page and save it in _fooddPageId here
                return true;
            }
        }

        /// <summary>
        /// Retrieves the HTML content of the shopping list page, if successful. If unsuccessful, returns None.
        /// </summary>
        /// <returns></returns>
        public async Task<Option<List<OneNoteCheckboxNode>>> GetShoppingListPageContent()
        {
            await Initialize();
            if (!_initialized)
            {
                return Option.None<List<OneNoteCheckboxNode>>();
            }

            var response = await MakeOneNoteRequest(($"{OneNoteBaseUrl}notes/pages/{_fooddPageId}/content?includeIDs=true"), HttpMethod.Get);
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
                        .ToList()
                        .Some();
                }
                return Option.None<List<OneNoteCheckboxNode>>();
            }
            else
            {
                return Option.None<List<OneNoteCheckboxNode>>();
            }
        }

        public async Task<bool> UpdateShoppingListContent(List<OneNoteCheckboxNode> updatedNodes)
        {
            await Initialize();
            if (!_initialized)
            {
                return false;
            }

            var latestNodes = (await GetShoppingListPageContent())
                .Match(
                    x => x,
                    () => null
                );

            if (latestNodes == null)
            {
                return false;
            }

            List<OneNoteChangeObject> processedList = ProcessUploadList(updatedNodes, latestNodes);
            string listAsJson = JsonConvert.SerializeObject(processedList, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var response = await MakeOneNoteRequest(
                $"{OneNoteBaseUrl}/notes/pages/pages/{_fooddPageId}/content?includeIDs=true", 
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
        /// Disconnects from the OneNote API, and deletes all cached keys from the credential vault.
        /// </summary>
        /// <returns></returns>
        public async Task Logout()
        {
            _fooddPageId = null;
            await _msaAuthProvider.SignOutAsync();
            ConnectedStatus = false;            
        }

        private async Task<HttpResponseMessage> MakeOneNoteRequest(string url, HttpMethod method, IHttpContent content = null)
        {                  
            var message = new HttpRequestMessage(method, new Uri(url));
            message.Content = content;
            var response = await _httpClient.SendRequestAsync(message);

            // For now, if we get an auth failure, only try to re-auth and make a new request once. 
            // If this starts happening a lot, maybe we can add exponential back-off or something.
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _msaAuthProvider.AuthenticateUserAsync();                               
                response = await _httpClient.SendRequestAsync(message);
            }
            return response;
        }

        private static List<OneNoteChangeObject> ProcessUploadList(List<OneNoteCheckboxNode> updatedNodes, List<OneNoteCheckboxNode> latestCheckboxNodes)
        {
            List<OneNoteChangeObject> nodesToSend = new List<OneNoteChangeObject>();            

            foreach(OneNoteCheckboxNode uploadingNode in updatedNodes)
            {
                var latestNode = latestCheckboxNodes.FirstOrDefault(x => x.DataId == uploadingNode.DataId && uploadingNode.DataId != null);

                // Item exists in both lists, and has a data-id
                if (latestNode != null)
                {
                    uploadingNode.IsChecked = false;
                    uploadingNode.GeneratedId = latestNode.GeneratedId;
                    nodesToSend.Add(new OneNoteChangeObject
                    {
                        Target = OneNoteChangeTarget.DataId(uploadingNode.DataId),
                        Action = OneNoteChangeAction.Replace,
                        HtmlContent = uploadingNode.ToHtmlContent()
                    });
                }
                else
                {
                    // Item not found. Check to see if we can do a name-match.
                    var stringMatchedLatest = latestCheckboxNodes.FirstOrDefault(x => x.Content == uploadingNode.Content);
                    if (stringMatchedLatest != null)
                    {
                        uploadingNode.IsChecked = false;
                        uploadingNode.GeneratedId = stringMatchedLatest.GeneratedId;
                        nodesToSend.Add(new OneNoteChangeObject
                        {
                            Target = OneNoteChangeTarget.GeneratedId(uploadingNode.GeneratedId),
                            Action = OneNoteChangeAction.Replace,
                            HtmlContent = uploadingNode.ToHtmlContent()                            
                        });
                    }
                    else
                    {
                        // No item with a matching data-id OR name was found. Safe to just append it.
                        nodesToSend.Add(new OneNoteChangeObject
                        {
                            Target = OneNoteChangeTarget.DataId(CheckboxDivId),
                            Action = OneNoteChangeAction.Append,
                            Position = OneNoteChangePosition.After,
                            HtmlContent = uploadingNode.ToHtmlContent()
                        });
                    }
                }                
            }
            return nodesToSend;
        }
    }
}
