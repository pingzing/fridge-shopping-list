using FridgeShoppingList.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using Template10.Services.NavigationService;

namespace FridgeShoppingList.Services
{
    public class ServiceRegistrar
    {
        static ServiceRegistrar()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
            {
                //design-time stuff
            }
            else
            {
                SimpleIoc.Default.Register<IMessenger>(() => Messenger.Default);
                SimpleIoc.Default.Register<INetworkService>(() => new NetworkService());
                SimpleIoc.Default.Register(() => SettingsServices.SettingsService.Instance);
                SimpleIoc.Default.Register<IDialogService>(() => new DialogService());
            }
            SimpleIoc.Default.Register<MainPageViewModel>();
            SimpleIoc.Default.Register<SettingsPageViewModel>();
            SimpleIoc.Default.Register<NetworkConfigPageViewModel>();
        }        
    }
}
