using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Epicweb.Optimizely.RedirectManager
{
    [Authorize(Roles = "CmsAdmins,WebAdmins,Administrators,RedirectManagers")]

    public class RedirectManagerController : Controller
    {
        private readonly RedirectService _redirectService;

        public RedirectManagerController(RedirectService redirectService)
        {
            _redirectService = redirectService;
        }

        [Route("/redirectmanager")]
        public ActionResult Index()
        {
            var model = new RedirectManagerViewModel();
            return View("/RedirectManagerIndex.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/redirectmanager/create")]
        public ActionResult Create(RedirectManagerViewModel model)
        {
            try
            {
                if (model.Id > 0)
                {
                    var effected = _redirectService.ModifyRedirect(model.Id, model.SortOrder, model.Host, model.FromUrl, model.WildCard, model.ToUrl, model.ToConentId, model.ToContentLang);
                }
                else
                {
                    int n = _redirectService.AddRedirect(model.SortOrder,
                        model.Host,
                        model.FromUrl,
                        model.WildCard,
                        model.ToUrl,
                        model.ToConentId,
                        model.ToContentLang
                        );

                }

            }
            catch
            { }

            return RedirectToAction("Index", "Redirectmanager");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/redirectmanager/export")]
        public IActionResult Export(string format, bool convertToUrl)
        {
            try
            {
                byte[] fileBytes;
                string fileName;
                string contentType;
                
                if (format?.ToLower() == "csv")
                {
                    fileBytes = _redirectService.ExportToCsv(convertToUrl);
                    fileName = $"RedirectRules_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    contentType = "text/csv";
                }
                else
                {
                    fileBytes = _redirectService.ExportToExcel(convertToUrl);
                    fileName = $"RedirectRules_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
                
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Export failed: {ex.Message}";
                return RedirectToAction("Index", "Redirectmanager");
            }
        }
    }
}
