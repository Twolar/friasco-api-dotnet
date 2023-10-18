using System.Text.Json.Serialization;
using friasco_api.Enums;

namespace friasco_api.Entities;

public class User
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public UserRoleEnum Role { get; set; }

    [JsonIgnore]
    public string? PasswordHash { get; set; }
}
