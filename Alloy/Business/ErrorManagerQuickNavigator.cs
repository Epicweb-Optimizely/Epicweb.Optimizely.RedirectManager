using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System.Collections.Generic;

namespace Epicweb.Optimizely.RedirectManager
{
    [ServiceConfiguration(ServiceType = typeof(IQuickNavigatorItemProvider))]
    public class ErrorManagerQuickNavigator : IQuickNavigatorItemProvider
    {
        public int SortOrder
        {
            get { return 100; }
        }

        public IDictionary<string, QuickNavigatorMenuItem> GetMenuItems(ContentReference currentContent)
        {
            var dictionary = new Dictionary<string, QuickNavigatorMenuItem>();


            dictionary.Add("Error", new QuickNavigatorMenuItem("Error", "/Error/404", null, "true", null));


            return dictionary;
        }
    }
}