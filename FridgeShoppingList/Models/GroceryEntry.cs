using DynamicData.Binding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FridgeShoppingList.Models
{
    [DebuggerDisplay("Name: {ItemType.Name}, ExpiryDates Count: {ExpiryDates.Count}")]
    public class GroceryEntry : IEquatable<GroceryEntry>
    {
        public GroceryItemType ItemType { get; set; }
        //Eventually, this'll turn into some kind of list of GroceryItems if we ever need more than an expiry date 
        public ObservableCollectionExtended<DateTime> ExpiryDates { get; set; } = new ObservableCollectionExtended<DateTime>();

        public GroceryEntry()
        {
            ItemType = null;
        }

        public GroceryEntry(GroceryItemType item, DateTime singleItemExpiry)
        {
            ItemType = item;
            ExpiryDates.Add(singleItemExpiry);
        }

        public GroceryEntry(GroceryItemType item, IEnumerable<DateTime> expiryTimes)
        {
            ItemType = item;
            ExpiryDates = new ObservableCollectionExtended<DateTime>(expiryTimes);
        }

        public bool Equals(GroceryEntry other)
        {
            if((object)other == null)
            {
                return false;
            }

            return !this.ExpiryDates.Except(other.ExpiryDates).Any()
                && !other.ExpiryDates.Except(this.ExpiryDates).Any();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals(obj as GroceryEntry);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 13;
                hashCode = (hashCode * 397) ^ ItemType.GetHashCode();
                hashCode = (hashCode * 397) ^ ExpiryDates.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(GroceryEntry a, GroceryEntry b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(GroceryEntry a, GroceryEntry b)
        {
            return !(a == b);
        }
    }
}
