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
    <body></body>
</html>");
                var createResponse = await _httpClient.PostAsync(new Uri($"{OneNoteBaseUrl}notes/pages?sectionName=FOODD"),
                    new HttpStringContent(shoppingListPageHtml, UnicodeEncoding.Utf8, Constants.HtmlTextMediaType));
                if (createResponse == null || !createResponse.IsSuccessStatusCode)
                {
                    return false;
                }

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

        /// <summary>
        /// Disconnects from the OneNote API, and deletes all cached keys from the credential vault.
        /// </summary>
        /// <returns></returns>
        public async Task Logout()
        {
            await _msaAuthProvider.SignOutAsync();
            ConnectedStatus = false;            
        }

        private async Task<HttpResponseMessage> MakeOneNoteRequest(string url, HttpMethod method)
        {                  
            var message = new HttpRequestMessage(method, new Uri(url));
            var response = await _httpClient.SendRequestAsync(message);

            // For now, if we get an auth failure, only try to re-auth and make a new request once. 
            // If this starts happening a lot, maybe we can add exponential back-off or something.
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _msaAuthProvider.RestoreMostRecentFromCacheOrAuthenticateUserAsync();                               
                response = await _httpClient.SendRequestAsync(message);
            }
            return response;

        }                
    }
}
