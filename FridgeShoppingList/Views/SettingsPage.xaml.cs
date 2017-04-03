using FridgeShoppingList.Models;
using FridgeShoppingList.Services;
using FridgeShoppingList.ViewModels;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Linq;
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

        private void ItemTypesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vmSelectedItems = ViewModel.ItemTypesPartViewModel.SelectedItems;
            foreach(GroceryItemType itemType in e.RemovedItems.OfType<GroceryItemType>())
            {
                if (vmSelectedItems.Contains(itemType))
                {
                    vmSelectedItems.Remove(itemType);
                }
            }

            foreach(GroceryItemType itemType in e.AddedItems.OfType<GroceryItemType>())
            {
                if (!vmSelectedItems.Contains(itemType))
                {
                    vmSelectedItems.Add(itemType);
                }
            }
        }
    }
}
