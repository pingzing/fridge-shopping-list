using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using FridgeShoppingList.Models;

namespace FridgeShoppingList.ViewModels
{   
    public class MainPageViewModel : ViewModelBaseEx
    {
        public MainPageViewModel()
        {            
            ShoppingLists = new ObservableCollection<DummyElement>
            {
                new DummyElement {Element = "Item 1" },
                new DummyElement {Element = "Item 2" },
                new DummyElement {Element = "Item 3" },
            };
        }        

        private ObservableCollection<DummyElement> _shoppingLists = new ObservableCollection<DummyElement>();
        public ObservableCollection<DummyElement> ShoppingLists
        {
            get { return _shoppingLists; }
            set { Set(ref _shoppingLists, value); }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                //restore values from suspensionState dict
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                //store values for later restoration in suspensionState dict
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }        

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoPrivacy() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 2);

    }
}

