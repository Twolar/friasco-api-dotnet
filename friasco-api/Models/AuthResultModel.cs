namespace friasco_api;

public class AuthResultModel
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }

    public AuthResultModel(string token, string refreshToken)
    {
        Token = token;
        RefreshToken = refreshToken;
    }
}
