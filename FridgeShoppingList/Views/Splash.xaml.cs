using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FridgeShoppingList.Views
{
    public sealed partial class Splash : UserControl
    {
        public Task SplashInProgress { get; private set; }

        public Splash(SplashScreen splashScreen)
        {
            InitializeComponent();

            TopBorder.Visibility = Visibility.Collapsed;
            BottomBorder.Visibility = Visibility.Collapsed;
            HeaderTextBackground.Opacity = 0;
            HeaderText.Opacity = 0;

            SplashInProgress = BeginSplashProcess();
        }

        private async Task BeginSplashProcess()
        {            
            //Simulate some kind of fun login text here
            await Task.Delay(10000);
        }
        

        private async void Image_Loaded(object sender, RoutedEventArgs e)
        {            
            StarfleetRotateStoryboard.Begin();

            TopBorder.Visibility = Visibility.Visible;
            BottomBorder.Visibility = Visibility.Visible;
            await GrowInBordersStoryboard.BeginAsync();

            HeaderTextBackground.Opacity = 1;            
            await SwooshInHeaderTextStoryboard.BeginAsync();
        }
    }
}

