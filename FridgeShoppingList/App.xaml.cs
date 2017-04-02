using Windows.UI.Xaml;
using System.Threading.Tasks;
using FridgeShoppingList.Services.SettingsServices;
using Windows.ApplicationModel.Activation;
using Template10.Controls;
using Template10.Common;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
using Template10.Services.NavigationService;
using FridgeShoppingList.Views;
using GalaSoft.MvvmLight.Ioc;
using FridgeShoppingList.ViewModels;
using FridgeShoppingList.Services;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI;
using TCD.Controls.Keyboard;

namespace FridgeShoppingList
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    [Bindable]
    sealed partial class App : BootStrapper
    {
        private Splash _splashView;

        public ServiceRegistrar Registrar { get; set; }

        public App()
        {
            InitializeComponent();
            SplashFactory = (e) =>
            {
                _splashView = new Splash(e);
                return _splashView;
            };

            Registrar = new ServiceRegistrar();            

            // some settings must be set in app.constructor
            var settings = SettingsService.Instance;
            RequestedTheme = ApplicationTheme.Dark;
            CacheMaxDuration = settings.CacheMaxDuration;
            ShowShellBackButton = false;
            AutoSuspendAllFrames = true;
            AutoRestoreAfterTerminated = true;
            AutoExtendExecutionSession = true;
        }

        public override UIElement CreateRootElement(IActivatedEventArgs e)
        {
            var service = NavigationServiceFactory(BackButton.Attach, ExistingContent.Exclude);
            var rootElement = new ModalDialog
            {
                DisableBackButtonWhenModal = true,
                Content = service.Frame,
                ModalContent = new Views.Busy(),
                ModalBackground = new SolidColorBrush(Colors.Transparent)
            };            

            return rootElement;            
        }        

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // TODO: add your long-running task here      
            await _splashView.SplashInProgress;
            await NavigationService.NavigateAsync(typeof(Views.MainPage));            
        }

        //Handles resolution for pages' ViewModels.
        public override INavigable ResolveForPage(Page page, NavigationService navigationService)
        {
            if(page is MainPage)
            {
                return SimpleIoc.Default.GetInstance<MainPageViewModel>();
            }
            else if(page is SettingsPage)
            {
                return SimpleIoc.Default.GetInstance<SettingsPageViewModel>();
            }            
            else
            {
                return base.ResolveForPage(page, navigationService);
            }
        }
    }
}

