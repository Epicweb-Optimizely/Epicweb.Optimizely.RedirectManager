using EPiServer;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Web;
using OfficeOpenXml;
using System.IO;

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

        public byte[] ExportToExcel(bool convertToUrl)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Redirect Rules");
                
                // Add headers
                worksheet.Cells[1, 1].Value = "Order";
                worksheet.Cells[1, 2].Value = "Host";
                worksheet.Cells[1, 3].Value = "From Url";
                worksheet.Cells[1, 4].Value = "Wildcard";
                worksheet.Cells[1, 5].Value = "To Url";
                worksheet.Cells[1, 6].Value = "To Content Id";
                worksheet.Cells[1, 7].Value = "Language";
                
                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }
                
                var rules = List();
                int row = 2;
                
                foreach (var rule in rules)
                {
                    worksheet.Cells[row, 1].Value = rule.SortOrder;
                    worksheet.Cells[row, 2].Value = rule.Host;
                    worksheet.Cells[row, 3].Value = rule.FromUrl;
                    worksheet.Cells[row, 4].Value = rule.Wildcard ? "Yes" : "No";
                    
                    if (convertToUrl && rule.ToContentId > 0)
                    {
                        try
                        {
                            var url = _urlResolver.GetUrl(new ContentReference(rule.ToContentId), rule.ToContentLang);
                            worksheet.Cells[row, 5].Value = url ?? rule.ToUrl;
                            worksheet.Cells[row, 6].Value = 0;
                        }
                        catch
                        {
                            worksheet.Cells[row, 5].Value = rule.ToUrl;
                            worksheet.Cells[row, 6].Value = rule.ToContentId;
                        }
                    }
                    else
                    {
                        worksheet.Cells[row, 5].Value = rule.ToUrl;
                        worksheet.Cells[row, 6].Value = rule.ToContentId;
                    }
                    
                    worksheet.Cells[row, 7].Value = rule.ToContentLang;
                    row++;
                }
                
                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                
                return package.GetAsByteArray();
            }
        }

        public byte[] ExportToCsv(bool convertToUrl)
        {
            var csv = new System.Text.StringBuilder();
            
            // Add headers
            csv.AppendLine("Order,Host,From Url,Wildcard,To Url,To Content Id,Language");
            
            var rules = List();
            
            foreach (var rule in rules)
            {
                var toUrl = rule.ToUrl;
                var toContentId = rule.ToContentId;
                
                if (convertToUrl && rule.ToContentId > 0)
                {
                    try
                    {
                        var url = _urlResolver.GetUrl(new ContentReference(rule.ToContentId), rule.ToContentLang);
                        if (!string.IsNullOrEmpty(url))
                        {
                            toUrl = url;
                            toContentId = 0;
                        }
                    }
                    catch
                    {
                        // Keep original values if conversion fails
                    }
                }
                
                csv.AppendLine($"{rule.SortOrder}," +
                              $"\"{EscapeCsv(rule.Host)}\"," +
                              $"\"{EscapeCsv(rule.FromUrl)}\"," +
                              $"{(rule.Wildcard ? "Yes" : "No")}," +
                              $"\"{EscapeCsv(toUrl)}\"," +
                              $"{toContentId}," +
                              $"\"{EscapeCsv(rule.ToContentLang)}\"");
            }
            
            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }
        
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
                
            return value.Replace("\"", "\"\"");
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
