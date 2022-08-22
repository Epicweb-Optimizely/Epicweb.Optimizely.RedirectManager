using Epicweb.Optimizely.RedirectManager.Alloy.Models.ViewModels;

namespace Epicweb.Optimizely.RedirectManager.Alloy.Business;

/// <summary>
/// Defines a method which may be invoked by PageContextActionFilter allowing controllers
/// to modify common layout properties of the view model.
/// </summary>
internal interface IModifyLayout
{
    void ModifyLayout(LayoutModel layoutModel);
}
