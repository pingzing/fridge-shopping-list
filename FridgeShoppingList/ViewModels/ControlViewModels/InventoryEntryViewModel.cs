using FridgeShoppingList.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Mvvm;

namespace FridgeShoppingList.ViewModels.ControlViewModels
{
    public class InventoryEntryViewModel : BindableBase
    {
        public InventoryEntry Entry { get; set; }

        public InventoryEntryViewModel(InventoryEntry entry)
        {
            Entry = entry;
        }
    }
}
