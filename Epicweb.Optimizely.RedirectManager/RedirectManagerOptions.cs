using System;
using static Epicweb.Optimizely.RedirectManager.RedirectKeeper;

namespace Epicweb.Optimizely.RedirectManager
{
    public class RedirectManagerOptions
    {
        public const string AuthorizationPolicyName = "episerver:redirectmanager";

        /// <summary>
        /// Enable Quick nav link, default true
        /// </summary>
        public bool AddQuickNavigator { get; set; } = true;

        /// <summary>
        /// Enable auto wire up events, default true
        /// </summary>
        public bool EnableChangeEvent { get; set; } = true;

        /// <summary>
        /// If you have complex language setup, change to Name or ThreeLetter
        /// </summary>
        public LangParam LangParam { get; set; } = LangParam.TwoLetter;

        /// <summary>
        /// Roles that are granted access to the Redirect Manager UI and menu items.
        /// When using Opti ID, only CmsAdmins/CmsEditors are mapped automatically;
        /// create a custom role (e.g. RedirectManagers) in the Opti ID Admin Center.
        /// </summary>
        public string[] AllowedRoles { get; set; } = new[] { "RedirectManagers", "CmsAdmins", "WebAdmins", "Administrators" };

        /// <summary>
        /// Authentication schemes used when authorizing the /redirectmanager endpoints.
        /// Leave empty to use the application's default scheme. When running Opti ID in
        /// mixed-mode (AddOptimizelyIdentity(useAsDefault: false)), set this to
        /// OptimizelyIdentityDefaults.SchemeName so the CMS login is used.
        /// </summary>
        public string[] AuthenticationSchemes { get; set; } = Array.Empty<string>();

        internal static RedirectManagerOptions Current { get; set; } = new RedirectManagerOptions();
    }
}
