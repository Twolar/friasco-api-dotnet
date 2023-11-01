using System.ComponentModel.DataAnnotations;

namespace friasco_api.Models;

public class AuthLoginRequestModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Password { get; set; }
}
