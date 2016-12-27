using System;
using FridgeShoppingList.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

namespace FridgeShoppingList.Views
{
    public sealed partial class MainPage : Page
    {
        private MainPageViewModel _viewModel;
        public MainPageViewModel ViewModel => _viewModel ?? (_viewModel = (MainPageViewModel)DataContext);

        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }
    }
}
