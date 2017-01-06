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
using System.Reactive.Linq;
using System.Collections.Specialized;
using GalaSoft.MvvmLight.Command;
using Reactive.Bindings;
using DynamicData;
using DynamicData.Binding;

namespace FridgeShoppingList.ViewModels
{
    public class MainPageViewModel : ViewModelBaseEx
    {
        private readonly SettingsService _settings;

        public ObservableCollectionExtended<InventoryEntryViewModel> InventoryItems { get; private set; } = new ObservableCollectionExtended<InventoryEntryViewModel>();
        public ObservableCollectionExtended<GroceryItemType> SavedItemTypes { get; private set; } = new ObservableCollectionExtended<GroceryItemType>();

        public RelayCommand<GroceryItemType> AddItemCommand => new RelayCommand<GroceryItemType>(AddItem);
        public RelayCommand<string> AddItemTypeCommand => new RelayCommand<string>(AddItemType);

        IObservable<IChangeSet<IGroup<GroceryEntry, Guid>>> groups;

        public MainPageViewModel(SettingsService settings)
        {
            _settings = settings;

            _settings.InventoryItems
                .Transform(x => new InventoryEntryViewModel(x))                
                .ObserveOnDispatcher()
                .Bind(InventoryItems)
                .Subscribe();            

            _settings.GroceryTypes
                .ObserveOnDispatcher()
                .Bind(SavedItemTypes)
                .Subscribe();
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
            _settings.AddToInventoryItems(new GroceryEntry(item, DateTime.Now));
        }

        public void AddItemType(string itemName)
        {
            _settings.AddToGroceryTypes(new GroceryItemType { Name = itemName, ItemTypeId = Guid.NewGuid() });
        }
    }
}

