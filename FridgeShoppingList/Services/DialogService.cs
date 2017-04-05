using FridgeShoppingList.Controls;
using FridgeShoppingList.Controls.LcarsModalDialog;
using FridgeShoppingList.Models;
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
        /// <summary>
        /// Shows a modal dialog backed by the given ViewModel, and expects a return value of the given type.
        /// </summary>
        /// <typeparam name="TViewModel">The backing viewmodel that the dialog should use.</typeparam>
        /// <typeparam name="TResult">The expected return value the dialog should return.</typeparam>
        /// <returns>A TResult.</returns>
        Task<TResult> ShowModalDialogAsync<TViewModel, TResult>() where TViewModel : IResultDialogViewModel<TResult>;

        /// <summary>
        /// Shows a modal dialog backed by the given ViewModel, and expects a return value of the given type.
        /// </summary>
        /// <param name="args">The parameter to be passed to the ViewModel's constructor.</param>
        /// <typeparam name="TViewModel">The backing viewmodel that the dialog should use.</typeparam>
        /// <typeparam name="TResult">The expected return value the dialog should return.</typeparam>
        /// <returns>A TResult.</returns>
        Task<TResult> ShowModalDialogAsync<TViewModel, TResult>(object args) where TViewModel : IResultDialogViewModel<TResult>;
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

        public async Task<TResult> ShowModalDialogAsync<TViewModel, TResult>() where TViewModel : IResultDialogViewModel<TResult>
        {
            IResultDialogViewModel<TResult> vm = GetViewModel<TViewModel, TResult>();
            LcarsModalDialog dialog = ResolveModalDialogForViewModel(vm);

            _semaphore.Wait();
            await dialog.OpenAsync();
            _semaphore.Release();

            return vm.Result;
        }

        public async Task<TResult> ShowModalDialogAsync<TViewModel, TResult>(object args) where TViewModel : IResultDialogViewModel<TResult>
        {
            IResultDialogViewModel<TResult> vm = GetViewModel<TViewModel, TResult>(args);
            LcarsModalDialog dialog = ResolveModalDialogForViewModel(vm);

            _semaphore.Wait();
            await dialog.OpenAsync();
            _semaphore.Release();

            return vm.Result;
        }

        private static IResultDialogViewModel<TResult> GetViewModel<TViewModel, TResult>(object args = null)
        {
            if (typeof(TViewModel) == typeof(AddToInventoryViewModel))
            {
                return (IResultDialogViewModel<TResult>)new AddToInventoryViewModel(args);
            }
            else if (typeof(TViewModel) == typeof(AddGroceryItemTypeViewModel))
            {
                return (IResultDialogViewModel<TResult>)new AddGroceryItemTypeViewModel((GroceryItemType)args);
            }          
            else
            {
                throw new ArgumentException("No ViewModel registered for the given type.");
            }
        }                

        private static LcarsModalDialog ResolveModalDialogForViewModel<T>(IResultDialogViewModel<T> vm)
        {
            if (vm is AddToInventoryViewModel)
            {
                return new AddToInventoryModalDialog((AddToInventoryViewModel)vm);
            }
            else if(vm is AddGroceryItemTypeViewModel)
            {
                return new AddGroceryItemTypeModalDialog((AddGroceryItemTypeViewModel)vm);
            }           
            else
            {
                return new LcarsModalDialog();
            }
        }
    }
}
