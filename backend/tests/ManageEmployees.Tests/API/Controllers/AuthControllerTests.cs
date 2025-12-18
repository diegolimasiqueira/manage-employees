using FluentAssertions;
using ManageEmployees.API.Controllers;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ManageEmployees.Tests.API.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _serviceMock = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_serviceMock.Object);
    }

    [Fact]
    public async Task Login_ReturnsOkResponse()
    {
        _serviceMock.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenResponse());

        var result = await _controller.Login(new LoginRequest(), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeAssignableTo<ApiResponse<TokenResponse>>();
    }

    [Fact]
    public async Task RegisterFirstDirector_ReturnsCreatedResponse()
    {
        _serviceMock.Setup(s => s.RegisterFirstDirectorAsync(It.IsAny<RegisterFirstDirectorRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenResponse());

        var result = await _controller.RegisterFirstDirector(new RegisterFirstDirectorRequest(), CancellationToken.None);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task SelfRegister_ReturnsCreatedResponse()
    {
        _serviceMock.Setup(s => s.SelfRegisterAsync(It.IsAny<SelfRegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeDto());

        var result = await _controller.SelfRegister(new SelfRegisterRequest(), CancellationToken.None);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task HasDirector_ReturnsOk()
    {
        _serviceMock.Setup(s => s.HasDirectorAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _controller.HasDirector(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetCurrentUser_UsesClaimFromContext()
    {
        var userInfo = new UserInfoResponse
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            Role = new RoleSimpleDto()
        };
        _serviceMock.Setup(s => s.GetCurrentUserAsync(userInfo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userInfo);

        var controller = new AuthController(_serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userInfo.Id.ToString())
            }));

        var result = await controller.GetCurrentUser(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }
}
