using System.ComponentModel.DataAnnotations;

namespace friasco_api.Models;

public class AuthChangePasswordRequestModel
{
    [Required]
    public string? Password { get; set; }

    [Required]
    public string? NewPassword { get; set; }

    [Required]
    public string? ConfirmNewPassword { get; set; }
}
