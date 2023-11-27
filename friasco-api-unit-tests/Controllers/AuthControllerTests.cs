using friasco_api.Controllers;
using friasco_api.Models;
using friasco_api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace friasco_api_unit_tests.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<ILogger<AuthController>> _loggerMock;
    private Mock<IAuthService> _authServiceMock;
    private Mock<HttpContext> _httpContextMock;
    private Mock<HttpResponse> _httpResponseMock;
    private Mock<HttpRequest> _httpRequestMock;
    private Mock<IResponseCookies> _responseCookiesMock;
    private Mock<IRequestCookieCollection> _responseCookieCollectionMock;
    private AuthController _controller;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<AuthController>>();
        _authServiceMock = new Mock<IAuthService>();
        _httpContextMock = new Mock<HttpContext>();
        _httpResponseMock = new Mock<HttpResponse>();
        _httpRequestMock = new Mock<HttpRequest>();
        _responseCookiesMock = new Mock<IResponseCookies>();
        _responseCookieCollectionMock = new Mock<IRequestCookieCollection>();
        _controller = new AuthController(_loggerMock.Object, _authServiceMock.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = _httpContextMock.Object
            }
        };
    }

    [Test]
    public async Task Login_ReturnsOkResult()
    {
        var model = new AuthLoginRequestModel();
        _authServiceMock.Setup(x => x.Login(model)).ReturnsAsync(new AuthResultModel("JwtString", "RefreshToken"));
        _httpContextMock.Setup(x => x.Response).Returns(_httpResponseMock.Object);
        _httpResponseMock.Setup(x => x.Cookies).Returns(_responseCookiesMock.Object);

        var result = await _controller.Login(model);
        Assert.IsInstanceOf<OkObjectResult>(result);

        _authServiceMock.Verify(x => x.Login(It.IsAny<AuthLoginRequestModel>()), Times.Once);
    }

    [Test]
    public async Task Register_ReturnsOkResult()
    {
        var model = new UserCreateRequestModel();
        _authServiceMock.Setup(x => x.Register(model)).ReturnsAsync(new AuthResultModel("JwtString", "RefreshToken"));
        _httpContextMock.Setup(x => x.Response).Returns(_httpResponseMock.Object);
        _httpResponseMock.Setup(x => x.Cookies).Returns(_responseCookiesMock.Object);

        var result = await _controller.Register(model);
        Assert.IsInstanceOf<OkObjectResult>(result);

        _authServiceMock.Verify(x => x.Register(It.IsAny<UserCreateRequestModel>()), Times.Once);
    }

    [Test]
    public async Task Refresh_ReturnsOkResult()
    {
        var oldRefreshToken = "OldRefreshToken";

        var model = new AuthTokenRequestModel();
        _authServiceMock.Setup(x => x.Refresh(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new AuthResultModel("NewJwtToken", "NewRefreshToken"));
        _httpContextMock.Setup(x => x.Request).Returns(_httpRequestMock.Object);
        _httpContextMock.Setup(x => x.Response).Returns(_httpResponseMock.Object);

        _httpRequestMock.Setup(x => x.Cookies).Returns(_responseCookieCollectionMock.Object);
        _responseCookieCollectionMock.Setup(x => x.TryGetValue(It.IsAny<string>(), out oldRefreshToken)).Returns(true);

        _httpResponseMock.Setup(x => x.Cookies).Returns(_responseCookiesMock.Object);
        _responseCookiesMock.Setup(x => x.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()));

        var result = await _controller.Refresh(model);
        Assert.IsInstanceOf<OkObjectResult>(result);

        _authServiceMock.Verify(x => x.Refresh(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _responseCookieCollectionMock.Verify(x => x.TryGetValue(It.IsAny<string>(), out oldRefreshToken), Times.Once);
        _responseCookiesMock.Verify(x => x.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()), Times.Once);
    }

    [Test]
    public async Task Logout_ReturnsOkResult()
    {
        var oldRefreshToken = "OldRefreshToken";

        var model = new AuthTokenRequestModel();
        _authServiceMock.Setup(x => x.Logout(It.IsAny<string>()));
        _httpContextMock.Setup(x => x.Request).Returns(_httpRequestMock.Object);

        _httpRequestMock.Setup(x => x.Cookies).Returns(_responseCookieCollectionMock.Object);
        _responseCookieCollectionMock.Setup(x => x.TryGetValue(It.IsAny<string>(), out oldRefreshToken)).Returns(true);

        var result = await _controller.Logout();
        Assert.IsInstanceOf<OkResult>(result);

        _authServiceMock.Verify(x => x.Logout(It.IsAny<string>()), Times.Once);
        _responseCookieCollectionMock.Verify(x => x.TryGetValue(It.IsAny<string>(), out oldRefreshToken), Times.Once);
    }

    [Test]
    public async Task LogoutAll_ReturnsOkResult()
    {
        var oldRefreshToken = "OldRefreshToken";

        var model = new AuthTokenRequestModel();
        _authServiceMock.Setup(x => x.LogoutAll(It.IsAny<string>()));
        _httpContextMock.Setup(x => x.Request).Returns(_httpRequestMock.Object);

        _httpRequestMock.Setup(x => x.Cookies).Returns(_responseCookieCollectionMock.Object);
        _responseCookieCollectionMock.Setup(x => x.TryGetValue(It.IsAny<string>(), out oldRefreshToken)).Returns(true);

        var result = await _controller.LogoutAll();
        Assert.IsInstanceOf<OkResult>(result);

        _authServiceMock.Verify(x => x.LogoutAll(It.IsAny<string>()), Times.Once);
        _responseCookieCollectionMock.Verify(x => x.TryGetValue(It.IsAny<string>(), out oldRefreshToken), Times.Once);
    }
}
