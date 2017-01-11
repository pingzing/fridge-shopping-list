using FridgeShoppingList.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace FridgeShoppingList.ViewModels.ControlViewModels
{
    public class ShoppingListEntryViewModel : BindableBase
    {
        //Todo: Maybe replace this with a ShoppingListEntry.
        public GroceryEntry Entry { get; set; }

        public RelayCommand AddOneCommand => new RelayCommand(AddOne);
        public RelayCommand SubtractOneCommand => new RelayCommand(SubtractOne);        

        public ShoppingListEntryViewModel(GroceryEntry entry)
        {
            Entry = entry;
        }

        private void AddOne()
        {
            Entry.ExpiryDates.Add(new DateTime());
        }

        private void SubtractOne()
        {
            if (Entry.ExpiryDates.Count > 1)
            {
                Entry.ExpiryDates.RemoveAt(0);
            }
        }
    }
}
