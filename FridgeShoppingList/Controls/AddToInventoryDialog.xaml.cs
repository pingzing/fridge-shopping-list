using FridgeShoppingList.Models;
using FridgeShoppingList.ViewModels.ControlViewModels;
using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FridgeShoppingList.Controls
{
    public sealed partial class AddToInventoryDialog : ContentDialog
    {
        public AddToInventoryViewModel ViewModel { get; private set; }

        public AddToInventoryDialog(AddToInventoryViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.SetResultToCurrentState();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
