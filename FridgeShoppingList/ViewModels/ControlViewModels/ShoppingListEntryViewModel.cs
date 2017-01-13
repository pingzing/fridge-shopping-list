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
        public ShoppingListEntry Entry { get; set; }

        public RelayCommand AddOneCommand => new RelayCommand(AddOne);
        public RelayCommand SubtractOneCommand => new RelayCommand(SubtractOne);        

        public ShoppingListEntryViewModel(ShoppingListEntry entry)
        {
            Entry = entry;
        }

        private void AddOne()
        {
            if(Entry.Count < uint.MaxValue)
            {
                Entry.Count += 1;
            }
        }

        private void SubtractOne()
        {
            if (Entry.Count > 1)
            {
                Entry.Count -= 1;
            }
        }
    }
}
