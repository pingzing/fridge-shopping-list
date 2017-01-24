using DynamicData;
using DynamicData.Binding;
using FridgeShoppingList.Models;
using FridgeShoppingList.Services.SettingsServices;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Controls;

namespace FridgeShoppingList.ViewModels.ControlViewModels
{
    public class AddToInventoryViewModel : BindableBase, IResultDialogViewModel<InventoryEntry>
    {
        private static readonly SettingsService _settings;

        public InventoryEntry Result { get; set; }

        public ObservableCollectionExtended<GroceryItemType> ItemTypes { get; private set; } = new ObservableCollectionExtended<GroceryItemType>();
        public ObservableCollectionExtended<DateTimeOffsetWrapper> ExpiryDates { get; private set; } 
            = new ObservableCollectionExtended<DateTimeOffsetWrapper>()
            {
                new DateTimeOffsetWrapper { DateTimeOffset = DateTime.Today }
            };

        GroceryItemType _selectedItemType = default(GroceryItemType);
        public GroceryItemType SelectedItemType
        {
            get { return _selectedItemType; }
            set
            {
                Set(ref _selectedItemType, value);
                RaisePropertyChanged(nameof(IsAddButtonEnabled));
            }
        }

        private int _selectedCountIndex = 0;
        public int SelectedCountIndex
        {
            get { return _selectedCountIndex; }
            set { Set(ref _selectedCountIndex, value); }
        }

        bool areDatesLinked = default(bool);
        public bool AreDatesLinked
        {
            get { return areDatesLinked; }
            set { Set(ref areDatesLinked, value); }
        }

        public bool IsAddButtonEnabled => SelectedItemType != null && ItemTypes.Count > 0;

        static AddToInventoryViewModel()
        {
            _settings = SimpleIoc.Default.GetInstance<SettingsService>();
        }

        public AddToInventoryViewModel(object args)
        {
            _settings.GroceryTypes           
                .Sort(SortExpressionComparer<GroceryItemType>.Ascending(x => x.Name))
                .ObserveOnDispatcher()                
                .Bind(ItemTypes)
                .Subscribe();

            DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
            await Task.Delay(200);

            ShoppingListEntry entry = args as ShoppingListEntry;
            if (entry == null)
            {
                SelectedItemType = _settings.GroceryTypes
                    .AsObservableList()
                    .Items
                    .OrderBy(x => x.Name)
                    .FirstOrDefault();
            }
            else
            {
                SelectedItemType = _settings.GroceryTypes
                    .AsObservableList()
                    .Items
                    .Where(x => x.ItemTypeId == entry.ItemType.ItemTypeId)
                    .FirstOrDefault();
                
                    SelectedCountIndex = entry.Count < 12 ? (int)entry.Count - 1 : 11;
                }
            });            
        }

        public void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int value = Convert.ToInt32(((GridViewItem)e.AddedItems.FirstOrDefault()).Content);
            var dateTimes = new List<DateTimeOffsetWrapper>(value);
            for (int i = 0; i < value; i++)
            {
                dateTimes.Add(new DateTimeOffsetWrapper { DateTimeOffset = DateTime.Today });
            }

            using (ExpiryDates.SuspendCount())
            using (ExpiryDates.SuspendNotifications())
            {
                ExpiryDates.Clear();
                ExpiryDates.AddRange(dateTimes);
            }
        }        

        public void SetResultToCurrentState()
        {
            if (AreDatesLinked)
            {
                Result = new InventoryEntry(SelectedItemType, 
                    Enumerable.Repeat(ExpiryDates.First().DateTimeOffset.DateTime, SelectedCountIndex + 1));
            }
            else
            {
                Result = new InventoryEntry(SelectedItemType, ExpiryDates.Select(x => x.DateTimeOffset.DateTime));
            }
        }
    }
}
