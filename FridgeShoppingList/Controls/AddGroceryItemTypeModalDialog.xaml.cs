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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FridgeShoppingList.Controls
{
    public sealed partial class AddGroceryItemTypeModalDialog : LcarsModalDialog.LcarsModalDialog
    {        
        public AddGroceryItemTypeViewModel ViewModel { get; private set; }

        public AddGroceryItemTypeModalDialog(AddGroceryItemTypeViewModel viewModel)
        {
            ViewModel = ViewModel;
            this.DataContext = ViewModel;
            this.InitializeComponent();
        }

        private void LcarsModalDialog_PrimaryButtonClick(LcarsModalDialog.LcarsModalDialog sender, object args)
        {
            ViewModel.SetResultToCurrentState();
        }
    }
}
