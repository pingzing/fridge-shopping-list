using FridgeShoppingList.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Template10.Common;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;
using Windows.Devices.WiFi;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;

namespace FridgeShoppingList.Services
{
    public interface INetworkService
    {
        Task<bool> ConnectToNetwork(WiFiAvailableNetwork network, bool autoConnect);
        Task<bool> ConnectToNetworkWithPassword(WiFiAvailableNetwork network, bool autoConnect, PasswordCredential password);
        void DisconnectNetwork(WiFiAvailableNetwork network);
        Task<IList<WiFiAvailableNetwork>> GetAvailableWifiNetworks();
        WiFiAvailableNetwork GetCurrentWifiNetwork();
        string GetDirectConnectionName();
        string GetCurrentNetworkName();
        string GetCurrentIpv4Address();
        bool IsNetworkOpen(WiFiAvailableNetwork network);
        Task<bool> IsWifiAvailable();
        bool IsOnWiredConnection();
        Task<IList<NetworkInfo>> GetNetworkInformation();
        Task<bool> IsWifiRadioOn();
        Task EnableWifiRadio();
        Task DisableWifiRadio();
        event TypedEventHandler<Radio, object> WifiRadioStateChanged;
        event TypedEventHandler<NetworkService, AvailableNetworksChangedArgs> AvailableNetworksChanged;
    }

    public class NetworkInfo
    {
        public string NetworkName { get; set; }
        public string NetworkIpv6 { get; set; }
        public string NetworkIpv4 { get; set; }
        public string NetworkStatus { get; set; }
        public bool IsWired { get; set; }
    }

    public class NetworkService : INetworkService
    {
        private const string NoInternetConnectionString = "No internet connection.";
        private readonly static uint EthernetIanaType = 6;
        private readonly static uint WirelessInterfaceIanaType = 71;
        private static WiFiAccessStatus? AccessStatus;
        private Dictionary<String, WiFiAdapter> _wiFiAdapters = new Dictionary<string, WiFiAdapter>();
        private Dictionary<WiFiAdapter, List<WiFiAvailableNetwork>> _availableNetworks = new Dictionary<WiFiAdapter, List<WiFiAvailableNetwork>>();
        private DeviceWatcher _wiFiAdaptersWatcher;
        private Dictionary<WiFiAvailableNetwork, WiFiAdapter> _networkNameToInfo;
        private ManualResetEvent _enumAdaptersCompleted = new ManualResetEvent(false);
        private Radio _wifiRadio;

        public event TypedEventHandler<Radio, object> WifiRadioStateChanged;
        public event TypedEventHandler<NetworkService, AvailableNetworksChangedArgs> AvailableNetworksChanged;

        public NetworkService()
        {
            _wiFiAdaptersWatcher = DeviceInformation.CreateWatcher(WiFiAdapter.GetDeviceSelector());
            _wiFiAdaptersWatcher.EnumerationCompleted += AdaptersEnumCompleted;
            _wiFiAdaptersWatcher.Added += AdaptersAdded;
            _wiFiAdaptersWatcher.Removed += AdaptersRemoved;
            _wiFiAdaptersWatcher.Start();
            InitializeWifiRadio();
        }

        private async Task<bool> InitializeWifiRadio()
        {
            if (_wifiRadio != null)
            {
                return true;
            }

            var access = await Radio.RequestAccessAsync();
            if(access != RadioAccessStatus.Allowed)
            {
                return false;
            }

            var radios = await Radio.GetRadiosAsync();
            foreach (var radio in radios)
            {
                if (radio.Kind == RadioKind.WiFi)
                {
                    _wifiRadio = radio;
                    radio.StateChanged += WifiRadioStateChanged;
                    return true;
                }
            }

            return false;
        }

        private void AdaptersRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            _wiFiAdapters[args.Id].AvailableNetworksChanged -= AdapterNetworksChanged;
            _wiFiAdapters.Remove(args.Id);
        }

        private void AdaptersAdded(DeviceWatcher sender, DeviceInformation args)
        {
            _wiFiAdapters.Add(args.Id, null);
        }

