namespace FridgeShoppingList.ViewModels.ControlViewModels
{
    public interface IResultDialogViewModel<T>
    {
        /// <summary>
        /// The result that the dialog service will inspect in order to return a value.
        /// </summary>
        T Result { get; }

        /// <summary>
        /// This method can be called by external actors in order to commit the 
        /// ViewModel's current state to the Result property.
        /// </summary>
        void SetResultToCurrentState();
    }
}
