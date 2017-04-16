using System;
using System.ComponentModel;

namespace FridgeShoppingList.Models
{
    public class GroceryItemType : INotifyPropertyChanged, IEquatable<GroceryItemType>
    {
        private const uint NumOfSamplesToAverage = 4;

        public uint AverageDaysTillExpiry { get; private set; }
        public Guid ItemTypeId { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }        

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

        /// <summary>
        /// Update the average days an item takes to expire with a new sample.
        /// </summary>
        /// <param name="daysInTheFuture"></param>
        public void UpdateAverageExpiryTime(params uint[] daysInTheFuture)
        {
            foreach (uint sample in daysInTheFuture)
            {
                AverageDaysTillExpiry -= AverageDaysTillExpiry / NumOfSamplesToAverage;
                AverageDaysTillExpiry += sample / NumOfSamplesToAverage;
            }
        }        

        public event PropertyChangedEventHandler PropertyChanged;

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
