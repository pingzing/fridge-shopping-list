using FridgeShoppingList.ViewModels;
using FridgeShoppingList.ViewModels.ControlViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FridgeShoppingList.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NetworkConfigPage : Page
    {
        private NetworkConfigPageViewModel _viewModel;
        public NetworkConfigPageViewModel ViewModel => _viewModel ?? (_viewModel = (NetworkConfigPageViewModel)DataContext);

        public NetworkConfigPage()
        {
            this.InitializeComponent();
        }

        private void WifiOnOffSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            bool newState = ((ToggleSwitch)sender).IsOn;
            ViewModel.WifiToggled.Execute(newState);
        }

        private void WifiNetworksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView list = (ListView)sender;
            foreach(WifiItemViewModel item in e.RemovedItems.OfType<WifiItemViewModel>())
            {
                item.IsSelected = false;
            }
            foreach (WifiItemViewModel item in e.AddedItems.OfType<WifiItemViewModel>())
            {
                item.IsSelected = true;
            }
        }
    }
}