        private async void AdaptersEnumCompleted(DeviceWatcher sender, object args)
        {
            List<String> WiFiAdaptersID = new List<string>(_wiFiAdapters.Keys);
            for (int i = 0; i < WiFiAdaptersID.Count; i++)
            {
                string id = WiFiAdaptersID[i];
                try
                {
                    _wiFiAdapters[id] = await WiFiAdapter.FromIdAsync(id);
                    _wiFiAdapters[id].AvailableNetworksChanged += AdapterNetworksChanged;
                }
                catch (Exception)
                {
                    _wiFiAdapters.Remove(id);
                }
            }
            _enumAdaptersCompleted.Set();
        }

        private void AdapterNetworksChanged(WiFiAdapter sender, object args)
        {
            if (!_availableNetworks.ContainsKey(sender))
            {
                //blindly add it, woooo
                var newNetworks = sender.NetworkReport.AvailableNetworks;
                _availableNetworks.Add(sender, newNetworks.ToList());
                System.Diagnostics.Debug.WriteLine($"{newNetworks.Count} networks added");
                this.AvailableNetworksChanged?.Invoke(this, new AvailableNetworksChangedArgs(newNetworks, null));
            }
            else
            {
                //remove stale networks
                var staleNetworks = _availableNetworks[sender].Except(sender.NetworkReport.AvailableNetworks, new WiFiAvailableNetworkComparer()).ToList();
                foreach(var staleNetwork in staleNetworks)
                {
                    _availableNetworks[sender].Remove(staleNetwork);
                }

                //Add new networks
                var newNetworks = sender.NetworkReport.AvailableNetworks.Except(_availableNetworks[sender], new WiFiAvailableNetworkComparer()).ToList();
                foreach(var newNetwork in newNetworks)
                {
                    _availableNetworks[sender].Add(newNetwork);
                }
                System.Diagnostics.Debug.WriteLine($"{newNetworks.Count} networks added, {staleNetworks.Count} networks removed");
                this.AvailableNetworksChanged?.Invoke(this, new AvailableNetworksChangedArgs(newNetworks.ToList(), staleNetworks));
            }
        }

