using FridgeShoppingList.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace FridgeShoppingList.ViewModels.ControlViewModels
{
    public class AddGroceryItemTypeViewModel : BindableBase, IResultDialogViewModel<GroceryItemType>
    {
        public GroceryItemType Result { get; set; }

        public bool IsConfirmButtonEnabled => !String.IsNullOrWhiteSpace(ItemName);

        string itemName = default(string);
        public string ItemName
        {
            get { return itemName; }
            set
            {
                Set(ref itemName, value);
                RaisePropertyChanged(nameof(IsConfirmButtonEnabled));
            }
        }

        public AddGroceryItemTypeViewModel(GroceryItemType typeToEdit)
        {
            if (typeToEdit != null)
            {
                ItemName = typeToEdit.Name;
                Result = typeToEdit;
            }
        }

        public void SetResultToCurrentState()
        {
            if (Result == null)
            {
                Result = new GroceryItemType(ItemName);
            }
            else // We're editing, and not adding, in this case...
            {
                Result.Name = ItemName;                
            }
        }
    }
}
