using friasco_api;
using friasco_api.Controllers;
using friasco_api.Models;
using friasco_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace friasco_api_unit_tests.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<ILogger<AuthController>> _loggerMock;
    private Mock<IAuthService> _authServiceMock;
    private AuthController _controller;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<AuthController>>();
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_loggerMock.Object, _authServiceMock.Object);
    }

    [Test]
    public async Task Login_ReturnsOkResult()
    {
        var model = new AuthLoginRequestModel();
        _authServiceMock.Setup(x => x.Login(model)).ReturnsAsync(new AuthResponseModel { Token = "JwtString", RefreshToken = "RefreshToken" });
        var result = await _controller.Login(model);
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task Register_ReturnsOkResult()
    {
        var model = new UserCreateRequestModel();
        _authServiceMock.Setup(x => x.Register(model)).ReturnsAsync(new AuthResponseModel { Token = "JwtString", RefreshToken = "RefreshToken" });
        var result = await _controller.Register(model);
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    // TODO: Add refresh token tests
}
