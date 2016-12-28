using FridgeShoppingList.Models;
using FridgeShoppingList.Services;
using FridgeShoppingList.Services.SettingsServices;
using FridgeShoppingList.ViewModels.ControlViewModels;
using GalaSoft.MvvmLight.Command;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace FridgeShoppingList.ViewModels
{
    public class NetworkConfigPageViewModel : ViewModelBaseEx
    {        
        private readonly SettingsService _settingsService;
        private readonly INetworkService _networkService;

        private string _currentNetworkName;
        public string CurrentNetworkName
        {
            get { return _currentNetworkName; }
            set { Set(ref _currentNetworkName, value); }
        }

        private string _currentNetworkIconGlyph;
        public string CurrentNetworkIconGlyph
        {
            get { return _currentNetworkIconGlyph; }
            set { Set(ref _currentNetworkIconGlyph, value); }
        }

        private string _currentIpAddress;
        public string CurrentIpAddress
        {
            get { return _currentIpAddress; }
            set { Set(ref _currentIpAddress, value); }
        }

        private bool _isWifiEnabled;
        public bool IsWifiEnabled
        {
            get { return _isWifiEnabled; }
            set { Set(ref _isWifiEnabled, value); }
        }

        private bool _isLookingForNetworks;
        public bool IsLookingForNetworks
        {
            get { return _isLookingForNetworks; }
            set { Set(ref _isLookingForNetworks, value); }
        }        

        private ObservableCollection<WifiItemViewModel> _wifiNetworks = new ObservableCollection<WifiItemViewModel>();
        public ObservableCollection<WifiItemViewModel> WifiNetworks
        {
            get { return _wifiNetworks; }
            set { Set(ref _wifiNetworks, value); }
        }        

        public RelayCommand<bool> WifiToggled => new RelayCommand<bool>(ToggleWifi);        

        public NetworkConfigPageViewModel(INetworkService networkService, SettingsService settings)
        {
            _networkService = networkService;
            _settingsService = settings;            
        }        

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            NetworkInfo info = parameter as NetworkInfo;
            if(info != null)
            {
                CurrentIpAddress = info.NetworkIpv4;
                CurrentNetworkName = info.NetworkName;
                CurrentNetworkIconGlyph = info.IsWired
                    ? FontIcons.Ethernet
                    : FontIcons.WifiFourBars;
            }

            _networkService.WifiRadioStateChanged += WifiRadioStateChanged;
            _networkService.AvailableNetworksChanged += AvailableNetworksChanged;            

            IsWifiEnabled = await _networkService.IsWifiRadioOn();
            WifiNetworks = new ObservableCollection<WifiItemViewModel>( 
                (await _networkService.GetAvailableWifiNetworks())
                .Select(x => new WifiItemViewModel(x, _networkService, _settingsService))
            );
        }        

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {            
            _networkService.WifiRadioStateChanged -= WifiRadioStateChanged;
            _networkService.AvailableNetworksChanged -= AvailableNetworksChanged;
            
            foreach(var network in WifiNetworks)
            {
                network.IsSelected = false;
            }
            return Task.CompletedTask;
        }       

        private void ToggleWifi(bool newState)
        {
            if(newState)
            {
                _networkService.EnableWifiRadio();
            }
            else
            {
                _networkService.DisableWifiRadio();
            }
        }

        private void WifiRadioStateChanged(Windows.Devices.Radios.Radio sender, object args)
        {
            bool newIsOn = sender.State == Windows.Devices.Radios.RadioState.On;
            if (IsWifiEnabled != newIsOn)
            {
                IsWifiEnabled = newIsOn;
            }
        }

        private async void AvailableNetworksChanged(NetworkService sender, AvailableNetworksChangedArgs args)
        {
            if(args.AddedNetworks != null)
            {
                foreach(var added in args.AddedNetworks)
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(
                        () => WifiNetworks.Add(new WifiItemViewModel(added, _networkService, _settingsService)));
                }
            }

            if(args.RemovedNetworks != null)
            {
                foreach(var removed in args.RemovedNetworks)
                {
                    var toRemove = WifiNetworks.Where(x => x.Bssid == removed.Bssid).FirstOrDefault();
                    if (toRemove != null)
                    {
                        await DispatcherHelper.ExecuteOnUIThreadAsync(
                            () => WifiNetworks.Remove(toRemove));                        
                    }
                }
            }
        }
    }
}
