using EPiServer;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Web;

namespace Epicweb.Optimizely.RedirectManager
{
    public class RedirectService
    {
        private readonly UrlResolver _urlResolver;
        private readonly IContentRepository _contentRepository;
        private readonly ISiteDefinitionRepository _siteDefinitionRepository;
        private readonly RedirectRuleStorage RedirectRuleStorage;
        private readonly RedirectDbContext Context;
        public RedirectService(UrlResolver urlResolver, IContentRepository contentRepository, ISiteDefinitionRepository siteDefinitionRepository,
            RedirectRuleStorage redirectStorage,
            RedirectDbContext _context)
        {
            _urlResolver = urlResolver;
            _contentRepository = contentRepository;
            _siteDefinitionRepository = siteDefinitionRepository;
            RedirectRuleStorage = redirectStorage;
            RedirectRuleStorage.Init();
            Context = _context;
        }

        public RedirectRule GetRedirect(int id)
        {
            if (!RedirectRuleStorage.IsUpToDate) return null;
            return Context.RedirectRules.FirstOrDefault(x => x.Id == id);

        }

        public RedirectRule[] List()
        {
            if (!RedirectRuleStorage.IsUpToDate) return new RedirectRule[] { };

            return Context.RedirectRules.AsQueryable()
                            .OrderBy(x => x.SortOrder)
                            .ThenBy(x => x.Host)
                            .ThenBy(x => x.FromUrl)
                            .ToArray();
        }


        public int AddRedirect(int? sortOrder, string host, string fromUrl, bool? wildcard, string toUrl, int? toContentId, string toContentLang)
        {
            if (string.IsNullOrEmpty(toUrl) && toContentId.GetValueOrDefault(0) <= 0)
                return 0;
            if (string.IsNullOrEmpty(toContentLang))
                toContentLang = null;
            if (string.IsNullOrEmpty(fromUrl))
                fromUrl = "/";
            if (!fromUrl.StartsWith("/"))
                fromUrl = "/" + fromUrl;
            var r = new RedirectRule()
            {
                SortOrder = sortOrder.GetValueOrDefault(0),
                Host = host,
                FromUrl = fromUrl,
                Wildcard = wildcard.GetValueOrDefault(false),
                ToUrl = toUrl,
                ToContentId = toContentId.GetValueOrDefault(0),
                ToContentLang = toContentLang,
            };
            Context.RedirectRules.Add(r);
            return Context.SaveChanges();
        }

        public int ModifyRedirect(int id, int? sortOrder, string host, string fromUrl, bool? wildcard, string toUrl, int? toContentId, string toContentLang)
        {
            if (string.IsNullOrEmpty(toUrl) && toContentId.GetValueOrDefault(0) <= 0)
                return 0;
            if (string.IsNullOrEmpty(toContentLang))
                toContentLang = null;
            if (string.IsNullOrEmpty(fromUrl))
                fromUrl = "/";
            if (!fromUrl.StartsWith("/"))
                fromUrl = "/" + fromUrl;

            var r = Context.RedirectRules.First(x => x.Id == id);
            r.SortOrder = sortOrder.GetValueOrDefault(0);
            r.FromUrl = fromUrl;
            r.Host = host;
            r.Wildcard = wildcard.GetValueOrDefault(false);
            r.ToUrl = toUrl;
            r.ToContentId = toContentId.GetValueOrDefault(0);
            r.ToContentLang = toContentLang;
            Context.Entry(r).State = EntityState.Modified;
            return Context.SaveChanges();

        }

        public int DeleteRedirect(int id)
        {
            var r = Context.RedirectRules.First(x => x.Id == id);
            Context.RedirectRules.Remove(r);
            return Context.SaveChanges();

        }

        public string GetPrimaryRedirectUrlOrDefault(string host, string relativeUrl)
        {
            if (!RedirectRuleStorage.IsUpToDate) return null;
            if (string.IsNullOrEmpty(relativeUrl))
                return null;
            if (relativeUrl.Length > 1 && relativeUrl.Last() == '/')
                relativeUrl = relativeUrl.Remove(relativeUrl.Length - 1);
            relativeUrl = HttpUtility.UrlDecode(relativeUrl.ToLower());
            var exactMatch = Context.RedirectRules.AsQueryable()
                            .Where(x => x.Host == null || x.Host == "*" || x.Host == host.ToLower())
                            .Where(x => x.FromUrl == relativeUrl.ToLower())
                            .OrderBy(x => x.SortOrder)
                            .ThenBy(x => x.FromUrl)
                            .FirstOrDefault();

            var wildcards = Context.RedirectRules.AsQueryable()
                            .Where(x => x.Host == null || x.Host == "*" || x.Host == host)
                            .Where(x => x.Wildcard)
                            .OrderBy(x => x.SortOrder)
                            .ThenBy(x => x.FromUrl);
            var match = wildcards.FirstOrDefault(x => relativeUrl.StartsWith(x.FromUrl)
                                                    || relativeUrl == x.FromUrl);

            RedirectRule theMatch = exactMatch != null && match != null
                                ? exactMatch.SortOrder <= match.SortOrder ? exactMatch : match
                                : exactMatch ?? match;
            if (theMatch == null) return null;

            return theMatch.ToContentId > 0
                    ? _urlResolver.GetUrl(new ContentReference(theMatch.ToContentId), theMatch.ToContentLang)
                    : theMatch.ToUrl;

        }

