using EPiServer.Shell.Navigation;
using System.Collections.Generic;
using System.Linq;

namespace Epicweb.Optimizely.RedirectManager
{
    [MenuProvider]
    public class RedirectManagerMenuProvider : IMenuProvider
    {
        public IEnumerable<MenuItem> GetMenuItems()
        {
            var menuItems = new List<MenuItem>();

            menuItems.Add(new UrlMenuItem("RedirectManager",
                MenuPaths.Global + "/cms/redirectmanager",
                "/redirectmanager/")
            {
                SortIndex = SortIndex.First + 25,
                IsAvailable = (context) => RedirectManagerOptions.Current.AllowedRoles.Any(role => context.User.IsInRole(role)),
                AuthorizationPolicy = RedirectManagerOptions.AuthorizationPolicyName
            });
            return menuItems;
        }
    }
}
