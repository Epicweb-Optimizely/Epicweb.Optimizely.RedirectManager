using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/redirectmanager/import")]
        public IActionResult Import(IFormFile importFile, bool removeAllRules)
        {
            try
            {
                if (importFile == null || importFile.Length == 0)
                {
                    TempData["ImportMessage"] = "Please select a file to import.";
                    TempData["ImportMessageType"] = "danger";
                    return RedirectToAction("Index", "Redirectmanager");
                }

                // Check file extension
                var extension = Path.GetExtension(importFile.FileName).ToLower();
                if (extension != ".xlsx" && extension != ".csv")
                {
                    TempData["ImportMessage"] = "Invalid file format. Please upload an Excel (.xlsx) or CSV (.csv) file.";
                    TempData["ImportMessageType"] = "danger";
                    return RedirectToAction("Index", "Redirectmanager");
                }

                ImportResult result;
                using (var stream = importFile.OpenReadStream())
                {
                    if (extension == ".csv")
                    {
                        result = _redirectService.ImportFromCsv(stream, removeAllRules);
                    }
                    else
                    {
                        result = _redirectService.ImportFromExcel(stream, removeAllRules);
                    }
                }

                if (result.Success)
                {
                    TempData["ImportMessage"] = result.GetSummary();
                    TempData["ImportMessageType"] = "success";
                    
                    if (result.Errors.Count > 0)
                    {
                        TempData["ImportErrors"] = string.Join("<br/>", result.Errors);
                    }
                }
                else
                {
                    TempData["ImportMessage"] = result.GetSummary();
                    TempData["ImportMessageType"] = "danger";
                    
                    if (result.Errors.Count > 0)
                    {
                        TempData["ImportErrors"] = string.Join("<br/>", result.Errors);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ImportMessage"] = $"Import failed: {ex.Message}";
                TempData["ImportMessageType"] = "danger";
            }

            return RedirectToAction("Index", "Redirectmanager");
        }
    }
}
