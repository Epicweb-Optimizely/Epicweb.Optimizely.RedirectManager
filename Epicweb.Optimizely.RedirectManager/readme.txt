ONE MORE THING .... 

add this code into your error/404 custom page controller
full example here: https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.RedirectManager/tree/main/Alloy/Features/Error 

'

using Epicweb.Optimizely.RedirectManager;

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

Add to startup.cs

'
using Epicweb.Optimizely.RedirectManager;

services.AddRedirectManager(
    addQuickNavigator: true, 
    enableChangeEvent: true);
'

also
'
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        //remember if you use env.IsDevelopment() do activate error pages in dev env too
        //do NOT use => app.UseStatusCodePagesWithRedirects("/Error/{0}");//not redirects
        app.UseStatusCodePagesWithReExecute("/Error/{0}");// <-- important
        app.UseExceptionHandler("/Error/500");
    }
'
Happy redirecting! 

Have you looked at this awesome plugin yet? => https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.QuickNavExtension

Check out AI-Assistant for Optimizely => https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.AIAssistant
