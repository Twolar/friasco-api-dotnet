using friasco_api.Models;

namespace friasco_api_unit_tests.Models;

public class UserUpdateRequestTests
{
    private UserUpdateRequest _userUpdateRequest;

    [SetUp]
    public void Setup()
    {
        _userUpdateRequest = new UserUpdateRequest();
    }

    [Test]
    public void PasswordSetter_SetsPasswordToNull_WhenValueIsEmpty()
    {
        string value = "";

        _userUpdateRequest.Password = value;

        Assert.That(_userUpdateRequest.Password, Is.Null);
    }

    [Test]
    public void PasswordSetter_SetsPasswordToValue_WhenValueIsNotEmpty()
    {
        string value = "password123";

        _userUpdateRequest.Password = value;

        Assert.That(_userUpdateRequest.Password, Is.EqualTo(value));
    }

    [Test]
    public void ConfirmPasswordSetter_SetsConfirmPasswordToNull_WhenValueIsEmpty()
    {
        string value = "";

        _userUpdateRequest.ConfirmPassword = value;

        Assert.That(_userUpdateRequest.ConfirmPassword, Is.Null);
    }

    [Test]
    public void ConfirmPasswordSetter_SetsConfirmPasswordToValue_WhenValueIsNotEmpty()
    {
        string value = "password123";

        _userUpdateRequest.ConfirmPassword = value;

        Assert.That(_userUpdateRequest.ConfirmPassword, Is.EqualTo(value));
    }

}