        public string GetDirectConnectionName()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp != null)
            {
                if (icp.NetworkAdapter.IanaInterfaceType == EthernetIanaType)
                {
                    return icp.ProfileName;
                }
            }

            return null;
        }

        public string GetCurrentNetworkName()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp != null)
            {
                return icp.ProfileName;
            }

            return NoInternetConnectionString;
        }

        public string GetCurrentIpv4Address()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp != null && icp.NetworkAdapter != null && icp.NetworkAdapter.NetworkAdapterId != null)
            {
                var name = icp.ProfileName;

                try
                {
                    var hostnames = NetworkInformation.GetHostNames();

                    foreach (var hn in hostnames)
                    {
                        if (hn.IPInformation != null &&
                            hn.IPInformation.NetworkAdapter != null &&
                            hn.IPInformation.NetworkAdapter.NetworkAdapterId != null &&
                            hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId &&
                            hn.Type == HostNameType.Ipv4)
                        {
                            return hn.CanonicalName;
                        }
                    }
                }
                catch (Exception)
                {
                    // do nothing
                    // in some (strange) cases NetworkInformation.GetHostNames() fails... maybe a bug in the API...
                }
            }

            return NoInternetConnectionString;
        }

        // Call this method before accessing WiFiAdapters Dictionary
        private async Task UpdateAdapters()
        {
            bool fInit = false;
            foreach (var adapter in _wiFiAdapters)
            {
                if (adapter.Value == null)
                {
                    // New Adapter plugged-in which requires Initialization
                    fInit = true;
                }
            }

            if (fInit)
            {
                List<String> WiFiAdaptersID = new List<string>(_wiFiAdapters.Keys);
                for (int i = 0; i < WiFiAdaptersID.Count; i++)
                {
                    string id = WiFiAdaptersID[i];
                    try
                    {
                        _wiFiAdapters[id] = await WiFiAdapter.FromIdAsync(id);
                    }
                    catch (Exception)
                    {
                        _wiFiAdapters.Remove(id);
                    }
                }
            }
        }
        public async Task<bool> IsWifiAvailable()
        {
            if ((await TestAccess()) == false)
            {
                return false;
            }

            try
            {
                _enumAdaptersCompleted.WaitOne();
                await UpdateAdapters();
                return (_wiFiAdapters.Count > 0);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> UpdateWifiInfo()
        {
            if ((await TestAccess()) == false)
            {
                return false;
            }

            _networkNameToInfo = new Dictionary<WiFiAvailableNetwork, WiFiAdapter>();
            List<WiFiAdapter> WiFiAdaptersList = new List<WiFiAdapter>(_wiFiAdapters.Values);
            foreach (var adapter in WiFiAdaptersList)
            {
                if (adapter == null)
                {
                    return false;
                }

                try
                {
                    await adapter.ScanAsync();

                    if (adapter.NetworkReport == null)
                    {
                        continue;
                    }

                    foreach (var network in adapter.NetworkReport.AvailableNetworks)
                    {
                        if (!HasSsid(_networkNameToInfo, network.Ssid))
                        {
                            _networkNameToInfo[network] = adapter;
                        }
                    }

                    return true;
                }
                catch (OperationCanceledException)
                {
                    return false;
                }                
            }
            return false;
        }

        private bool HasSsid(Dictionary<WiFiAvailableNetwork, WiFiAdapter> resultCollection, string ssid)
        {
            foreach (var network in resultCollection)
            {
                if (!string.IsNullOrEmpty(network.Key.Ssid) && network.Key.Ssid == ssid)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<IList<WiFiAvailableNetwork>> GetAvailableWifiNetworks()
        {
            await UpdateWifiInfo();

            return _networkNameToInfo.Keys.ToList();
        }

        public WiFiAvailableNetwork GetCurrentWifiNetwork()
        {
            var connectionProfiles = NetworkInformation.GetConnectionProfiles();

            if (connectionProfiles.Count < 1)
            {
                return null;
            }

            var validProfiles = connectionProfiles.Where(profile =>
            {
                return (profile.IsWlanConnectionProfile && profile.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.None);
            });

            if (validProfiles.Count() < 1)
            {
                return null;
            }

            var firstProfile = validProfiles.First() as ConnectionProfile;

            return _networkNameToInfo.Keys.FirstOrDefault(wifiNetwork => wifiNetwork.Ssid.Equals(firstProfile.ProfileName));
        }

        public async Task<bool> ConnectToNetwork(WiFiAvailableNetwork network, bool autoConnect)
        {
            if (network == null)
            {
                return false;
            }

            var result = await _networkNameToInfo[network].ConnectAsync(network, autoConnect ? WiFiReconnectionKind.Automatic : WiFiReconnectionKind.Manual);

            return (result.ConnectionStatus == WiFiConnectionStatus.Success);
        }

        public void DisconnectNetwork(WiFiAvailableNetwork network)
        {
            _networkNameToInfo[network].Disconnect();
        }

        public bool IsNetworkOpen(WiFiAvailableNetwork network)
        {
            return network.SecuritySettings.NetworkEncryptionType == NetworkEncryptionType.None;
        }

        public async Task<bool> ConnectToNetworkWithPassword(WiFiAvailableNetwork network, bool autoConnect, PasswordCredential password)
        {
            if (network == null)
            {
                return false;
            }

            var result = await _networkNameToInfo[network].ConnectAsync(
                network,
                autoConnect ? WiFiReconnectionKind.Automatic : WiFiReconnectionKind.Manual,
                password);

            return (result.ConnectionStatus == WiFiConnectionStatus.Success);
        }

        private static async Task<bool> TestAccess()
        {
            if (!AccessStatus.HasValue)
            {
                AccessStatus = await WiFiAdapter.RequestAccessAsync();
            }

            return (AccessStatus == WiFiAccessStatus.Allowed);
        }

        public bool IsOnWiredConnection()
        {
            IReadOnlyList<ConnectionProfile> connections = NetworkInformation.GetConnectionProfiles();
            foreach (var connection in connections)
            {
                if (connection == null)
                {
                    continue;
                }

                if (connection.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.None)
                {
                    if (connection.IsWlanConnectionProfile || connection.IsWwanConnectionProfile)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<IList<NetworkInfo>> GetNetworkInformation()
        {
            var networkList = new Dictionary<Guid, NetworkInfo>();

            try
            {
                var hostNamesList = NetworkInformation.GetHostNames();

                foreach (var hostName in hostNamesList)
                {
                    if (hostName.Type == HostNameType.Ipv4 || hostName.Type == HostNameType.Ipv6)
                    {
                        NetworkInfo info = null;
                        if (hostName.IPInformation != null && hostName.IPInformation.NetworkAdapter != null)
                        {
                            var profile = await hostName.IPInformation.NetworkAdapter.GetConnectedProfileAsync();
                            if (profile != null)
                            {
                                var found = networkList.TryGetValue(hostName.IPInformation.NetworkAdapter.NetworkAdapterId, out info);
                                if (!found)
                                {
                                    info = new NetworkInfo();
                                    networkList[hostName.IPInformation.NetworkAdapter.NetworkAdapterId] = info;

                                    // NetworkAdapter API does not provide a way to tell if this is a physical adapter or virtual one; e.g. soft AP
                                    // So, provide heuristics to check for virtual network adapter
                                    if ((hostName.IPInformation.NetworkAdapter.IanaInterfaceType == WirelessInterfaceIanaType &&
                                        profile.ProfileName.Equals("Ethernet")) ||
                                        (hostName.IPInformation.NetworkAdapter.IanaInterfaceType == WirelessInterfaceIanaType &&
                                        hostName.IPInformation.NetworkAdapter.InboundMaxBitsPerSecond == 0 &&
                                        hostName.IPInformation.NetworkAdapter.OutboundMaxBitsPerSecond == 0)
                                        )
                                    {
                                        info.NetworkName = "Virtual network adapter";
                                    }
                                    else
                                    {
                                        info.NetworkName = profile.ProfileName;
                                    }
                                    var statusTag = profile.GetNetworkConnectivityLevel().ToString();
                                    info.NetworkStatus = statusTag;
                                    info.IsWired = hostName.IPInformation.NetworkAdapter.IanaInterfaceType == EthernetIanaType;
                                }
                            }
                        }

                        // No network adapter was found. So, assign the network info to a virtual adapter header
                        if (info == null)
                        {
                            info = new NetworkInfo();
                            info.NetworkName = "Virtual Network Adapter";
                            // Assign a new GUID, since we don't have a network adapter
                            networkList[Guid.NewGuid()] = info;
                            info.NetworkStatus = "Local Access";
                        }

                        if (hostName.Type == HostNameType.Ipv4)
                        {
                            info.NetworkIpv4 = hostName.CanonicalName;
                        }
                        else
                        {
                            info.NetworkIpv6 = hostName.CanonicalName;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // do nothing
                // in some (strange) cases NetworkInformation.GetHostNames() fails... maybe a bug in the API...
            }

            var res = new List<NetworkInfo>();
            res.AddRange(networkList.Values);
            return res;
        }

        public async Task<bool> IsWifiRadioOn()
        {
            if (!(await InitializeWifiRadio()))
            {
                return false;
            }

            return _wifiRadio.State == RadioState.On;
        }

        public async Task DisableWifiRadio()
        {
            if (!(await InitializeWifiRadio()))
            {
                return;
            }
            await _wifiRadio.SetStateAsync(RadioState.Off);
        }

        public async Task EnableWifiRadio()
        {
            if (!(await InitializeWifiRadio()))
            {
                return;
            }
            await _wifiRadio.SetStateAsync(RadioState.On);
        }
    }

    public class AvailableNetworksChangedArgs
    {
        public IReadOnlyList<WiFiAvailableNetwork> AddedNetworks { get; }
        public IReadOnlyList<WiFiAvailableNetwork> RemovedNetworks { get; }

        public AvailableNetworksChangedArgs(IReadOnlyList<WiFiAvailableNetwork> added, IReadOnlyList<WiFiAvailableNetwork> removed)
        {
            AddedNetworks = added;
            RemovedNetworks = removed;
        }
    }
}
