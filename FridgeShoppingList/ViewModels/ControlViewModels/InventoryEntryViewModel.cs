using FridgeShoppingList.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace FridgeShoppingList.ViewModels.ControlViewModels
{
    public class InventoryEntryViewModel : BindableBase
    {
        public ObservableCollection<GroceryItem> Items { get; set; }

        public InventoryEntryViewModel(GroceryItem item)
        {
            Items = new ObservableCollection<GroceryItem>(); 
            Items.Add(item);
        }

        public InventoryEntryViewModel(IEnumerable<GroceryItem> items)
        {
            Items = new ObservableCollection<GroceryItem>(items);
        }
    }
}
