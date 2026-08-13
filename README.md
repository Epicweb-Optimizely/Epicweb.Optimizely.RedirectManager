# Epicweb.Optimizely.RedirectManager
This .NET 10 library contains a RedirectManager and admin user interface integration for Optimizely CMS 13 (and Commerce 14). If you are running Optimizely CMS 12, use package version 6.x instead.

[![Platform](https://img.shields.io/badge/Platform-.NET%2010-blue.svg?style=flat)](https://learn.microsoft.com/dotnet/) [![Platform](https://img.shields.io/badge/Optimizely-%2013.1-green.svg?style=flat)](https://world.optimizely.com/products/#contentcloud) [![Twitter Follow](https://img.shields.io/twitter/follow/lucgosso.svg?style=social&label=Follow)](https://twitter.com/lucgosso)

An Optimizely addon that helps with management of redirects. Simple but yet so effective. It is based on https://github.com/huilaaja/RedirectManager

**This is the CMS 13 / .NET 10 version of https://github.com/huilaaja/RedirectManager**

**For CMS 12, use version 6.x.**

**Preview:**

![alt text](https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.RedirectManager/blob/main/preview.png?raw=true "This is how the manager looks like")

# Features

- Easily create redirects to any URLs or to Optimizely CMS pages, products, images and documents.
- Wild card rules.
- Reordering and prioritizing rules.
- Multi-site and lang support.
- Allow moving and changing URLs of Optimizely pages and the redirects still works.
- All redirects are HTTP 301 (Moved permanently), because search engines only follow this kind of redirects.
- Clean up rules functionality (duplicate rules remover)
- **Search functionality** - Quickly find redirect rules by searching From Url, To Url, or To Content Id with real-time filtering.
- **Export rules to Excel** - Export all redirect rules to Excel format with optional URL conversion for Content IDs.
- **Import rules from Excel** - Import redirect rules from Excel files with update or replace modes.
- Access restrictions allow usage of rule manager to only administrators or redirectmanagers.
- And the most important: It's open Source and it's yours to extend and manipulate!


**Preview:**

![alt text](https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.RedirectManager/blob/main/preview-create-table.png?raw=true "Click the button the first time")

![alt text](https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.RedirectManager/blob/main/preview_remove_rules.png?raw=true "Remove duplicate rules or circular references")

![alt text](https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.RedirectManager/blob/main/preview-QuickNavigation.png?raw=true "Add a quick nav on public site when logged in")

<img width="761" height="398" alt="image" src="https://github.com/user-attachments/assets/aea4b6c7-8735-45ec-97df-9eb88bbd4745" />


# Installation and configuration

Available on nuget.optimizely.com https://nuget.optimizely.com/package/?id=Epicweb.Optimizely.RedirectManager

## How to get started?

Start by installing NuGet package:

    Install-Package Epicweb.Optimizely.RedirectManager

Add to startup.cs

    services.AddRedirectManager(
        addQuickNavigator: true, 
        enableChangeEvent: true,
        langParam: RedirectKeeper.LangParam.Name);//if you have complex language setup, change to Name or ThreeLetter, default is TwoLetter

or use the options overload for full control (roles, authentication schemes):

    services.AddRedirectManager(options =>
    {
        options.AddQuickNavigator = true;
        options.EnableChangeEvent = true;
        options.LangParam = RedirectKeeper.LangParam.Name;
        options.AllowedRoles = new[] { "RedirectManagers", "CmsAdmins", "WebAdmins", "Administrators" };//default
        options.AuthenticationSchemes = Array.Empty<string>();//default, see Opti ID section below
    });

also

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        //remember if you use env.IsDevelopment() do activate error pages in dev env too
        //do NOT use => app.UseStatusCodePagesWithRedirects("/Error/{0}");//not redirects
        app.UseStatusCodePagesWithReExecute("/Error/{0}");
        app.UseExceptionHandler("/Error/500");
    }

Run your application

First time, you will be prompted to create the redirect table "SEO_redirect"

![alt text](https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.RedirectManager/blob/main/preview-create-table.png?raw=true "Click the button the first time")

### Upgraded from .netFramework 4?

That should not be a problem. If you used the Solita solution, change the name of the table "SOLITA_Redirect" to "SEO_Redirect", should be the same schema if you run on latest solution, make sure you have run this V2 upgrade before, (or added the host column manually) [https://github.com/huilaaja/RedirectManager/blob/c7ec6ea4b12aa36b53b27fa89bb373286fe0d53d/WebProject/Redirects/RedirectService.cs#L282]

Schema should look like this:

![image](https://user-images.githubusercontent.com/9716195/231706843-b4b5e9f2-d32f-41d4-9c79-09371f1b105d.png)

### Add code to 404 handler

add this code into your error/404 custom page controller

```
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

```

### Complete example of ErrorController

https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.RedirectManager/tree/main/Alloy/Features/Error

```csharp
using Epicweb.Optimizely.RedirectManager;
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
```

## Export and Import

### Export Redirect Rules

The Redirect Manager includes a powerful export feature that allows you to:
- Export all redirect rules to an Excel (.xlsx) file
- **Convert to URL** option: Automatically converts `ToContentId` references to their actual URLs
  - When enabled, content ID references are resolved to full URLs
  - The exported `ToContentId` column will be set to 0 for converted entries
  - Useful when migrating rules between environments or for backup purposes

**Excel Format:**
The exported file contains the following columns:
- Order
- Host
- From Url
- Wildcard (Yes/No)
- To Url
- To Content Id
- Language
  
<img width="734" height="284" alt="image" src="https://github.com/user-attachments/assets/78539532-791a-468d-b066-10da1d3dd899" />


### Import Redirect Rules

<img width="737" height="717" alt="image" src="https://github.com/user-attachments/assets/8105b273-a0a5-4ad0-8be1-364a1c89d32a" />

Import redirect rules from Excel (.xlsx) files with two different modes:

**Update Mode (Default):**
- Existing rules (matched by FromUrl and Host) will be updated with new values
- Rules not found will be added as new entries
- Preserves other existing rules that are not in the import file

**Replace Mode (Start from blank):**
- ?? **Warning:** Deletes ALL existing redirect rules before importing
- Completely replaces your redirect configuration
- Use with caution - this action cannot be undone!

**Excel Requirements:**
- Must include header row with columns: Order, Host, From Url, Wildcard, To Url, To Content Id, Language
- `From Url` and either `To Url` or `To Content Id` are required
- Wildcard should be `Yes` or `No`
- Host can be `*` for all domains or a specific site name
- Language is optional (for Content ID redirects)

**Import Results:**
After import, you'll see a summary showing:
- Number of rules imported (new)
- Number of rules updated
- Number of rules skipped (invalid)
- Number of errors encountered

## Roles and restrictions

Users with role RedirectManagers, CmsAdmins, WebAdmins or Administrators will automatically see the menu in Optimizely CMS.

The roles can be changed via the options overload:

    services.AddRedirectManager(options =>
    {
        options.AllowedRoles = new[] { "RedirectManagers", "CmsAdmins" };
    });

## Using with Opti ID

If your site uses Opti ID (`EPiServer.OptimizelyIdentity`) instead of ASP.NET Identity, note the following:

- Opti ID only maps the virtual roles **CmsAdmins** and **CmsEditors** automatically. Roles like `WebAdmins` and `Administrators` do not exist. Users with CMS admin access will get access to the Redirect Manager out of the box (via `CmsAdmins`).
- To grant access to non-admins, create a custom role named **RedirectManagers** in the Opti ID Admin Center and assign it to users or groups. The role syncs to the CMS when the user logs in.
- If Opti ID is your default authentication scheme (`services.AddOptimizelyIdentity(useAsDefault: true)`), no further configuration is needed.
- If you run **mixed-mode** authentication (`useAsDefault: false`, e.g. because your site has front-end visitor login), the `/redirectmanager` route is not covered by the Opti ID scheme automatically since it is not a protected shell module. Tell the Redirect Manager to authorize against the Opti ID scheme:

```csharp
using EPiServer.OptimizelyIdentity;

services.AddRedirectManager(options =>
{
    options.AuthenticationSchemes = new[] { OptimizelyIdentityDefaults.SchemeName };
});
```

# Sandbox alloy app

**Get this solution running**

1. Clone it

2. Create a empty database and update connection string in appsettings.json

3. Make sure the DefaultSiteContent.episerverdata is in app_data folder

4. dotnet run

5. register an admin user to login

# Package maintainer

https://github.com/lucgosso
