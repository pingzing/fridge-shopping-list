using FridgeShoppingList.Models;
using System.Linq;
using System.Reactive;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Template10.Common;
using Template10.Utils;
using Windows.UI.Xaml;
using DynamicData;

namespace FridgeShoppingList.Services.SettingsServices
{
    public class SettingsService
    {
        public static SettingsService Instance { get; } = new SettingsService();
        Template10.Services.SettingsService.ISettingsHelper _helper;

        private SourceList<GroceryItemType> _groceryTypes { get; set; } = new SourceList<GroceryItemType>();
        public IObservable<IChangeSet<GroceryItemType>> GroceryTypes { get; }

        private SourceList<GroceryEntry> _inventoryItems { get; set; } = new SourceList<GroceryEntry>();
        public IObservable<IChangeSet<GroceryEntry>> InventoryItems { get; }

        private SourceList<GroceryEntry> _shoppingListItems { get; set; } = new SourceList<GroceryEntry>();
        public IObservable<IChangeSet<GroceryEntry>> ShoppingListItems { get; }


        public TimeSpan CacheMaxDuration
        {
            get { return _helper.Read(nameof(CacheMaxDuration), TimeSpan.FromDays(2)); }
            set
            {
                _helper.Write(nameof(CacheMaxDuration), value);
                BootStrapper.Current.CacheMaxDuration = value;
            }
        }

        public ImmutableDictionary<string, bool> SsidToAutoConnect
        {
            get { return _helper.Read(nameof(SsidToAutoConnect), ImmutableDictionary<string, bool>.Empty); }
            set { _helper.Write(nameof(SsidToAutoConnect), value); }
        }

        private SettingsService()
        {
            _helper = new Template10.Services.SettingsService.SettingsHelper();

            //Initialize GroceryItemTypes
            GroceryItemType[] savedGroceryTypes = _helper.Read(nameof(GroceryTypes), new GroceryItemType[]
            {
                new GroceryItemType("Milk, 1L"),
                new GroceryItemType("Bread"),
                new GroceryItemType("Cheese"),
                new GroceryItemType("Ketchup"),
                new GroceryItemType("Weiner schnitzel"),
                new GroceryItemType("Juice"),
                new GroceryItemType("Herb tomato pureé")
            });
            _groceryTypes.AddRange(savedGroceryTypes);
            GroceryTypes = _groceryTypes.Connect();

            //Initialize GroceryItems
            GroceryEntry[] savedInventory = _helper.Read(nameof(InventoryItems), new GroceryEntry[0]);
            _inventoryItems.AddRange(savedInventory);
            InventoryItems = _inventoryItems.Connect();

            //Initialize ShoppingListItems
            GroceryEntry[] savedShoppingList = _helper.Read(nameof(ShoppingListItems), new GroceryEntry[0]);
            _shoppingListItems.AddRange(savedShoppingList);
            ShoppingListItems = _shoppingListItems.Connect();

            //Write changes to Grocery Types 30 seconds after changes stop coming in
            GroceryTypes.Throttle(TimeSpan.FromSeconds(30))
                .Subscribe(
                    onNext: _ => _helper.Write(nameof(GroceryTypes), _groceryTypes.Items)
                );

            //Write changes to Inventory Items to disk 30 seconds after changes stop coming in
            InventoryItems.Throttle(TimeSpan.FromSeconds(30))
                .Subscribe(
                    onNext: _ => _helper.Write(nameof(InventoryItems), _inventoryItems.Items)
                );

            //Write changed to the Shopping List items to disk 30 seconds after changes stop coming in
            ShoppingListItems.Throttle(TimeSpan.FromSeconds(30))
                .Subscribe(
                    onNext: _ => _helper.Write(nameof(ShoppingListItems), _shoppingListItems.Items)
                );
        }

        public void AddToGroceryTypes(GroceryItemType newType)
        {
            _groceryTypes.Add(newType);
        }

        public void RemoveFromGroceryTypes(GroceryItemType typeToRemove)
        {
            _groceryTypes.Remove(typeToRemove);
        }

        public void AddToInventoryItems(GroceryEntry item)
        {
            var existingEntry = _inventoryItems.Items.FirstOrDefault(x => x.ItemType == item.ItemType);
            if (existingEntry != null)
            {
                existingEntry.ExpiryDates.AddRange(item.ExpiryDates);
            }
            else
            {
                _inventoryItems.Add(item);
            }
        }

        public void RemoveFromInventoryItems(GroceryEntry itemToRemove)
        {
            _inventoryItems.Remove(itemToRemove);
        }
    }
}

