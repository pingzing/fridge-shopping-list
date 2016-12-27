using System;
using System.Collections.Generic;
using Windows.Devices.WiFi;

namespace FridgeShoppingList.Helpers
{
    public class WiFiAvailableNetworkComparer : IEqualityComparer<WiFiAvailableNetwork>
    {
        public bool Equals(WiFiAvailableNetwork x, WiFiAvailableNetwork y)
        {
            if (String.IsNullOrWhiteSpace(x.Ssid) || String.IsNullOrWhiteSpace(y.Ssid))
            {
                return x.Bssid == y.Bssid;
            }
            else
            {
                return x.Ssid == y.Ssid;
            }
        }

        public int GetHashCode(WiFiAvailableNetwork obj)
        {
            unchecked
            {
                int hashCode = obj.Ssid.GetHashCode() * 23;
                hashCode *= obj.Bssid.GetHashCode() * 23;
                return hashCode;
            }
        }
    }
}
