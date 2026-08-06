using EPiServer;
using EPiServer.Applications;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System;
using System.Globalization;
using System.Linq;

namespace Epicweb.Optimizely.RedirectManager
{
    public static class RedirectKeeper
    {
        //enum for two letter, three letter and name
        public enum LangParam
        {
            TwoLetter,
            ThreeLetter,
            Name
        }

        public static LangParam LangParameter { get; set; } = LangParam.TwoLetter;
        public static bool Enabled { get; set; } = true;

        public static void Page_Moving(object sender, ContentEventArgs e)
        {
            if (!Enabled || e?.Content is not PageData page)
            {
                return;
            }

            var contentLoader = ServiceLocator.Current.GetInstance<IContentRepository>();

            if (e.TargetLink == ContentReference.WasteBasket || contentLoader.Get<PageData>(e.ContentLink).IsDeleted)
            {
                return;
            }

            var pages = contentLoader.GetLanguageBranches<PageData>(e.ContentLink);

            foreach (PageData pageInLanguage in pages)
            {
                if (ContentReference.IsNullOrEmpty(pageInLanguage.ArchiveLink))
                {
                    LogChange(pageInLanguage, true);
                }
            }
        }

        public static void UrlSegment_Changed(object sender, ContentEventArgs e)
        {
            if (e?.Content is not PageData pageData || ContentReference.IsNullOrEmpty(e.ContentLink))
            {
                return;
            }

            var previousPage = GetLastVersion(e.ContentLink, pageData.Language?.TwoLetterISOLanguageName ?? string.Empty) as PageData;

            if (previousPage != null && previousPage.URLSegment != pageData.URLSegment)
            {
                LogChange(previousPage, true);
            }
        }

        public static IContent? GetLastVersion(ContentReference reference, string lang)
        {
            if (ContentReference.IsNullOrEmpty(reference))
            {
                return null;
            }

            var versionRepository = ServiceLocator.Current.GetInstance<IContentVersionRepository>();
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            var versions = versionRepository.List(reference).ToList();

            if (versions.Count <= 1)
            {
                return null;
            }

            var lastVersion = versions
                .OrderBy(v => v.Saved)
                .Take(versions.Count - 1)
                .OrderByDescending(v => v.Saved)
                .FirstOrDefault(version => string.Equals(version.LanguageBranch, lang, StringComparison.OrdinalIgnoreCase));

            if (lastVersion == null)
            {
                return null;
            }

            return contentRepository.Get<IContent>(lastVersion.ContentLink, LanguageSelector.AutoDetect(true));
        }

        private static void LogChange(PageData changedPage, bool wildcard = false)
        {
            if (changedPage == null)
            {
                return;
            }

            var relativeUrl = ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(changedPage.ContentLink)?.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(relativeUrl))
            {
                return;
            }

            if (relativeUrl.Length > 1 && relativeUrl.Last() == '/')
            {
                relativeUrl = relativeUrl.Remove(relativeUrl.Length - 1);
            }

            var redirectService = ServiceLocator.Current.GetInstance<RedirectService>();
            var hostName = GetApplicationName();

            redirectService.AddRedirect(
                10000,
                hostName,
                relativeUrl,
                wildcard,
                null,
                changedPage.ContentLink.ID,
                GetLangParameter(changedPage.Language));
        }

        private static string GetApplicationName()
        {
            var applicationResolver = ServiceLocator.Current.GetInstance<IApplicationResolver>();
            var application = applicationResolver.GetByContext();

            return application?.Name?.Trim().ToLowerInvariant() ?? string.Empty;
        }

        private static string GetLangParameter(CultureInfo language)
        {
            ArgumentNullException.ThrowIfNull(language);

            if (LangParameter == LangParam.ThreeLetter)
            {
                return language.ThreeLetterISOLanguageName;
            }

            if (LangParameter == LangParam.Name)
            {
                return language.Name;
            }

            return language.TwoLetterISOLanguageName;
        }

        public static void Page_Deleted(object sender, DeleteContentEventArgs e)
        {
            var context = ServiceLocator.Current.GetInstance<RedirectDbContext>();

            foreach (ContentReference descendent in e.DeletedDescendents)
            {
                var redirects = context.RedirectRules.Where(x => x.ToContentId == descendent.ID).ToList();
                foreach (var r in redirects)
                {
                    context.RedirectRules.Remove(r);
                }
            }

            context.SaveChanges();
        }
    }
}
