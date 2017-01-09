using FridgeShoppingList.ViewModels.ControlViewModels;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FridgeShoppingList.Controls
{
    public sealed partial class AddToInventoryModalDialog : LcarsModalDialog.LcarsModalDialog
    {
        public AddToInventoryViewModel ViewModel { get; private set; }

        public AddToInventoryModalDialog(AddToInventoryViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
        }

        private void LcarsModalDialog_PrimaryButtonClick(LcarsModalDialog.LcarsModalDialog sender, object args)
        {
            ViewModel.SetResultToCurrentState();
        }

        private void LcarsModalDialog_SecondaryButtonClick(LcarsModalDialog.LcarsModalDialog sender, object args)
        {

        }
    }
}
