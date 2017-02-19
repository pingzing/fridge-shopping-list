using GalaSoft.MvvmLight.Command;
using System;
using Template10.Mvvm;
using Windows.Foundation;

namespace FridgeShoppingList.ViewModels.ControlViewModels
{
    public class LoginToOneNoteViewModel : BindableBase, IResultDialogViewModel<string>
    {
        private Uri _lastNavigationUri;

        /// <summary>
        /// The 'code' used in the OneDrive Code Flow authentication process. Valid for a few minutes, and should be immediately redeemed for an access token.
        /// </summary>
        public string Result { get; private set; }

        private Uri _webViewUrl;
        public Uri WebViewUrl
        {
            get { return _webViewUrl; }
            set { Set(ref _webViewUrl, value); }
        }

        public RelayCommand<Uri> WebViewNavigatingCommand => new RelayCommand<Uri>(WebViewNavigating);

        public void SetResultToCurrentState()
        {
            var decoder = new WwwFormUrlDecoder(_lastNavigationUri.Query);
            Result = decoder.GetFirstValueByName("code");            
        }

        public LoginToOneNoteViewModel(object args)
        {
            string startingUrl = args as string;
            WebViewUrl = new Uri(startingUrl);
        }

        private void WebViewNavigating(Uri obj)
        {
            _lastNavigationUri = obj;            
        }

    }
}
