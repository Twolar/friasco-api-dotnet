using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using friasco_api.Controllers;
using friasco_api.Models;
using friasco_api.Services;

namespace friasco_api_unit_tests.Controllers;

[TestFixture]
public class UsersControllerTests
{
    private Mock<ILogger<UsersController>> _loggerMock;
    private Mock<IUserService> _userServiceMock;
    private UsersController _controller;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<UsersController>>();
        _userServiceMock = new Mock<IUserService>();
        _controller = new UsersController(_loggerMock.Object, _userServiceMock.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOkResult()
    {
        var result = await _controller.GetAll();
        Assert.IsInstanceOf<OkResult>(result);
    }

    [Test]
    public async Task GetById_ReturnsOkResult()
    {
        var result = await _controller.GetById(1);
        Assert.IsInstanceOf<OkResult>(result);
    }

    [Test]
    public async Task Create_ReturnsOkResult()
    {
        var model = new UserCreateRequestModel();
        var result = await _controller.Create(model);
        Assert.IsInstanceOf<OkResult>(result);
    }

    public async Task Update_ReturnsOkResult()
    {
        var model = new UserUpdateRequestModel();
        var result = await _controller.Update(1, model);
        Assert.IsInstanceOf<OkResult>(result);
    }

    public async Task Delete_ReturnsOkResult()
    {
        var result = await _controller.Delete(1);
        Assert.IsInstanceOf<OkResult>(result);
    }
}
