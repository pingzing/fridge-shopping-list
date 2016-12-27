using FridgeShoppingList.Models;
using FridgeShoppingList.Services;
using FridgeShoppingList.Services.SettingsServices;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using Windows.UI.Xaml;

namespace FridgeShoppingList.ViewModels.ControlViewModel
{
    public class WifiItemViewModel : BindableBase
    {        
        private const string ConnectedNetworkStateString = "Connected";
        private const string OpenNetworkStateString = "Open";
        private const string SecuredNetworkStateString = "Secured";

        private readonly SettingsService _settingsService;
        private readonly INetworkService _networkService;

        private string _bssid;
        private WiFiAvailableNetwork _backingNetwork;

        private string _networkName;
        public string NetworkName
        {
            get { return _networkName; }
            set { Set(ref _networkName, value); }
        }

        private string _networkGlyph;
        public string NetworkGlyph
        {
            get { return _networkGlyph; }
            set { Set(ref _networkGlyph, value); }
        }

        private string _networkState;
        public string NetworkState
        {
            get { return _networkState; }
            set { Set(ref _networkState, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                Set(ref _isSelected, value);
                RaisePropertyChanged(nameof(IsPasswordBoxVisible));             
            }
        }        

        private bool _isAutoConnect;
        public bool IsAutoConnect
        {
            get { return _isAutoConnect; }
            set
            {
                Set(ref _isAutoConnect, value);
                _settingsService.SsidToAutoConnect = _settingsService.SsidToAutoConnect.Add(NetworkName, value);
            }
        }

        private string _changeConnectButtonText;
        public string ChangeConnectButtonText
        {
            get { return _changeConnectButtonText; }
            set { Set(ref _changeConnectButtonText, value); }
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get { return _isProcessing; }
            set
            {
                Set(ref _isProcessing, value);
                ChangeConnectButtonCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _requiresPassword;
        public bool RequiresPassword
        {
            get { return _requiresPassword; }
            set { Set(ref _requiresPassword, value); }
        }
        
        public bool IsPasswordBoxVisible
        {
            get
            {
                var currentNetwork = _networkService.GetCurrentWifiNetwork();
                bool thisIsNotCurrentNetwork = currentNetwork == null
                    || currentNetwork.Ssid != NetworkName
                    || currentNetwork.Bssid != _bssid;
                bool requiresPassword = !_networkService.IsNetworkOpen(_backingNetwork);
                return IsSelected && thisIsNotCurrentNetwork && requiresPassword;
            }
        }

        public RelayCommand<string> ChangeConnectButtonCommand => new RelayCommand<string>(ChangeConnectionState,
            ChangeConnectButtonCanExecute);

        private bool ChangeConnectButtonCanExecute(string arg)
        {
            return !IsProcessing;
        }

        private async void ChangeConnectionState(string password)
        {
            IsProcessing = true;
            var currentNetwork = _networkService.GetCurrentWifiNetwork();       
            if(currentNetwork == null 
                || currentNetwork.Ssid != NetworkName 
                || currentNetwork.Bssid != _bssid)
            {
                //connect
                if (String.IsNullOrWhiteSpace(password))
                {
                    await _networkService.ConnectToNetwork(_backingNetwork, IsAutoConnect);
                }
                else
                {
                    PasswordCredential credential = new PasswordCredential();
                    credential.Password = password;
                    await _networkService.ConnectToNetworkWithPassword(_backingNetwork, IsAutoConnect, credential);
                }
            }
            else
            {
                //disconnect
                _networkService.DisconnectNetwork(currentNetwork);                
            }            

            var updatedBackingNetwork = (await _networkService.GetAvailableWifiNetworks())
                    .Where(x => x.Bssid == _bssid && x.Ssid == NetworkName)
                    .FirstOrDefault();
            UpdateValues(updatedBackingNetwork);
            IsProcessing = false;
        }

        public WifiItemViewModel(WiFiAvailableNetwork model, INetworkService networkService, SettingsService settings)
        {
            _networkService = networkService;
            _settingsService = settings;

            UpdateValues(model);
        }

        private void UpdateValues(WiFiAvailableNetwork network)
        {
            _backingNetwork = network;
            _bssid = network.Bssid;
            NetworkName = network.Ssid;
            NetworkGlyph = network.SignalBars == 4 ? FontIcons.WifiFourBars
                : network.SignalBars == 3 ? FontIcons.WifiThreeBars
                : network.SignalBars == 2 ? FontIcons.WifiTwoBars
                : FontIcons.WifiOneBar;
            RequiresPassword = network.SecuritySettings.NetworkAuthenticationType != NetworkAuthenticationType.None;
            bool tryIsAutoConnect;
            if (_settingsService.SsidToAutoConnect.TryGetValue(NetworkName, out tryIsAutoConnect))
            {
                IsAutoConnect = tryIsAutoConnect;
            }
            if (_networkService.GetCurrentWifiNetwork() == network)
            {
                NetworkState = ConnectedNetworkStateString;
                ChangeConnectButtonText = "Disconnect";
                if (network.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.None)
                {
                    NetworkState += ", " + OpenNetworkStateString;
                }
                else
                {
                    NetworkState += ", " + SecuredNetworkStateString;
                }
            }
            else
            {
                ChangeConnectButtonText = "Connect";
                if (network.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.None)
                {
                    NetworkState = OpenNetworkStateString;
                }
                else
                {
                    NetworkState = SecuredNetworkStateString;
                }
            }
        }        
    }
}
