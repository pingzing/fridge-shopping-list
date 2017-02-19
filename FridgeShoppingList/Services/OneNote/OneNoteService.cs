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
using Microsoft.OneDrive.Sdk;

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
        private const string GetAccessTokenUrl = "https://login.live.com/oauth20_token.srf";
        private const string ShoppingListPageSearchString = OneNoteBaseUrl + "notes/pages?filter=tolower(title)%20eq%20'shopping%20list'%20and%20tolower(parentSection%2Fname)%20eq%20'foodd'";
        private const string OneDriveRedirectUri = "https://login.live.com/oauth20_desktop.srf";
        private const string ControlAppPagesScopes = "office.onenote_update_by_app%20wl.offline_access";
        private const string _oneDriveClientId = "00000000481CBFE3";

        private readonly IDialogService _dialogService;
        private readonly HttpClient _httpClient;
        private Task _initTask;
        
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

        public OneNoteService(IDialogService dialogService)
        {
            _dialogService = dialogService;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue(Constants.JsonApplicationMediaType));            
        }

        bool _initialized;        
        private async Task Initialize()
        {

            if (!_initialized)
            {
                string authUrl = $"https://login.live.com/oauth20_authorize.srf?response_type=code&client_id={_oneDriveClientId}&redirect_uri={OneDriveRedirectUri}&scope={ControlAppPagesScopes}";
                string redemptionCode = await _dialogService.ShowModalDialogAsync<LoginToOneNoteViewModel, string>(authUrl);
                var response = await _httpClient.PostAsync(
                    new Uri(GetAccessTokenUrl),
                    new HttpStringContent(
                        $"grant_type=authorization_code&client_id={_oneDriveClientId}&client_secret=<FIND A SECURE WAY TO BRING IN THE CLIENT SECRET HERE YO>&code={redemptionCode}&redirect_uri={OneDriveRedirectUri}",
                        UnicodeEncoding.Utf8,
                        Constants.UrlEncodedFormMediaType)
                    );

                ConnectedStatus = true;
                _httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", "");
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
        /// Retrieves the HTML content of the shopping list page, if successful. If unsuccessful, returns null.
        /// </summary>
        /// <returns></returns>
        public async Task<List<OneNoteCheckboxNode>> GetShoppingListPageContent()
        {
            await Initialize();

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
