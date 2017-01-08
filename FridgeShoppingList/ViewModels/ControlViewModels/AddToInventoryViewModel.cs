using DynamicData;
using DynamicData.Binding;
using FridgeShoppingList.Models;
using FridgeShoppingList.Services.SettingsServices;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace FridgeShoppingList.ViewModels.ControlViewModels
{
    public class AddToInventoryViewModel : BindableBase, IResultDialogViewModel<GroceryEntry>
    {
        private static readonly SettingsService _settings;

        public GroceryEntry Result { get; set; }

        public ObservableCollectionExtended<GroceryItemType> ItemTypes { get; private set; } = new ObservableCollectionExtended<GroceryItemType>();
        public ObservableCollectionExtended<DateTime> ExpiryDates { get; private set; } = new ObservableCollectionExtended<DateTime>() { DateTime.Today };

        GroceryItemType _selectedItemType = default(GroceryItemType);
        public GroceryItemType SelectedItemType
        {
            get { return _selectedItemType; }
            set { Set(ref _selectedItemType, value); }
        }

        bool areDatesLinked = default(bool);
        public bool AreDatesLinked
        {
            get { return areDatesLinked; }
            set { Set(ref areDatesLinked, value); }
        }

        static AddToInventoryViewModel()
        {
            _settings = SimpleIoc.Default.GetInstance<SettingsService>();
        }

        public AddToInventoryViewModel()
        {
            _settings.GroceryTypes
                .ObserveOnDispatcher()
                .Bind(ItemTypes)
                .Subscribe();

            SelectedItemType = _settings.GroceryTypes.AsObservableList().Items.FirstOrDefault();            
        }

        public void SubtractOne()
        {
            if (ExpiryDates.Count > 1)
            {
                ExpiryDates.RemoveAt(ExpiryDates.Count - 1);
            }
        }

        public void AddOne()
        {
            if (ExpiryDates.Count < int.MaxValue)
            {
                ExpiryDates.Add(DateTime.Today);
            }
        }

        public void SetResultToCurrentState()
        {
            Result = new GroceryEntry(SelectedItemType, ExpiryDates);
        }
    }
}
