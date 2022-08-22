using Epicweb.Optimizely.RedirectManager;
using EPiServer.Web;
using Microsoft.AspNetCore.Mvc;

namespace Epicweb.Optimizely.Blog.Features.Error
{
    public class ErrorController : Controller
    {
        private readonly IContentRepository _contentRepository;
        private readonly RedirectService _redirectService;

        public ErrorController(IContentRepository contentRepository, RedirectService redirectService)
        {
            _contentRepository = contentRepository;
            _redirectService = redirectService;
        }
        [Route("/Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            ViewBag.Code = statusCode;
            //this is specific redirectManager
            #region RedirectManager
            if (statusCode == 404)
            {
                string originalRelativePath = HttpContext.Request.GetRawUrl();//get current url
                string redirectTo = _redirectService.GetPrimaryRedirectUrlOrDefault(SiteDefinition.Current.Name, originalRelativePath);//check if redirect rule exists
                if (redirectTo != null)
                {
                    Response.Redirect(redirectTo, true);
                }
            } 
            #endregion
            return View("~/Features/Error/Error.cshtml");
        }
    }
}
