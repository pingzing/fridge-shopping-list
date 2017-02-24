using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FridgeShoppingList.Models
{
    public class DeletePageResult
    {
        public string PageId { get; set; }
        public DeleteResult Result { get; set; }

        public bool IsSuccessResult => Result == DeleteResult.Success;
    }

    public enum DeleteResult
    {
        Success,
        FailureByNetwork,
        FailureBy404,
        FailureUnspecified
    }
}
