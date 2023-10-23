using friasco_api.Models;

namespace friasco_api_unit_tests.Models;

public class UserUpdateRequestModelTests
{
    private UserUpdateRequestModel _UserUpdateRequestModel;

    [SetUp]
    public void Setup()
    {
        _UserUpdateRequestModel = new UserUpdateRequestModel();
    }

    [Test]
    public void PasswordSetter_SetsPasswordToNull_WhenValueIsEmpty()
    {
        string value = "";

        _UserUpdateRequestModel.Password = value;

        Assert.That(_UserUpdateRequestModel.Password, Is.Null);
    }

    [Test]
    public void PasswordSetter_SetsPasswordToValue_WhenValueIsNotEmpty()
    {
        string value = "password123";

        _UserUpdateRequestModel.Password = value;

        Assert.That(_UserUpdateRequestModel.Password, Is.EqualTo(value));
    }

    [Test]
    public void ConfirmPasswordSetter_SetsConfirmPasswordToNull_WhenValueIsEmpty()
    {
        string value = "";

        _UserUpdateRequestModel.ConfirmPassword = value;

        Assert.That(_UserUpdateRequestModel.ConfirmPassword, Is.Null);
    }

    [Test]
    public void ConfirmPasswordSetter_SetsConfirmPasswordToValue_WhenValueIsNotEmpty()
    {
        string value = "password123";

        _UserUpdateRequestModel.ConfirmPassword = value;

        Assert.That(_UserUpdateRequestModel.ConfirmPassword, Is.EqualTo(value));
    }

}