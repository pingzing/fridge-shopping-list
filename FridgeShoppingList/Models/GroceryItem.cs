using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FridgeShoppingList.Models
{
    [DebuggerDisplay("Name: {Name}, Expires: {ExpiryDate?.ToString()}")]
    public class GroceryItem
    {
        public GroceryItemType ItemType { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
