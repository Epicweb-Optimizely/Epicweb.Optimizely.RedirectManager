using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace Epicweb.Optimizely.RedirectManager
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class RedirectKeeperInit : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            if (RedirectKeeper.Enabled)
            {
                IContentEvents events = ServiceLocator.Current.GetInstance<IContentEvents>();
                events.MovingContent += RedirectKeeper.Page_Moving;
                events.PublishingContent += RedirectKeeper.UrlSegment_Changed;
                events.DeletedContent += RedirectKeeper.Page_Deleted;
            }
        }

        public void Uninitialize(InitializationEngine context)
        {
            if (RedirectKeeper.Enabled)
            {
                IContentEvents events = ServiceLocator.Current.GetInstance<IContentEvents>();
                events.MovingContent -= RedirectKeeper.Page_Moving;
                events.PublishingContent -= RedirectKeeper.UrlSegment_Changed;
                events.DeletedContent -= RedirectKeeper.Page_Deleted;
            }
        }
    }
}
