using FridgeShoppingList.Controls;
using FridgeShoppingList.ViewModels.ControlViewModels;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace FridgeShoppingList.Services
{
    public interface IDialogService
    {
        Task ShowDialog(string message);
        Task ShowDialog(string message, string title);
        Task<IUICommand> ShowDialog(MessageDialog dialog);
        Task<ContentDialogResult> ShowDialog(ContentDialog dialog);
        Task<TResult> ShowDialogAsync<TViewModel, TResult>() where TViewModel : IResultDialogViewModel<TResult>;
    }

    public class DialogService : IDialogService
    {
        private static readonly SemaphoreSlim _semaphore;

        static DialogService()
        {
            _semaphore = new SemaphoreSlim(1);
            SimpleIoc.Default.Register<AddToInventoryViewModel>();
        }

        public async Task ShowDialog(string message)
        {
            var dialog = new MessageDialog(message);
            await ShowDialog(dialog);
        }

        public async Task ShowDialog(string message, string title)
        {
            var dialog = new MessageDialog(message, title);
            await ShowDialog(dialog);
        }

        public async Task<IUICommand> ShowDialog(MessageDialog dialog)
        {
            _semaphore.Wait();
            var result = await dialog.ShowAsync();
            _semaphore.Release();

            return result;
        }

        public async Task<ContentDialogResult> ShowDialog(ContentDialog dialog)
        {
            _semaphore.Wait();
             var result = await dialog.ShowAsync();
            _semaphore.Release();

            return result;
        }

        public async Task<TResult> ShowDialogAsync<TViewModel, TResult>() where TViewModel : IResultDialogViewModel<TResult>
        {
            IResultDialogViewModel<TResult> vm = GetViewModel<TViewModel, TResult>();
            ContentDialog dialog = ResolveDialogForVieWModel(vm);

            _semaphore.Wait();
            var result = await dialog.ShowAsync();
            _semaphore.Release();

            if(result == ContentDialogResult.Primary)
            {
                return vm.Result;
            }
            else
            {
                return default(TResult);
            }
        }

        private static IResultDialogViewModel<TResult> GetViewModel<TViewModel, TResult>()
        {
            if(typeof(TViewModel) == typeof(AddToInventoryViewModel))
            {
                return (IResultDialogViewModel<TResult>)new AddToInventoryViewModel();
            }
            else if(typeof(TViewModel) == typeof(AddGroceryItemTypeViewModel))
            {
                return (IResultDialogViewModel<TResult>)new AddGroceryItemTypeViewModel();
            }
            else
            {
                throw new ArgumentException("No ViewModel registered for the given type.");
            }
        }

        private static ContentDialog ResolveDialogForVieWModel<T>(IResultDialogViewModel<T> vm)
        {
            if (vm is AddToInventoryViewModel)
            {
                return new AddToInventoryDialog((AddToInventoryViewModel)vm);
            }
            else if (vm is AddGroceryItemTypeViewModel)
            {
                return new AddGroceryItemTypeDialog((AddGroceryItemTypeViewModel)vm);
            }
            else
            {
                return new ContentDialog();
            }
        }
    }
}
