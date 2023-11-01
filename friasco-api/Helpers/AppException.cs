namespace friasco_api.Helpers;

public class AppException : Exception
{
    public AppException(string message) : base(message) { }
}
