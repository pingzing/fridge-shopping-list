using System;

namespace FridgeShoppingList.Models
{
    public class GroceryItemType : IEquatable<GroceryItemType>
    {
        public string Name { get; set; }
        public Guid ItemTypeId { get; set; }

        public GroceryItemType()
        {
            Name = null;
            ItemTypeId = Guid.Empty;
        }

        public GroceryItemType(string name)
        {
            Name = name;
            ItemTypeId = Guid.NewGuid();
        }

        public bool Equals(GroceryItemType other)
        {            
            return ItemTypeId == other?.ItemTypeId;
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

            return Equals(obj as GroceryItemType);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 13;                
                hashCode = (hashCode * 397) ^ ItemTypeId.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(GroceryItemType a, GroceryItemType b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(GroceryItemType a, GroceryItemType b)
        {
            return !(a == b);
        }
    }
}
