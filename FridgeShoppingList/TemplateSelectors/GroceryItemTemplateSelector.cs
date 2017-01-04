using FridgeShoppingList.ViewModels.ControlViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FridgeShoppingList.TemplateSelectors
{
    public class InventoryItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate GroupedItemTemplate { get; set; }
        public DataTemplate SingleItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            InventoryEntryViewModel groceryVm = item as InventoryEntryViewModel;
            if (groceryVm != null)
            {
                if(groceryVm.Items.Count > 1)
                {
                    return GroupedItemTemplate;
                }
                else
                {
                    return SingleItemTemplate;
                }
            }

            return base.SelectTemplateCore(item);
        }
    }
}
