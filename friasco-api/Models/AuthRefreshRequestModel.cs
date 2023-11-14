using System.ComponentModel.DataAnnotations;

namespace friasco_api.Models;

public class AuthRefreshRequestModel
{
    [Required]
    public string? Token { get; set; }

    [Required]
    public string? RefreshToken { get; set; }
}
