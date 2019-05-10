using FridgeShoppingList.Helpers;
using FridgeShoppingList.Models;
using FridgeShoppingList.Services.SettingsServices;
using Microsoft.Identity.Client;
using Microsoft.Toolkit.Uwp;
using Optional;
using Optional.Unsafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using MSGraph = Microsoft.Graph;

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
        private const string ShoppingListPageFilter = "tolower(title) eq 'shopping list'";
        private const string OneDriveRedirectUri = "https://login.live.com/oauth20_desktop.srf";
        private static readonly string[] ControlAppPagesScopes = new string[] { "Notes.ReadWrite" };
        private const string AzureClientID = "84f0ce39-3578-4586-87ed-741a0d00f5ae";
        private const string CheckboxDivId = "checkbox-div";

        private static readonly HttpClient _httpClient;
        private static readonly IPublicClientApplication _authClient;
        private readonly SettingsService _settingsService;
        private static MSGraph.GraphServiceClient _graphClient;

        private string CachedPageId => _settingsService.OneNotePageId;
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

        static OneNoteService()
        {
            var httpBaseFilter = new HttpBaseProtocolFilter();
            httpBaseFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
            _httpClient = new HttpClient(httpBaseFilter);
            _authClient = PublicClientApplicationBuilder.Create(AzureClientID).Build();
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

                _authToken = await _authClient.AcquireTokenSilent(ControlAppPagesScopes, accounts.First()).ExecuteAsync();
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
            }
        }

        public async Task<Option<IEnumerable<OneNoteCheckboxNode>>> GetShoppingListPageContent()
        {
            if (!_initialized) { await Initialize(); }
            Option<MSGraph.OnenotePage> page = await GetFooddPage();
            // if this is None, go and create the page
            if (!page.HasValue)
            {
                // create the page
            }

            page.ValueOrFailure().Content

            return Option.None<IEnumerable<OneNoteCheckboxNode>>();
        }

        public async Task<bool> UpdateShoppingListContent(IEnumerable<OneNoteCheckboxNode> localList)
        {
            if (!_initialized) { await Initialize(); }
            throw new NotImplementedException();
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

            return allPages
                // We SHOULD be able to do the parentSection/name filter in the query up above, but the filtering
                // OData syntax doesn't seem to support subproperties anymore. So, do it in code.
                .SelectMany(x => x.CurrentPage)
                .FirstOrDefault(x => x.ParentSection.DisplayName.ToLowerInvariant() == "foodd")
                .SomeNotNull();
        }
    }
}