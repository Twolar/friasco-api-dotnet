namespace friasco_api.Helpers;

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string message) : base(message) { }
}
