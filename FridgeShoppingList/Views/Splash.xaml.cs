using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FridgeShoppingList.Views
{
    public sealed partial class Splash : UserControl
    {
        private Random _rng = new Random();
        private List<string> _loginPhrases = new List<string>
        {
            "Aligning dilithium crystals...",
            "Reticulating splines...",
            "Fluffing bio-neural gel packs...",
            "Replicating tea, Earl Grey, hot...",
            "Counterfeiting gold-pressed latinum...",
            "Estimating good days on which to die...",
        };

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
            await Task.Delay(2500);
            int firstIndex = _rng.Next(_loginPhrases.Count - 1);
            SplashLoginText.Text = _loginPhrases[firstIndex].ToUpperInvariant();
            await Task.Delay(3000);

            int secondIndex = -1;
            do
            {
                secondIndex = _rng.Next(_loginPhrases.Count - 1);
            } while (secondIndex == firstIndex);
            SplashLoginText.Text = _loginPhrases[secondIndex].ToUpperInvariant();
            await Task.Delay(2500);
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

