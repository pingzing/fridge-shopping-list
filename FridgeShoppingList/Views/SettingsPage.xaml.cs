using FridgeShoppingList.Services;
using FridgeShoppingList.ViewModels;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FridgeShoppingList.Views
{
    public sealed partial class SettingsPage : Page
    {
        Template10.Services.SerializationService.ISerializationService _SerializationService;

        private SettingsPageViewModel _viewModel;
        public SettingsPageViewModel ViewModel => _viewModel ?? (_viewModel = (SettingsPageViewModel)DataContext);

        public SettingsPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            _SerializationService = Template10.Services.SerializationService.SerializationService.Json;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void WifiButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            (DataContext as SettingsPageViewModel)?.OpenNetworkConfigCommand?.Execute(null);
        }        
    }
}
