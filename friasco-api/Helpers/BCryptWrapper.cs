namespace friasco_api.Helpers;

public interface IBCryptWrapper
{
    string HashPassword(string password);
    bool Verify(string password, string hashedPassword);
}

public class BCryptWrapper : IBCryptWrapper
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
