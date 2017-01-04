using FridgeShoppingList.Models;
using Reactive.Bindings;
using System.Reactive;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Template10.Common;
using Template10.Utils;
using Windows.UI.Xaml;

namespace FridgeShoppingList.Services.SettingsServices
{
    public class SettingsService
    {
        public static SettingsService Instance { get; } = new SettingsService();
        Template10.Services.SettingsService.ISettingsHelper _helper;

        public ReactiveCollection<GroceryItemType> GroceryTypes { get; set; }

        public ReactiveCollection<GroceryItem> InventoryItems { get; set; }

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

            GroceryTypes = _helper.Read(nameof(GroceryTypes), new ReactiveCollection<GroceryItemType>());
            GroceryTypes.ToCollectionChanged()
                .Throttle(TimeSpan.FromSeconds(30))
                .Subscribe(
                    onNext: x =>
                    {
                        _helper.Write(nameof(GroceryTypes), GroceryTypes);
                    }
                );

            InventoryItems = _helper.Read(nameof(InventoryItems), new ReactiveCollection<GroceryItem>());
            InventoryItems.ToCollectionChanged()
                .Throttle(TimeSpan.FromSeconds(30))
                .Subscribe(
                    onNext: x =>
                    {
                        _helper.Write(nameof(InventoryItems), InventoryItems);
                    }
                );
        }
    }
}

