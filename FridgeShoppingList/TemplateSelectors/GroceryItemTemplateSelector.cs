using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FridgeShoppingList.TemplateSelectors
{
    public class GroceryItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate GroupedItemTemplate { get; set; }
        public DataTemplate SingleItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            //if (isSingleItem) //whatever
            //if (isMultiItem) //whatever
            if(item is string)
            {
                return SingleItemTemplate;
            }

            return base.SelectTemplateCore(item);
        }
    }
}
