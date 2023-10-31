namespace friasco_api.Helpers;

public class CustomAppException : Exception
{
    public CustomAppException(string message) : base(message) { }
}
