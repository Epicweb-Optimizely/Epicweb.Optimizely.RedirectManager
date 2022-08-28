# Epicweb.Optimizely.RedirectManager
This .net 6 library contains a RedirectManager and admin user interface integration in an Optimizely CMS 12 and commerce 14 project. Tested with Alloy. 

[![Platform](https://img.shields.io/badge/Platform-.NET%206-blue.svg?style=flat)](https://msdn.microsoft.com/en-us/library/w0x726c2%28v=vs.110%29.aspx) [![Platform](https://img.shields.io/badge/Optimizely-%2012.6-green.svg?style=flat)](https://world.optimizely.com/products/#contentcloud)

An Optimizely addon that helps with managements of redirects. Simple but yet so effective. It is base out of https://github.com/huilaaja/RedirectManager

**This is the .net 6 version of : https://github.com/huilaaja/RedirectManager ** <-- use this for CMS 11

**Preview:**

![alt text](https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.RedirectManager/blob/main/preview.png?raw=true "This is how the manager looks like")

# Features

- Easily create redirects to any URLs or to Episerver pages.
- Wild card rules.
- Reordering and prioritizing rules.
- Multi-site support.
- Allow moving and changing URLs of Optimizely pages and the redirects still works.
- All redirects are HTTP 301 (Moved permanently), because search engines only follow this kind of redirects.
- Clean up rules functionality (duplicate rules remover)
- Access restrictions allow usage of rule manager to only administrators or redirectmanagers.
- And the most important: It's open Source and it's yours to extend and manipulate! 


**Preview:**

![alt text](https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.RedirectManager/blob/main/preview-create-table.png?raw=true "Click the button the first time")

![alt text](https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.RedirectManager/blob/main/preview-quicknavigation.png?raw=true "Add a quick nav on public site when logged in")

![alt text](https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.RedirectManager/blob/main/preview_remove_rules.png?raw=true "Remove duplicate rules or circular references")

# Installation and configuration 

Available on nuget.optimizely.com https://nuget.optimizely.com/package/?id=Epicweb.Optimizely.RedirectManager

## How to get started?

Start by installing NuGet package:

    Install-Package Epicweb.Optimizely.RedirectManager

Add to startup.cs

    services.AddRedirectManager(
        addQuickNavigator: true, 
        enableChangeEvent: true);

Run your application

First time, you will be prompted to create the redirect table "SEO_redirect"

![alt text](https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.RedirectManager/blob/main/preview-create-table.png?raw=true "Click the button the first time")


### Add code to 404 handler

add this code into your error/404 custom page controller

```
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

```

### Complete example of ErrorController

https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.RedirectManager/tree/main/Alloy/Features/Error

```
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
```

## Roles

Users with role WebAdmins and RedirectManagers will automatically see the menu in Optimizely CMS

# Sandbox alloy app

**Get this solution runing**

1. Clone it

2. Unpack the /alloy/app_data/blobs-and-database.zip

3. dotnet run

4. log in to CMS with "admin" and "Test1234!"

# Package maintainer

https://github.com/lucgosso