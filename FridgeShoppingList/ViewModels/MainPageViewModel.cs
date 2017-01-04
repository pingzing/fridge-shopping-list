using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using FridgeShoppingList.Models;
using FridgeShoppingList.ViewModels.ControlViewModels;
using FridgeShoppingList.Services.SettingsServices;
using Reactive.Bindings;
using System.Reactive.Linq;
using System.Collections.Specialized;
using GalaSoft.MvvmLight.Command;

namespace FridgeShoppingList.ViewModels
{   
    public class MainPageViewModel : ViewModelBaseEx
    {
        public readonly SettingsService _settings;

        public ReactiveCollection<InventoryEntryViewModel> InventoryItems
        {
            get
            {
                return _settings.InventoryItems
                    .ToObservable()
                    .Select(x => new InventoryEntryViewModel(x))
                    .ToReactiveCollection();       
            }
        }

        private ObservableCollection<ShoppingListEntryViewModel> _shoppingListItems = new ObservableCollection<ShoppingListEntryViewModel>();
        public ObservableCollection<ShoppingListEntryViewModel> ShoppingListItems
        {
            get { return _shoppingListItems; }
            set { Set(ref _shoppingListItems, value); }
        }

        public RelayCommand<GroceryItemType> AddItemCommand => new RelayCommand<GroceryItemType>(AddItem);
        public RelayCommand<string> AddItemTypeCommand => new RelayCommand<string>(AddItemType);

        public MainPageViewModel(SettingsService settings)
        {
            _settings = settings;
        }                

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                //restore values from suspensionState dict
            }

            await Task.CompletedTask;            
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                //store values for later restoration in suspensionState dict
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public void GotoSettings()
        {
            NavigationService.Navigate(typeof(Views.SettingsPage));
        }

        public void AddItem(GroceryItemType item)
        {
            _settings.InventoryItems.AddOnScheduler(new GroceryItem { ItemType = item, ExpiryDate = DateTime.Today });
        }

        public void AddItemType(string itemName)
        {
            _settings.GroceryTypes.AddOnScheduler(new GroceryItemType { Name = itemName, ItemTypeId = Guid.NewGuid() });
        }
    }
}

