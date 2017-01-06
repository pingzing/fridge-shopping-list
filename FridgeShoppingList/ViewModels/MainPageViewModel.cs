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
using FridgeShoppingList.Services;
using Template10.Mvvm;

namespace FridgeShoppingList.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly SettingsService _settings;
        private readonly IDialogService _dialogService;

        public ObservableCollectionExtended<InventoryEntryViewModel> InventoryItems { get; private set; } = new ObservableCollectionExtended<InventoryEntryViewModel>();
        public ObservableCollectionExtended<GroceryItemType> SavedItemTypes { get; private set; } = new ObservableCollectionExtended<GroceryItemType>();
        
        public RelayCommand<string> AddItemTypeCommand => new RelayCommand<string>(AddItemType);

        IObservable<IChangeSet<IGroup<GroceryEntry, Guid>>> groups;

        public MainPageViewModel(SettingsService settings, IDialogService dialog)
        {
            _settings = settings;
            _dialogService = dialog;

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

        public async void AddItem()
        {
            GroceryEntry result = await _dialogService.ShowDialogAsync<AddToInventoryViewModel, GroceryEntry>();
            if(result != null)
            {
                _settings.AddToInventoryItems(result);
            }            
        }

        public void AddItemType(string itemName)
        {
            _settings.AddToGroceryTypes(new GroceryItemType { Name = itemName, ItemTypeId = Guid.NewGuid() });
        }
    }
}

