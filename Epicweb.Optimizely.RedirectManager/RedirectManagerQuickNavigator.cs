using EPiServer.Core;
using EPiServer.Security;
using EPiServer.Web;
using System.Collections.Generic;

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

            if (PrincipalInfo.CurrentPrincipal.IsInRole("CmsAdmins") || PrincipalInfo.CurrentPrincipal.IsInRole("RedirectManagers"))
            {
                dictionary.Add("redirectmanager", new QuickNavigatorMenuItem("RedirectManager", "/redirectmanager/", null, "true", null));
            }

            return dictionary;
        }
    }
}