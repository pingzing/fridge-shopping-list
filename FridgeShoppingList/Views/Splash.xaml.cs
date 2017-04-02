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
            Window.Current.SizeChanged += (s, e) => Resize(splashScreen);
            Resize(splashScreen);
            SplashInProgress = BeginSplashProcess();
        }

        private async Task BeginSplashProcess()
        {            
            //Simulate some kind of fun login text here
            await Task.Delay(10000);
        }

        private void Resize(SplashScreen splashScreen)
        {                                                    
            TextTransform.TranslateY = splashScreen.ImageLocation.Height * .75;            
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {            
            StarfleetRotateStoryboard.Begin();            
        }
    }
}