        public string[] GetGlobalLanguageOptions()
        {
            return _contentRepository.GetLanguageBranches<PageData>(ContentReference.StartPage)
                                    .Select(branch => ((ILocalizable)branch).Language.Name)
                                    .ToArray();
        }

        public string[] GetGlobalHostOptions()
        {
            return _siteDefinitionRepository.List()
                                            .Select(h => h.Name.ToLower())
                                            .Where(h => !h.Equals("*", StringComparison.InvariantCultureIgnoreCase))
                                            .OrderBy(h => h)
                                            .ToArray();
        }

        /// <summary>
        /// Cleaning rules (remove duplicates, remove deleted pages, Remove self references)
        /// </summary>
        /// <returns></returns>
        public int CleanupRulesJob()
        {
            int counter = RemoveAllDuplicateRules();

            foreach (var rule in List())
            {
                if (rule.ToContentId > 0)
                {
                    if (_contentRepository.TryGet(new ContentReference(rule.ToContentId), out IContent content))
                    {
                        //check if url is selfpointing
                        var url = _urlResolver.GetUrl(new ContentReference(rule.ToContentId));
                        if (url == null || url == rule.FromUrl + "/")
                        {
                            DeleteRedirect(rule.Id);
                            counter++;
                        }
                    }
                    else
                    {
                        //content removed
                        DeleteRedirect(rule.Id);
                        counter++;
                    }
                }
            }
            return counter;
        }

        public int RemoveAllDuplicateRules()
        {
            int counter = 0;

            var rules = Context.RedirectRules.FromSqlRaw(@"select *
                                            from [" + RedirectRuleStorage.RedirectTableName + @"]
                                            where ([FromUrl] IN
                                        (
                                            SELECT [FromUrl]
                                            FROM ["+ RedirectRuleStorage.RedirectTableName + @"] 
                                        GROUP BY
                                                [SortOrder]
                                                ,[Host]
                                                ,[FromUrl]
                                                ,[ToUrl]
                                                ,[ToContentId]
                                                ,[ToContentLang]
                                            ,[Wildcard]
                                        HAVING 
                                            COUNT(*) > 1
                                            )
                                            )
                                            order by [FromUrl]").ToList();
            RedirectRule workingRule = null;

            foreach (var rule in rules)
            {
                if (workingRule != null && workingRule.FromUrl == rule.FromUrl)
                {
                    DeleteRedirect(rule.Id);
                    counter++;
                }
                else if (workingRule == null)
                {
                    workingRule = rule;
                }
                else if (workingRule.FromUrl != rule.FromUrl)
                {
                    workingRule = rule;
                }
            }
            return counter;
        }

    }

    public class RedirectRuleStorage
    {
        public const string RedirectTableName = "SEO_Redirect";
        public bool IsUpToDate => TableExist;
        public bool TableExist { get; private set; }
        public RedirectDbContext Context { get; private set; }
        public RedirectRuleStorage(RedirectDbContext context)
        {
            Context = context;
            if (!IsUpToDate)
            {
                TableExist = RedirectTableExists();
            }
        }
        public void Init()
        {
            if (!IsUpToDate)
            {
                TableExist = RedirectTableExists();
            }
        }

        public bool CreateTable()
        {
            Context.Database.ExecuteSqlRaw(
                @"CREATE TABLE [dbo].[" + RedirectTableName + @"](
                    [Id][int] IDENTITY(1, 1) NOT NULL,
                    [SortOrder][int] NOT NULL,
                    [Host][nvarchar](max) NULL,
                    [FromUrl][nvarchar](max) NULL,
                    [ToUrl][nvarchar](max) NULL,
                    [ToContentId][int] NOT NULL,
                    [ToContentLang][nvarchar](10) NULL,
                 [Wildcard] [bit] NOT NULL DEFAULT ((0)),
                 CONSTRAINT[PK_dbo." + RedirectTableName + @"] PRIMARY KEY CLUSTERED
                ( [Id] ASC)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]
                ");
            return TableExist = true;

        }
        public bool RedirectTableExists()
        {               
            try
            {
                Context.RedirectRules.Any();//check if table exists, throws if none existing
                return TableExist = true;
            }
            catch (SqlException)
            {
                //Microsoft.Data.SqlClient.SqlException: 'Invalid object name 'SEO_Redirect'.'
            }
            return TableExist = false;
        }
    }
}
