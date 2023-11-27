using System.ComponentModel.DataAnnotations;

namespace friasco_api.Models;

public class AuthTokenRequestModel
{
    [Required]
    public string? Token { get; set; }
}
