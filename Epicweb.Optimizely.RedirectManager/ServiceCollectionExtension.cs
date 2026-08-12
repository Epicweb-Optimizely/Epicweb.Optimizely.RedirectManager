using EPiServer.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Epicweb.Optimizely.RedirectManager.RedirectKeeper;

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
        /// <param name="langParam">If you have complex language setup, change to Name or ThreeLetter</param>
        public static void AddRedirectManager(this IServiceCollection services, bool addQuickNavigator = true, bool enableChangeEvent = true, LangParam langParam = LangParam.TwoLetter)
        {
            services.AddRedirectManager(options =>
            {
                options.AddQuickNavigator = addQuickNavigator;
                options.EnableChangeEvent = enableChangeEvent;
                options.LangParam = langParam;
            });
        }

        /// <summary>
        /// Add Redirect Manager to Optimizely UI
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure">Configure roles, authentication schemes (e.g. for Opti ID) and other options</param>
        public static void AddRedirectManager(this IServiceCollection services, Action<RedirectManagerOptions> configure)
        {
            var options = new RedirectManagerOptions();
            configure?.Invoke(options);
            RedirectManagerOptions.Current = options;

            services.AddDbContext<RedirectDbContext>();
            services.AddTransient<RedirectService>();
            services.AddSingleton<RedirectRuleStorage>();
            if (options.AddQuickNavigator)
                services.AddTransient<IQuickNavigatorItemProvider, RedirectManagerQuickNavigator>();

            services.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.AddPolicy(RedirectManagerOptions.AuthorizationPolicyName, policy =>
                {
                    if (options.AuthenticationSchemes.Length > 0)
                        policy.AddAuthenticationSchemes(options.AuthenticationSchemes);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new HasRoleRequirement(options.AllowedRoles));
                });
            });

            services.AddSingleton<IAuthorizationHandler, RedirectPermissionHandler>();

            RedirectKeeper.Enabled = options.EnableChangeEvent;
            RedirectKeeper.LangParameter = options.LangParam;

        }

        public class HasRoleRequirement : IAuthorizationRequirement
        {
            public string[] Roles { get; }

            public HasRoleRequirement(params string[] roles)
            {
                Roles = roles ?? Array.Empty<string>();
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
                        if (req.Roles.Any(role => context.User.IsInRole(role)))
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
