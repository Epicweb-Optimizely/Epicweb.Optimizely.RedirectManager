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
    enableChangeEvent: true,
    langParam: RedirectKeeper.LangParam.Name);//if you have complex language setup, change to Name or ThreeLetter, default is TwoLetter
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
======================================
NEW IN VERSION 6.4.0
======================================

EXPORT RULES
- Export all redirect rules to Excel (.xlsx) format
- Option to convert Content IDs to URLs during export
- Perfect for backups, documentation, and sharing between environments

IMPORT RULES
- Import redirect rules from Excel (.xlsx) files
- Two modes: Update existing rules or Replace all (bulk operations)
- File format: Order, Host, From Url, Wildcard, To Url, To Content Id, Language
- Detailed error reporting for each row
- Ideal for bulk updates and environment migrations

======================================

## New in v6.4
- Search functionality: Quickly find redirect rules with real-time search filtering by From Url, To Url, or To Content Id
- Search panel for easy access to your redirect rules
- Excel export and import: Backup and restore redirect rules using Excel files (.xlsx)
- Convert to URL option: Export with automatic Content ID to URL conversion

Happy redirecting! 

Have you looked at this awesome plugin yet? => https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.QuickNavExtension

Check out AI-Assistant for Optimizely => https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.AIAssistant
