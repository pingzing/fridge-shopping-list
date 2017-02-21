using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace FridgeShoppingList.Extensions
{
    public static class PasswordVaultExtensions
    {
        /// <summary>
        /// Attempts to retreive find all resources, but returns null on failure instead of throwing an exception.
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public static IReadOnlyList<PasswordCredential> FindAllByResourceSafely(this PasswordVault vault, string resourceName)
        {
            if (resourceName == null)
            {
                return null;
            }
            try
            {
                return vault.FindAllByResource(resourceName);
            }
            catch(COMException)
            {
                return new List<PasswordCredential>().AsReadOnly();
            }
        }

        /// <summary>
        /// Attempts to retrieve the PasswordCredential as normal, but returns null on failure instead of throwing an exception.
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="resourceName"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static PasswordCredential RetrieveSafely(this PasswordVault vault, string resourceName, string userName)
        {
            if (userName  == null|| resourceName == null)
            {
                return null;
            }
            try
            {
                return vault.Retrieve(resourceName, userName);
            }
            catch(COMException)
            {
                return null;
            }
        }
    }
}
