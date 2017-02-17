using Microsoft.OneDrive.Sdk;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace FridgeShoppingList.Services
{
    public interface IOneNoteService
    {        
        Task GetPages();
    }

    public class OneNoteService : IOneNoteService
    {
        private const string OneNoteBaseUrl = "https://www.onenote.com/api/v1.0/me/";
        private readonly HttpClient _httpClient;
        private Task _initTask;
        private OnlineIdAuthenticationProvider _msaAuthProvider;

        public OneNoteService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));
            _initTask = InitializeAsync();
        }

        bool _initialized;
        private async Task InitializeAsync()
        {
            if (!_initialized)
            {                
                _msaAuthProvider = new OnlineIdAuthenticationProvider(new string[] { "office.onenote_update_by_app" });
                await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                    await _msaAuthProvider.RestoreMostRecentFromCacheOrAuthenticateUserAsync());
                _httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", _msaAuthProvider.CurrentAccountSession.AccessToken);
                await CreateShoppingListPage();
                _initialized = true;
            }
        }

        private async Task CreateShoppingListPage()
        {
            var sectionsResponse = await _httpClient.GetAsync(new Uri($"{OneNoteBaseUrl}notes/pages?filter=tolower(title)%20eq%20'shopping%20list'%20and%20tolower(parentSection%2Fname)%20eq%20'foodd'"));

            //todo: check if there is a section named FOODD with a page named Shopping List. If so, don't create a new page
            // the page doesn't exist if the response only returns the odata context and nothing else.
            // Although it seems to take a long time for a page to be deleted properly? 
                                   
            string shoppingListPageHtml = (@"<!DOCTYPE html>
<html>
    <head>
        <title>Shopping List</title>
        <meta name=""created"" content=" + DateTime.Now.ToString("o") + @" />
    </head>
    <body></body>
</html>");


            var response = await _httpClient.PostAsync(new Uri($"{OneNoteBaseUrl}notes/pages?sectionName=FOODD"), 
                new HttpStringContent(shoppingListPageHtml, Windows.Storage.Streams.UnicodeEncoding.Utf8, "text/html"));
        }

        public async Task GetPages()
        {
            await _initTask;

            var response = await _httpClient.GetAsync(new Uri($"{OneNoteBaseUrl}notes/pages"));
            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine(await response.Content.ReadAsStringAsync());
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GetPages failed: HTTP " + response.ReasonPhrase + ": " + await response.Content.ReadAsStringAsync());
            }
        }
    }
}
