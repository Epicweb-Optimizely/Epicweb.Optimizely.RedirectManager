using Epicweb.Optimizely.RedirectManager;
using EPiServer.Applications;
using EPiServer.Web;
using Microsoft.AspNetCore.Mvc;

namespace Epicweb.Optimizely.Blog.Features.Error
{
    public class ErrorController : Controller
    {
        private readonly IContentRepository _contentRepository;
        private readonly RedirectService _redirectService;
        private readonly IApplicationResolver applicationResolver;

        public ErrorController(IContentRepository contentRepository, RedirectService redirectService, IApplicationResolver applicationResolver)
        {
            _contentRepository = contentRepository;
            _redirectService = redirectService;
            this.applicationResolver = applicationResolver;
        }
        [Route("/Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            ViewBag.Code = statusCode;
            //this is specific redirectManager
            #region RedirectManager
            if (statusCode == 404)
            {
                //var applicationResolver = ServiceLocator.Current.GetInstance<IApplicationResolver>();
                var applicationName = applicationResolver.GetByContext().Name?.ToLower() ?? "*";
                string originalRelativePath = HttpContext.Request.GetRawUrl();//get current url
                string redirectTo = _redirectService.GetPrimaryRedirectUrlOrDefault(applicationName, originalRelativePath);//check if redirect rule exists
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
