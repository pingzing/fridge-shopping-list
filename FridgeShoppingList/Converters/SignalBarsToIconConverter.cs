using FridgeShoppingList.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace FridgeShoppingList.Converters
{
    public class SignalBarsToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            byte bars = (byte)value;
            switch(bars)
            {
                case 4:
                    return FontIcons.WifiFourBars;
                case 3:
                    return FontIcons.WifiThreeBars;
                case 2:
                    return FontIcons.WifiTwoBars;
                case 1:
                case 0:
                default:
                    return FontIcons.WifiOneBar;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
