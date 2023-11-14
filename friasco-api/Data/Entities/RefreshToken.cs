namespace friasco_api.Data.Entities;

public class RefreshToken
{
    public int? Id { get; set; }
    public Guid? UserGuid { get; set; }
    public string? JwtId { get; set; }
    public string? Token { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsUsed { get; set; }
    public bool IsValid { get; set; }
}
