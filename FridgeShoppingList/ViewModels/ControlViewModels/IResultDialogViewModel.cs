using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FridgeShoppingList.ViewModels.ControlViewModels
{
    public interface IResultDialogViewModel<T>
    {
        T Result { get; }
    }
}
