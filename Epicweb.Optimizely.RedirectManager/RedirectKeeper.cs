using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System.Linq;

namespace Epicweb.Optimizely.RedirectManager
{
    public static class RedirectKeeper
    {
        public static bool Enabled { get; set; } = true;
        public static void Page_Moving(object sender, ContentEventArgs e)
        {
            if (Enabled)
            {
                if (!(e.Content is PageData))
                    return;

                var contentLoader = ServiceLocator.Current.GetInstance<IContentRepository>();

                if (e.TargetLink == ContentReference.WasteBasket || contentLoader.Get<PageData>(e.ContentLink.ToPageReference()).IsDeleted)
                    return;

                var pages = contentLoader.GetLanguageBranches<PageData>(e.ContentLink.ToPageReference());

                foreach (PageData page in pages)
                {
                    if (ContentReference.IsNullOrEmpty(page.ArchiveLink)) //skip redirect if archived
                        LogChange(page, true);
                }
            }
        }

        public static void UrlSegment_Changed(object sender, ContentEventArgs e)
        {

            if (!(e.Content is PageData))
                return;

            if (ContentReference.IsNullOrEmpty(e.ContentLink))
                return; //new page

            PageData oldPage = GetLastVersion(e.ContentLink.ToPageReference(), (e.Content as ILocalizable).Language.TwoLetterISOLanguageName) as PageData;

            if (oldPage != null && oldPage.URLSegment != (e.Content as PageData).URLSegment)
                LogChange(oldPage, true);
        }

        public static IContent GetLastVersion(PageReference reference, string lang)
        {
            var versionRepository = ServiceLocator.Current.GetInstance<IContentVersionRepository>();
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            var versions = versionRepository.List(reference);
            var lastVersion = versions
                .OrderBy(v => v.Saved)
                .Take(versions.Count() - 1)
                .OrderByDescending(v => v.Saved)
                .FirstOrDefault(version => version.LanguageBranch == lang);

            if (lastVersion == null)
            {
                //var msg = string.Format("Unable to find last version for ContentReference '{0}'.", reference.ID);
                //throw new Exception(msg);
                return null;
            }

            return contentRepository.Get<IContent>(lastVersion.ContentLink, LanguageSelector.AutoDetect(true));
        }

        private static void LogChange(PageData changedPage, bool wildcard = false)
        {
            var relativeUrl = ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(changedPage.ContentLink)?.ToLower();

            if (relativeUrl == null)
                return;

            if (relativeUrl.Length > 1 && relativeUrl.Last() == '/')
                relativeUrl = relativeUrl.Remove(relativeUrl.Length - 1);

            var redirectService = ServiceLocator.Current.GetInstance<RedirectService>();
            redirectService.AddRedirect(10000,
                EPiServer.Web.SiteDefinition.Current.Name.ToLower(),
                relativeUrl,
                wildcard,
                null,
                changedPage.PageLink.ID,
                changedPage.Language.TwoLetterISOLanguageName);

        }

        public static void Page_Deleted(object sender, DeleteContentEventArgs e)
        {
            using (var context = ServiceLocator.Current.GetInstance<RedirectDbContext>())
            {
                foreach (ContentReference descendent in e.DeletedDescendents)
                {
                    var redirects = context.RedirectRules.Where<RedirectRule>(x => x.ToContentId == descendent.ID).ToList();
                    foreach (var r in redirects)
                    {
                        context.RedirectRules.Remove(r);
                    }
                }
                context.SaveChanges();
            }
        }
    }
}
