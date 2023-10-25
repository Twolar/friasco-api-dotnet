using AutoMapper;
using friasco_api.Data.Entities;
using friasco_api.Models;
using friasco_api.Helpers;
using friasco_api.Enums;

namespace friasco_api_unit_tests.Helpers;

[TestFixture]
public class MapperProfileTests
{
    private IMapper _mapper;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>());
        _mapper = config.CreateMapper();
    }

    [Test]
    public void UserCreateRequestModel_Matches_UserModel()
    {
        // Setup configuration for mapping and ignore certain properties...
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<UserCreateRequestModel, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()));

        // Assert convention of properties in Source matching Destination.
        configuration.AssertConfigurationIsValid();
    }

    [Test]
    public void UserUpdateRequestModel_Matches_UserModel()
    {
        // Setup configuration for mapping and ignore certain properties...
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<UserUpdateRequestModel, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()));

        // Assert convention of properties in Source matching Destination.
        configuration.AssertConfigurationIsValid();
    }

    [Test]
    public void UserCreateRequestModel_ToUser_Mapping()
    {
        var model = new UserCreateRequestModel
        {
            Username = "User1",
            Email = "user1@example.com",
            FirstName = "user1First",
            LastName = "user1Last",
            Role = UserRoleEnum.User,
            Password = "password123",
            ConfirmPassword = "password123",
        };
        var user = _mapper.Map<User>(model);

        Assert.That(user.Username, Is.EqualTo(model.Username));
        Assert.That(user.Email, Is.EqualTo(model.Email));
        Assert.That(user.FirstName, Is.EqualTo(model.FirstName));
        Assert.That(user.LastName, Is.EqualTo(model.LastName));
        Assert.That(user.Role, Is.EqualTo(model.Role));
        Assert.That(user.PasswordHash, Is.EqualTo(null));
    }

    [Test]
    public void UserUpdateRequestModel_ToUser_Mapping()
    {
        var model = new UserUpdateRequestModel
        {
            Username = "User1",
            Email = "user1@example.com",
            FirstName = "user1First",
            LastName = "user1Last",
            Role = UserRoleEnum.User,
            Password = "password123",
            ConfirmPassword = "password123",
        };
        var user = _mapper.Map<User>(model);

        Assert.That(user.Username, Is.EqualTo(model.Username));
        Assert.That(user.Email, Is.EqualTo(model.Email));
        Assert.That(user.FirstName, Is.EqualTo(model.FirstName));
        Assert.That(user.LastName, Is.EqualTo(model.LastName));
        Assert.That(user.Role, Is.EqualTo(model.Role));
        Assert.That(user.PasswordHash, Is.EqualTo(null));
    }

    [Test]
    public void UserUpdateRequestModel_ToUser_Mapping_IgnoresNullsAndEmptyStrings()
    {
        var userUsername = "user1";
        var userEmail = "user1@example.com";

        var userUpdateModel = new UserUpdateRequestModel() { Username = null, Email = "" };
        var user = new User { Username = userUsername, Email = userEmail };

        _mapper.Map(userUpdateModel, user);

        Assert.That(user.Username, Is.EqualTo(userUsername));
        Assert.That(user.Email, Is.EqualTo(userEmail));
    }

    [Test]
    public void UserUpdateRequestModel_ToUser_Mapping_ReplacesValues()
    {
        var model = new UserUpdateRequestModel
        {
            Username = "User1Changed",
            Email = "User1Changed@example.com",
            FirstName = "user1ChangedFirst",
            LastName = "user1ChangedLast",
            Role = UserRoleEnum.Admin,
            Password = "passwordChanged123",
            ConfirmPassword = "passwordChanged123",
        };
        var user = new User
        {
            Username = "User1",
            Email = "user1@example.com",
            FirstName = "user1First",
            LastName = "user1Last",
            Role = UserRoleEnum.User,
        };

        _mapper.Map(model, user);

        Assert.That(user.Username, Is.EqualTo(model.Username));
        Assert.That(user.Email, Is.EqualTo(model.Email));
        Assert.That(user.FirstName, Is.EqualTo(model.FirstName));
        Assert.That(user.LastName, Is.EqualTo(model.LastName));
        Assert.That(user.Role, Is.EqualTo(model.Role));
        Assert.That(user.PasswordHash, Is.EqualTo(null));
    }
}
