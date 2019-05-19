using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using FridgeShoppingList.Models;
using FridgeShoppingList.ViewModels.ControlViewModels;
using FridgeShoppingList.Services.SettingsServices;
using System.Reactive.Linq;
using GalaSoft.MvvmLight.Command;
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
        private readonly IOneNoteService _oneNoteService;

        public ObservableCollectionExtended<InventoryEntryViewModel> InventoryItems { get; private set; } = new ObservableCollectionExtended<InventoryEntryViewModel>();
        public ObservableCollectionExtended<GroceryItemType> SavedItemTypes { get; private set; } = new ObservableCollectionExtended<GroceryItemType>();
        public ObservableCollectionExtended<ShoppingListEntryViewModel> ShoppingListItems { get; private set; } = new ObservableCollectionExtended<ShoppingListEntryViewModel>();

        private bool _isSyncInProgress = false;
        public bool IsSyncInProgress
        {
            get { return _isSyncInProgress; }
            set { Set(ref _isSyncInProgress, value); }
        }

        public RelayCommand AddItemCommand => new RelayCommand(AddItem);
        public RelayCommand AddItemTypeCommand => new RelayCommand(AddItemType);
        public RelayCommand<ShoppingListEntryViewModel> DeleteFromShoppingListCommand => new RelayCommand<ShoppingListEntryViewModel>(DeleteFromShoppingList);
        public RelayCommand<ShoppingListEntryViewModel> MoveFromShoppingToInventoryCommand => new RelayCommand<ShoppingListEntryViewModel>(MoveFromShoppingToInventory);
        public RelayCommand SyncShoppingListCommand => new RelayCommand(SyncShoppingList);

        public MainPageViewModel(SettingsService settings, IDialogService dialog, IOneNoteService oneNote)
        {
            _settings = settings;
            _dialogService = dialog;
            _oneNoteService = oneNote;

            _settings.InventoryItems
                .Transform(x => new InventoryEntryViewModel(x, _settings))
                .ObserveOnDispatcher()
                .Bind(InventoryItems)
                .Subscribe();

            _settings.GroceryTypes
                .ObserveOnDispatcher()
                .Bind(SavedItemTypes)
                .Subscribe();

            _settings.ShoppingListItems
                .Transform(x => new ShoppingListEntryViewModel(x))
                .ObserveOnDispatcher()
                .Bind(ShoppingListItems)
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
            InventoryEntry result = await _dialogService.ShowModalDialogAsync<AddToInventoryViewModel, InventoryEntry>();
            if (result != null)
            {
                _settings.AddToInventoryItems(result);
                _settings.LastLocalUpdate = DateTimeOffset.UtcNow;
            }
        }       

        public async void AddItemType()
        {
            GroceryItemType result = await _dialogService.ShowModalDialogAsync<AddGroceryItemTypeViewModel, GroceryItemType>();
            if (result != null)
            {
                _settings.AddOrEditGroceryType(result);
            }
        }       

        private void DeleteFromShoppingList(ShoppingListEntryViewModel obj)
        {
            _settings.RemoveFromShoppingListItems(obj.Entry);
            _settings.LastLocalUpdate = DateTimeOffset.UtcNow;
        }

        private async void MoveFromShoppingToInventory(ShoppingListEntryViewModel entry)
        {
            var result = await _dialogService.ShowModalDialogAsync<AddToInventoryViewModel, InventoryEntry>(entry.Entry);
            if (result != null)
            {
                _settings.RemoveFromShoppingListItems(entry.Entry);
                _settings.AddToInventoryItems(result);
                _settings.LastLocalUpdate = DateTimeOffset.UtcNow;
            }            
        }

        public async void SyncShoppingList()
        {
            IsSyncInProgress = true;

            var currentShoppingList = _settings.ShoppingListItems.AsObservableList().Items;
            bool success = await _oneNoteService.UpdateShoppingListContent(currentShoppingList.Select(x => x.AsOneNoteCheckboxNode()));
            if (success)
            {
                await Task.Delay(3000); // Kind of a hack, but we're going to give the server a moment to update itself.                
                (await _oneNoteService.GetShoppingListPageContent()).MatchSome(response =>
                {
                    var newList = response
                       .Where(x => !x.IsChecked)
                       .Select(x => x.AsShoppingListEntry())
                       .Select(x => x.ValueOr(alternative: null))
                       .Where(x => x != null);

                    _settings.ClearShoppingListItems();
                    _settings.AddToShoppingList(newList);
                });
            }

            IsSyncInProgress = false;
        }
    }
}

