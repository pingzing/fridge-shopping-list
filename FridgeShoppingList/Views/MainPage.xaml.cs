using System;
using FridgeShoppingList.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;

namespace FridgeShoppingList.Views
{
    public sealed partial class MainPage : Page
    {
        private SolidColorBrush _redPurpleBrush = (SolidColorBrush)App.Current.Resources["LcarsRedPurpleBrush"];
        private SolidColorBrush _fadedRedPurpleBrush;

        private MainPageViewModel _viewModel;
        public MainPageViewModel ViewModel => _viewModel ?? (_viewModel = (MainPageViewModel)DataContext);

        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            _fadedRedPurpleBrush = new SolidColorBrush { Color = _redPurpleBrush.Color, Opacity = 0.5 };

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += (s, e) =>
            {
                var tickNow = DateTime.Now;
                DateTimeTextPart1.Text = tickNow.ToString("dddd, MMMM dd yyyy, hh").ToUpperInvariant();
                DateTimeTextPart2.Text = tickNow.ToString("mmtt").ToUpperInvariant();

                timer.Stop();
                int secondsTillNextMinute = 60 - tickNow.Second;
                timer.Interval = TimeSpan.FromSeconds(secondsTillNextMinute);
                timer.Start();
            };
            timer.Start();
            var now = DateTime.Now;
            DateTimeTextPart1.Text = now.ToString("dddd, MMMM dd yyyy, hh").ToUpperInvariant();
            DateTimeTextPart2.Text = now.ToString("mmtt").ToUpperInvariant();

            DispatcherTimer blinkTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),                
            };
            blinkTimer.Tick += (s, e) => 
            {                
                if (DateTimeTextPartColon.Foreground == _fadedRedPurpleBrush)
                {
                    DateTimeTextPartColon.Foreground = _redPurpleBrush;
                }
                else
                {
                    DateTimeTextPartColon.Foreground = _fadedRedPurpleBrush;
                }                
            };
            blinkTimer.Start();
        }
    }
}
