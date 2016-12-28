using Microsoft.Toolkit.Uwp.Helpers;
using System;
using Template10.Mvvm;
using Windows.UI.Xaml;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Template10.Services.NavigationService;
using FridgeShoppingList.Services;
using FridgeShoppingList.Models;
using GalaSoft.MvvmLight.Command;
using Windows.System;
using FridgeShoppingList.Views;

namespace FridgeShoppingList.ViewModels
{
    public class SettingsPageViewModel : ViewModelBaseEx
    {
        private INetworkService _networkService;

        public SettingsPartViewModel SettingsPartViewModel { get; } = new SettingsPartViewModel();
        public AboutPartViewModel AboutPartViewModel { get; private set; }

        public RelayCommand ShutdownCommand => new RelayCommand(ShutdownDevice);
        public RelayCommand RestartCommand => new RelayCommand(RestartDevice);
        public RelayCommand OpenNetworkConfigCommand => new RelayCommand(OpenNetworkConfig);

        public SettingsPageViewModel(INetworkService networkService)
        {
            _networkService = networkService;
            AboutPartViewModel = new AboutPartViewModel(_networkService);
        }

        private void ShutdownDevice()
        {
            ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromMilliseconds(0));
        }

        private void RestartDevice()
        {
            ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(5));
        }

        private void OpenNetworkConfig()
        {
            NavigationService.NavigateAsync(typeof(NetworkConfigPage), new NetworkInfo
            {
                IsWired = AboutPartViewModel.NetworkIconGlyph == FontIcons.Ethernet,
                NetworkIpv4 = AboutPartViewModel.IpAddress,
                NetworkName = AboutPartViewModel.NetworkName
            });
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            SettingsPartViewModel.OnNavigatedToAsync(parameter, mode, state);
            AboutPartViewModel.OnNavigatedToAsync(parameter, mode, state);
            return base.OnNavigatedToAsync(parameter, mode, state);
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            SettingsPartViewModel.OnNavigatedFromAsync(pageState, suspending);
            AboutPartViewModel.OnNavigatedFromAsync(pageState, suspending);
            return base.OnNavigatedFromAsync(pageState, suspending);
        }
    }

    public class SettingsPartViewModel : ViewModelBaseEx
    {
        Services.SettingsServices.SettingsService _settings;

        public SettingsPartViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime
            }
            else
            {                
            }
        }        
    }

    public class AboutPartViewModel : ViewModelBaseEx
    {
        private readonly INetworkService _networkService;

        private bool _networkInfoFirstLoaded = false;

        private DispatcherTimer _updateInfoTimer;

        public Uri Logo => Windows.ApplicationModel.Package.Current.Logo;

        public string DisplayName => Windows.ApplicationModel.Package.Current.DisplayName;

        public string Version
        {
            get
            {
                var v = Windows.ApplicationModel.Package.Current.Id.Version;
                return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            }
        }

        private string _availableMemory;
        public string AvailableMemory
        {
            get { return _availableMemory; }
            set { Set(ref _availableMemory, value); }
        }

        private string _networkIconGlyph = FontIcons.Ethernet;
        public string NetworkIconGlyph
        {
            get { return _networkIconGlyph; }
            set { Set(ref _networkIconGlyph, value); }
        }

        private string _networkName;
        public string NetworkName
        {
            get { return _networkName; }
            set { Set(ref _networkName, value); }
        }

        private string _ipAddress;
        public string IpAddress
        {
            get { return _ipAddress; }
            set { Set(ref _ipAddress, value); }
        }

        private bool _isNetworkInfoLoading;
        public bool IsNetworkInfoLoading
        {
            get { return _isNetworkInfoLoading; }
            set { Set(ref _isNetworkInfoLoading, value); }
        }       

        public AboutPartViewModel(INetworkService networkService)
        {
            _networkService = networkService;
            _updateInfoTimer = new DispatcherTimer();
            _updateInfoTimer.Tick += UpdateInfo;
            _updateInfoTimer.Interval = TimeSpan.FromSeconds(5);
            //Start is triggered in OnNavigatedTo            
        }

        private async void UpdateInfo(object sender, object e)
        {
            AvailableMemory = $"{SystemInformation.AvailableMemory.ToString("F")}MB";

            if (!_networkInfoFirstLoaded)
            {
                _networkInfoFirstLoaded = true;
                IsNetworkInfoLoading = true;
            }
            if (_networkService.IsOnWiredConnection())
            {
                NetworkName = _networkService.GetCurrentNetworkName();
                NetworkIconGlyph = FontIcons.Ethernet;
            }
            else if (await _networkService.IsWifiAvailable())
            {
                await _networkService.GetAvailableWifiNetworks();
                var wifi = _networkService.GetCurrentWifiNetwork();
                if (wifi != null)
                {
                    NetworkName = wifi.Ssid;
                    NetworkIconGlyph = wifi.SignalBars == 4 ? FontIcons.WifiFourBars
                        : wifi.SignalBars == 3 ? FontIcons.WifiThreeBars
                        : wifi.SignalBars == 2 ? FontIcons.WifiTwoBars
                        : FontIcons.WifiOneBar;
                }
            }
            IpAddress = _networkService.GetCurrentIpv4Address();
            IsNetworkInfoLoading = false;
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            UpdateInfo(null, null);
            _updateInfoTimer.Start();
            return Task.CompletedTask;
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            _updateInfoTimer.Stop();
            return Task.CompletedTask;
        }
    }
}

