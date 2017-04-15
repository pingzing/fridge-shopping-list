using FridgeShoppingList.Models;
using FridgeShoppingList.Services;
using FridgeShoppingList.ViewModels;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
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

        private SolidColorBrush _mostlyTransparentWhiteBrush = new SolidColorBrush { Color = Colors.White, Opacity = 0.1 };
        private SolidColorBrush _transparentBrush = new SolidColorBrush(Colors.Transparent);
        private void ItemTypesList_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.ItemIndex % 2 != 0)
            {
                args.ItemContainer.Background = _mostlyTransparentWhiteBrush;
            }
            else
            {
                args.ItemContainer.Background = _transparentBrush;
            }
        }
    }
}
