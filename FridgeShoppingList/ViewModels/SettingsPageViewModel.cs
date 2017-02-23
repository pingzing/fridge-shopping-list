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
    public class SettingsPageViewModel : ViewModelBase
    {        
        private IOneNoteService _oneNote;

        public OneNotePartViewModel OneNotePartViewModel { get; }
        public StatusPartViewModel StatusPartViewModel { get; private set; }

        public RelayCommand ShutdownCommand => new RelayCommand(ShutdownDevice);
        public RelayCommand RestartCommand => new RelayCommand(RestartDevice);        

        public SettingsPageViewModel(IOneNoteService oneNote)
        {            
            _oneNote = oneNote;
            StatusPartViewModel = new StatusPartViewModel();
            OneNotePartViewModel = new OneNotePartViewModel(_oneNote);
        }

        private void ShutdownDevice()
        {
            ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromMilliseconds(0));
        }

        private void RestartDevice()
        {
            ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(5));
        }      

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            OneNotePartViewModel.OnNavigatedToAsync(parameter, mode, state);
            StatusPartViewModel.OnNavigatedToAsync(parameter, mode, state);
            return base.OnNavigatedToAsync(parameter, mode, state);
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            OneNotePartViewModel.OnNavigatedFromAsync(pageState, suspending);
            StatusPartViewModel.OnNavigatedFromAsync(pageState, suspending);
            return base.OnNavigatedFromAsync(pageState, suspending);
        }
    }

    public class OneNotePartViewModel : ViewModelBase
    {
        //public for bindability
        public readonly IOneNoteService _oneNoteService;

        string oneNotestatusText = "Not connected";
        public string OneNoteStatusText
        {
            get { return oneNotestatusText; }
            set { Set(ref oneNotestatusText, value); }
        }

        public bool IsConnected => _oneNoteService.ConnectedStatus;

        public OneNotePartViewModel(IOneNoteService oneNote)
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime
            }
            else
            {
                _oneNoteService = oneNote;
                _oneNoteService.ConnectedStatusChanged += (s, newConnected) =>
                {
                    if (newConnected)
                    {
                        OneNoteStatusText = "Connected";
                        RaisePropertyChanged(nameof(IsConnected));
                    }
                    else
                    {
                        OneNoteStatusText = "Not connected";
                        RaisePropertyChanged(nameof(IsConnected));
                    }
                };
            }
        }       
        
        public async void ConnectToOneNote()
        {
            await _oneNoteService.GetShoppingListPageContent();
        } 

        public void DisconnectFromOneNote()
        {
            _oneNoteService.Logout();
        }
    }

    public class StatusPartViewModel : ViewModelBase
    {        
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

        public StatusPartViewModel()
        {
            _updateInfoTimer = new DispatcherTimer();
            _updateInfoTimer.Interval = TimeSpan.FromSeconds(3);
            _updateInfoTimer.Tick += UpdateInfo;
        }

        private void UpdateInfo(object sender, object e)
        {
            AvailableMemory = $"{SystemInformation.AvailableMemory.ToString("F")}MB";
            
            if (Microsoft.Toolkit.Uwp.ConnectionHelper.ConnectionType == Microsoft.Toolkit.Uwp.ConnectionType.Ethernet)
            {
                NetworkName = NetworkHelper.GetCurrentNetworkName();
                NetworkIconGlyph = FontIcons.Ethernet;
            }
            else if (Microsoft.Toolkit.Uwp.ConnectionHelper.ConnectionType == Microsoft.Toolkit.Uwp.ConnectionType.WiFi)
            {
                var wifi = NetworkHelper.GetCurrentWifiNetwork();                                
                if (wifi != null)
                {
                    NetworkName = wifi.WlanConnectionProfileDetails.GetConnectedSsid();
                    byte? signalBars = wifi.GetSignalBars();
                    NetworkIconGlyph = signalBars == 4 ? FontIcons.WifiFourBars
                        : signalBars == 3 ? FontIcons.WifiThreeBars
                        : signalBars == 2 ? FontIcons.WifiTwoBars
                        : FontIcons.WifiOneBar;
                }
            }
            IpAddress = NetworkHelper.GetCurrentIpv4Address();            
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

