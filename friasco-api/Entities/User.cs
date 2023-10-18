namespace friasco_api.Entities;

public class User
{
    // TODO: Password handling, hashing etc...
    private Guid _id;
    private string _email;
    private string _username;

    public User(Guid id, string username, string email)
    {
        _id = id;
        _username = username;
        _email = email;
    }
}
