using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FridgeShoppingList.Models
{
    public class ShoppingListEntry : INotifyPropertyChanged, IEquatable<ShoppingListEntry>
    {
        public GroceryItemType ItemType { get; set; }

        private uint _count;
        public uint Count
        {
            get { return _count; }
            set
            {
                if (_count != value)
                {
                    _count = value;
                    RaisePropertyChanged();
                }
            }
        }

        public OneNoteCheckboxNode AsOneNoteTodoItem()
        {
            return new OneNoteCheckboxNode(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public bool Equals(ShoppingListEntry other)
        {
            if((object)other == null)
            {
                return false;
            }

            return ItemType == other.ItemType
                && Count == other.Count;
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

            return Equals(obj as ShoppingListEntry);
        }

        public static bool operator ==(ShoppingListEntry a, ShoppingListEntry b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(ShoppingListEntry a, ShoppingListEntry b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 13;
                hashCode = (hashCode * 397) ^ ItemType.GetHashCode();
                hashCode = (hashCode * 397) ^ Count.GetHashCode();
                return hashCode;
            }
        }
    }
}
