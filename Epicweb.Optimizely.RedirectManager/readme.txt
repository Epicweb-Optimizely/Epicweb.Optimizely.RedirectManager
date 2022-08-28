ONE MORE THING .... 

add this code into your error/404 custom page controller

'
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

'

Happy redirecting! 

Have you looked at this awesome plugin yet? => https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.QuickNavExtension 
