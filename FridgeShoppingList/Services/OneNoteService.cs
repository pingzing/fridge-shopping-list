using FridgeShoppingList.Models;
using OD = Microsoft.OneDrive.Sdk;
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

namespace FridgeShoppingList.Services
{
    public interface IOneNoteService
    {
        event EventHandler<bool> ConnectedStatusChanged;
        bool ConnectedStatus { get; }
        Task<List<OneNoteCheckboxNode>> GetShoppingListPageContent();
    }

    public class OneNoteService : IOneNoteService
    {
        private const string OneNoteBaseUrl = "https://www.onenote.com/api/v1.0/me/";
        private const string ShoppingListPageSearchString = OneNoteBaseUrl + "notes/pages?filter=tolower(title)%20eq%20'shopping%20list'%20and%20tolower(parentSection%2Fname)%20eq%20'foodd'";
        private const string ControlAppPagesScope = "office.onenote_update_by_app";

        private readonly HttpClient _httpClient;
        private Task _initTask;
        private OD.OnlineIdAuthenticationProvider _msaAuthProvider;
        private string _fooddPageId;

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

        public OneNoteService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue(Constants.JsonApplicationMediaType));
            _initTask = InitializeAsync();
        }

        bool _initialized;
        private async Task InitializeAsync()
        {
            if (!_initialized)
            {                
                _msaAuthProvider = new OD.OnlineIdAuthenticationProvider(new string[] { ControlAppPagesScope });
                await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                    await _msaAuthProvider.RestoreMostRecentFromCacheOrAuthenticateUserAsync());
                if (!_msaAuthProvider.IsAuthenticated)
                {
                    return;
                }
                ConnectedStatus = true;
                _httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", _msaAuthProvider.CurrentAccountSession.AccessToken);
                bool success = await CreateShoppingListPage();
                _initialized = true;
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
        /// Retrieves the HTML content of the shopping list page, if successful. If unsuccessful, returns null.
        /// </summary>
        /// <returns></returns>
        public async Task<List<OneNoteCheckboxNode>> GetShoppingListPageContent()
        {
            await _initTask;

            var response = await _httpClient.GetAsync(new Uri($"{OneNoteBaseUrl}notes/pages/{_fooddPageId}/content?includeIDs=true"));
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
                        .ToList();
                }
                return null; 
            }
            else
            {
                return null;
            }
        }
    }
}
