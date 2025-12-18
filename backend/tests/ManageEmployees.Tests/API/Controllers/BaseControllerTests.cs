using System.Security.Claims;
using FluentAssertions;
using ManageEmployees.API.Controllers;
using ManageEmployees.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ManageEmployees.Tests.API.Controllers;

public class BaseControllerTests
{
    [Fact]
    public void CurrentUserProperties_ReadFromClaims()
    {
        var userId = Guid.NewGuid();
        var controller = CreateController(userId, "Admin", "test@test.com");

        controller.GetUserId().Should().Be(userId);
        controller.GetUserRole().Should().Be("Admin");
        controller.GetUserEmail().Should().Be("test@test.com");
    }

    [Fact]
    public void OkResponse_WrapsApiResponse()
    {
        var controller = CreateController(Guid.NewGuid(), "Role", "user@test.com");

        var result = controller.CallOk("data", "message");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("message");
    }

    [Fact]
    public void CreatedResponse_Returns201()
    {
        var controller = CreateController(Guid.NewGuid(), "Role", "user@test.com");

        var result = controller.CallCreated("data", "created");

        var obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(201);
        obj.Value.Should().BeOfType<ApiResponse<string>>();
    }

    [Fact]
    public void NoContentResponse_ReturnsOk()
    {
        var controller = CreateController(Guid.NewGuid(), "Role", "user@test.com");

        var result = controller.CallNoContent("done");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Message.Should().Be("done");
    }

    [Fact]
    public void CurrentUserProperties_ReturnDefaultsWhenMissingClaims()
    {
        var controller = new TestController();
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        controller.GetUserId().Should().Be(Guid.Empty);
        controller.GetUserRole().Should().BeEmpty();
        controller.GetUserEmail().Should().BeEmpty();
    }

    private static TestController CreateController(Guid userId, string role, string email)
    {
        var controller = new TestController();
        var context = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(ClaimTypes.Email, email)
                }))
            }
        };
        controller.ControllerContext = context;
        return controller;
    }

    private class TestController : BaseController
    {
        public Guid GetUserId() => CurrentUserId;
        public string GetUserRole() => CurrentUserRole;
        public string GetUserEmail() => CurrentUserEmail;
        public IActionResult CallOk<T>(T data, string message) => OkResponse(data, message);
        public IActionResult CallCreated<T>(T data, string message) => CreatedResponse(data, message);
        public IActionResult CallNoContent(string message) => NoContentResponse(message);
    }
}
