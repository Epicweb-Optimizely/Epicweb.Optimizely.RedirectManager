using Alloy.Cms13.Extensions;
using Epicweb.Optimizely.RedirectManager;
using EPiServer.Cms.Shell;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.DependencyInjection;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace Alloy.Cms13;

public class Startup
{
    private readonly IWebHostEnvironment _webHostingEnvironment;

    public Startup(IWebHostEnvironment webHostingEnvironment)
    {
        _webHostingEnvironment = webHostingEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        if (_webHostingEnvironment.IsDevelopment())
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(_webHostingEnvironment.ContentRootPath, "App_Data"));

            services.Configure<SchedulerOptions>(options => options.Enabled = false);
        }

        services.AddRedirectManager(
    addQuickNavigator: true,
    enableChangeEvent: true,
    langParam: RedirectKeeper.LangParam.Name);//if you have complex language setup, change to Name or ThreeLetter

        // Using Opti ID (EPiServer.OptimizelyIdentity) instead of ASP.NET Identity?
        // 1. Remove .AddCmsAspNetIdentity<ApplicationUser>() below and add:
        //      services.AddOptimizelyIdentity(useAsDefault: true);
        // 2. If you run mixed-mode (useAsDefault: false, e.g. front-end visitor login),
        //    tell the redirect manager to authorize against the Opti ID scheme:
        //      services.AddRedirectManager(options =>
        //      {
        //          options.LangParam = RedirectKeeper.LangParam.Name;
        //          options.AuthenticationSchemes = new[] { OptimizelyIdentityDefaults.SchemeName };
        //      });
        // 3. Create a "RedirectManagers" custom role in the Opti ID Admin Center,
        //    or rely on CmsAdmins which Opti ID maps automatically.

        services
            .AddCmsAspNetIdentity<ApplicationUser>()
            .AddCms()
            .AddAlloy()
            .AddAdminUserRegistration()
            .AddEmbeddedLocalization<Startup>();

        //services.AddVisitorGroups();

        // Required by Wangkanai.Detection
        services.AddDetection();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseStatusCodePagesWithReExecute("/Error/{0}");
        app.UseExceptionHandler("/Error/500");

        //if (env.IsDevelopment())
        //{
        app.UseDeveloperExceptionPage();
        //}

        // Required by Wangkanai.Detection
        app.UseDetection();
        app.UseSession();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapContent();
        });
    }
}
