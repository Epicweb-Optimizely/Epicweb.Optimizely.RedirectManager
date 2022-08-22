using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}
