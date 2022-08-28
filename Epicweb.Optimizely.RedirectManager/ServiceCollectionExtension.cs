using EPiServer.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace Epicweb.Optimizely.RedirectManager
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Add Redirect Manager to Optimizely UI
        /// </summary>
        /// <param name="services"></param>
        /// <param name="addQuickNavigator">Enable Quick nav link, default true</param>
        /// <param name="enableChangeEvent">Enable auto wire up events, default true</param>
        public static void AddRedirectManager(this IServiceCollection services, bool addQuickNavigator = true, bool enableChangeEvent = true)
        {
            services.AddDbContext<RedirectDbContext>();
            services.AddTransient<RedirectService>();
            services.AddSingleton<RedirectRuleStorage>();
            if (addQuickNavigator)
                services.AddTransient<IQuickNavigatorItemProvider, RedirectManagerQuickNavigator>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("episerver:redirectmanager", policy => policy.Requirements.Add(new HasRoleRequirement("RedirectManagers")));
            });

            services.AddSingleton<IAuthorizationHandler, RedirectPermissionHandler>();

            RedirectKeeper.Enabled = enableChangeEvent;

        }

        public class HasRoleRequirement : IAuthorizationRequirement
        {
            public string Role { get; }

            public HasRoleRequirement(string role)
            {
                Role = role;
            }
        }

        public class RedirectPermissionHandler : IAuthorizationHandler
        {
            public Task HandleAsync(AuthorizationHandlerContext context)
            {
                var pendingRequirements = context.PendingRequirements.ToList();

                foreach (var requirement in pendingRequirements)
                {
                    if (requirement is HasRoleRequirement req)
                    {
                        if (context.User.IsInRole(req.Role))
                        {
                            context.Succeed(requirement);
                        }

                        if (context.User.IsInRole("WebAdmins"))
                        {
                            context.Succeed(requirement);
                        }
                    }
                }
                return Task.CompletedTask;
            }
        }
    }
}
