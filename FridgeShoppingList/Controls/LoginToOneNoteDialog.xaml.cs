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

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FridgeShoppingList.Controls
{
    public sealed partial class LoginToOneNoteDialog : LcarsModalDialog.LcarsModalDialog
    {
        public LoginToOneNoteViewModel ViewModel { get; set; }

        public LoginToOneNoteDialog(LoginToOneNoteViewModel viewModel)
        {
            ViewModel = viewModel;
            this.DataContext = ViewModel;
            this.InitializeComponent();
        }

        private void LcarsModalDialog_PrimaryButtonClick(LcarsModalDialog.LcarsModalDialog sender, object args)
        {
            //explicitly do nothing
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            ViewModel.WebViewNavigatingCommand.Execute(args.Uri);
            if (args.Uri.AbsoluteUri.StartsWith("https://login.live.com/oauth20_desktop.srf?code="))
            {
                ViewModel.SetResultToCurrentState();
                this.Close();
            }
        }
    }
}
