# Changelog

All notable changes to this project will be documented in this file.

## \[7.1.0] - 2026-08

* Feature: Opti ID support — new `AddRedirectManager(Action<RedirectManagerOptions>)` overload with configurable `AllowedRoles` and `AuthenticationSchemes` (for mixed-mode `AddOptimizelyIdentity(useAsDefault: false)` setups)
* Change: `RedirectManagerController` now authorizes via the `episerver:redirectmanager` policy instead of hardcoded `[Authorize(Roles = ...)]`
* Change: Menu item and Quick Navigator visibility now honor the configured `AllowedRoles` (default: RedirectManagers, CmsAdmins, WebAdmins, Administrators) — CmsAdmins-only users (Opti ID admins) now see the menu
* Note: Under Opti ID, create a custom role `RedirectManagers` in the Opti ID Admin Center to grant access to non-admins

## \[7.0.0] - 2026-08

* Feature: Target Optimizely CMS 13 / .NET 10
* Feature: Updated admin UI experience for CMS 13 compatibility
* Feature: Added support for the current Optimizely shell resources and navigation pattern
* Note: If you are running Optimizely CMS 12, use package version 6.x instead

## \[6.4.0] - 2026-01

* Feature: Export redirect rules to Excel (.xlsx) format
* Feature: Option to convert Content IDs to URLs during export
* Feature: Import redirect rules from Excel (.xlsx) files
* Feature: Import with Update mode (match and update existing rules)
* Feature: Import with Replace All mode (delete all then import)
* Feature: Detailed error reporting for import operations
* Added EPPlus dependency for Excel file handling
* Support for .net8

## \[6.3.0]

* Feature: Introducing lang param for complex language setups on AddRedirectManager

## \[6.2.0]

* Bugfix: Segment not changed when not master language

## \[6.1.1]

* Upgrade UI to latest CMS branding

## \[6.0.0]

* Support for .net6
* Clean up rules functionality
