using FluentAssertions;
using ManageEmployees.Application.DTOs;
using Xunit;

namespace ManageEmployees.Tests.Application.DTOs;

public class ApiResponseTests
{
    [Fact]
    public void Ok_WithData_ReturnsSuccessResponse()
    {
        var response = ApiResponse<string>.Ok("data", "message");

        response.Success.Should().BeTrue();
        response.Message.Should().Be("message");
        response.Data.Should().Be("data");
    }

    [Fact]
    public void Fail_WithErrors_ReturnsFailureResponse()
    {
        var errors = new Dictionary<string, string[]> { { "Field", new[] { "Error" } } };

        var response = ApiResponse<string>.Fail("failed", errors);

        response.Success.Should().BeFalse();
        response.Message.Should().Be("failed");
        response.Errors.Should().BeSameAs(errors);
    }

    [Fact]
    public void NonGenericFail_UsesHiddenMethod()
    {
        var response = ApiResponse.Fail("invalid");

        response.Success.Should().BeFalse();
        response.Message.Should().Be("invalid");
    }
}
