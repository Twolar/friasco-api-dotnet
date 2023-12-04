using System.ComponentModel.DataAnnotations;

namespace friasco_api.Models;

public class AuthForgottenPasswordModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
}
