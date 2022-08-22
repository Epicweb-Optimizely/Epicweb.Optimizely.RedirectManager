using Epicweb.Optimizely.RedirectManager;
using Epicweb.Optimizely.RedirectManager.Alloy.Extensions;
using EPiServer.Cms.Shell;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace Epicweb.Optimizely.RedirectManager.Alloy;

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

        services.AddRedirectManager(addQuickNavigator: true, EnableChangeEvent: true);

        services
            .AddCmsAspNetIdentity<ApplicationUser>()
            .AddCms()
            .AddAlloy()
            .AddAdminUserRegistration()
            .AddEmbeddedLocalization<Startup>();

        // Required by Wangkanai.Detection
        services.AddDetection();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });


        services.AddControllersWithViews().AddRazorRuntimeCompilation();

        var viewAssembly = typeof(Epicweb.Optimizely.RedirectManager.RedirectKeeper).GetTypeInfo().Assembly;
        //var fileProvider = new EmbeddedFileProvider(viewAssembly);
        //services.Configure<RazorViewEngineOptions>(options => {
        //    options.FileProviders.Add(fileProvider);
        //});


        //IFileProvider physicalProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
        //IFileProvider embeddedProvider = new EmbeddedFileProvider(viewAssembly);
        //IFileProvider compositeProvider = new CompositeFileProvider(physicalProvider, embeddedProvider);
        ////IFileProvider embeddedProvider = new EmbeddedFileProvider(viewAssembly);
        //services.AddSingleton<IFileProvider>(compositeProvider);
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
            endpoints.MapContent();
        });
    }
}
