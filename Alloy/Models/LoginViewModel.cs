using System.ComponentModel.DataAnnotations;

namespace Epicweb.Optimizely.RedirectManager.Alloy.Models;

public class LoginViewModel
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}
