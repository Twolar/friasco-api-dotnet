namespace friasco_api.Models;

public class AuthResultModel
{
    public string Token { get; set; }
    public string TokenId { get; set; }
    public string RefreshToken { get; set; }

    public AuthResultModel(string token, string tokenId, string refreshToken)
    {
        Token = token;
        TokenId = tokenId;
        RefreshToken = refreshToken;
    }
}
