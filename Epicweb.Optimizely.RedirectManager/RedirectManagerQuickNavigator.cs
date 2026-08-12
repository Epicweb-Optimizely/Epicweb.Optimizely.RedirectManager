using EPiServer.Core;
using EPiServer.Security;
using EPiServer.Web;
using System.Collections.Generic;
using System.Linq;

namespace Epicweb.Optimizely.RedirectManager
{
    public class RedirectManagerQuickNavigator : IQuickNavigatorItemProvider
    {
        public int SortOrder
        {
            get { return 100; }
        }

        public IDictionary<string, QuickNavigatorMenuItem> GetMenuItems(ContentReference currentContent)
        {
            var dictionary = new Dictionary<string, QuickNavigatorMenuItem>();

            if (RedirectManagerOptions.Current.AllowedRoles.Any(role => PrincipalInfo.CurrentPrincipal.IsInRole(role)))
            {
                dictionary.Add("redirectmanager", new QuickNavigatorMenuItem("RedirectManager", "/redirectmanager/", null, "true", null));
            }

            return dictionary;
        }
    }
}