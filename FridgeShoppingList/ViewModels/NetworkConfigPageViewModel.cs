using FridgeShoppingList.Models;
using FridgeShoppingList.Services;
using FridgeShoppingList.Services.SettingsServices;
using FridgeShoppingList.ViewModels.ControlViewModel;
using GalaSoft.MvvmLight.Command;
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
        private readonly INetworkService _networkService;
        private readonly SettingsService _settingsService;
        private DispatcherTimer _networkSearchTimer = new DispatcherTimer();

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

        private ObservableCollection<WifiItemViewModel> _wifiNetowrks;
        public ObservableCollection<WifiItemViewModel> WifiNetworks
        {
            get { return _wifiNetowrks; }
            set { Set(ref _wifiNetowrks, value); }
        }

        public RelayCommand<bool> WifiToggled => new RelayCommand<bool>(ToggleWifi);        

        public NetworkConfigPageViewModel(INetworkService networkService, SettingsService settings)
        {
            _networkService = networkService;
            _settingsService = settings;
            _networkSearchTimer.Interval = TimeSpan.FromSeconds(30);
            _networkSearchTimer.Tick += NetworkSearchTimer_Tick;
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
            
            IsWifiEnabled = await _networkService.IsWifiRadioOn();                        
            _networkSearchTimer.Start();
            NetworkSearchTimer_Tick(null, null);             
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            _networkSearchTimer.Stop();
            _networkService.WifiRadioStateChanged -= WifiRadioStateChanged;
            return Task.CompletedTask;
        }

        private async void NetworkSearchTimer_Tick(object sender, object e)
        {
            if (IsWifiEnabled)
            {
                IsLookingForNetworks = true;
                WifiNetworks = new ObservableCollection<WifiItemViewModel>(
                    (await _networkService.GetAvailableWifiNetworks())
                    .OrderByDescending(x => x.SignalBars)
                    .Select(x => new WifiItemViewModel(x, _networkService, _settingsService)));
                IsLookingForNetworks = false;
            }
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
    }
}
