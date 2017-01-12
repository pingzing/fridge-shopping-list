using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FridgeShoppingList.Models
{    
    /// <summary>
    /// A wrapper for DateTimeOffsets, because binding to them directly in lists seems to not propagate changes correctly.
    /// </summary>
    [DebuggerDisplay("{DateTimeOffset.ToString()}")]
    public class DateTimeOffsetWrapper : INotifyPropertyChanged
    {
        private DateTimeOffset _dateTimeOffset;
        public DateTimeOffset DateTimeOffset
        {
            get { return _dateTimeOffset; }
            set
            {
                if(_dateTimeOffset != value)
                {
                    _dateTimeOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
