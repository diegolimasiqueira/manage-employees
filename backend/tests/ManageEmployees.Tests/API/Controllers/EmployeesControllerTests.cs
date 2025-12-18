using System.Security.Claims;
using FluentAssertions;
using ManageEmployees.API.Controllers;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ManageEmployees.Tests.API.Controllers;

public class EmployeesControllerTests
{
    private readonly Mock<IEmployeeService> _serviceMock = new();
    private readonly EmployeesController _controller;

    public EmployeesControllerTests()
    {
        _controller = new EmployeesController(_serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, Guid.Empty.ToString())
                    }))
                }
            }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkResponse()
    {
        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmployeeDto>());

        var result = await _controller.GetAll(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOkResponse()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeDto());

        var result = await _controller.GetById(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeDto?)null);

        var result = await _controller.GetById(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreatedResponse()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CreateEmployeeRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeDto());

        var result = await _controller.Create(new CreateEmployeeRequest(), CancellationToken.None);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
        _serviceMock.Verify(s => s.CreateAsync(It.IsAny<CreateEmployeeRequest>(), Guid.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsOkResponse()
    {
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateEmployeeRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeDto());

        var result = await _controller.Update(Guid.NewGuid(), new UpdateEmployeeRequest(), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResponse()
    {
        var result = await _controller.Delete(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), Guid.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPendingApprovals_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetPendingApprovalsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PendingApprovalDto>());

        var result = await _controller.GetPendingApprovals(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ApproveEmployee_ReturnsMessageBasedOnDecision()
    {
        _serviceMock.Setup(s => s.ApproveEmployeeAsync(It.IsAny<ApproveEmployeeRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeDto());

        var approveResult = await _controller.ApproveEmployee(new ApproveEmployeeRequest { Approve = true }, CancellationToken.None);
        var rejectResult = await _controller.ApproveEmployee(new ApproveEmployeeRequest { Approve = false }, CancellationToken.None);

        approveResult.Should().BeOfType<OkObjectResult>();
        rejectResult.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSubordinates_ReturnsOkResponse()
    {
        _serviceMock.Setup(s => s.GetSubordinatesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmployeeSimpleDto>());

        var result = await _controller.GetSubordinates(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_ReturnsOk()
    {
        var result = await _controller.ChangePassword(new ChangePasswordRequest(), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.ChangePasswordAsync(Guid.Empty, It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAvailableManagers_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAvailableManagersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ManagerOptionDto>());

        var result = await _controller.GetAvailableManagers(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_ReturnsTemporaryPassword()
    {
        _serviceMock.Setup(s => s.ResetPasswordAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("temp");

        var result = await _controller.ResetPassword(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.ResetPasswordAsync(It.IsAny<Guid>(), Guid.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_ReturnsOk()
    {
        _serviceMock.Setup(s => s.UpdateProfileAsync(It.IsAny<Guid>(), It.IsAny<UpdateProfileRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeDto());

        var result = await _controller.UpdateProfile(new UpdateProfileRequest(), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetProfile_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeDto?)null);

        var result = await _controller.GetProfile(CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetProfile_WhenFound_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeDto());

        var result = await _controller.GetProfile(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UploadPhoto_WhenNoFileOrInvalid_ReturnsBadRequest()
    {
        var resultNull = await _controller.UploadPhoto(Guid.NewGuid(), null!, CancellationToken.None);
        resultNull.Should().BeOfType<BadRequestObjectResult>();

        var invalidExtension = new FormFile(new MemoryStream(new byte[1]), 0, 1, "file", "photo.exe");
        var resultInvalid = await _controller.UploadPhoto(Guid.NewGuid(), invalidExtension, CancellationToken.None);
        resultInvalid.Should().BeOfType<BadRequestObjectResult>();

        var largeFile = new FormFile(new MemoryStream(new byte[5 * 1024 * 1024 + 1]), 0, 5 * 1024 * 1024 + 1, "file", "photo.png");
        var resultLarge = await _controller.UploadPhoto(Guid.NewGuid(), largeFile, CancellationToken.None);
        resultLarge.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadPhoto_WhenValid_ReturnsOk()
    {
        _serviceMock.Setup(s => s.UploadPhotoAsync(It.IsAny<Guid>(), It.IsAny<IFormFile>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/photo.png");
        var file = new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "file", "photo.png");

        var result = await _controller.UploadPhoto(Guid.NewGuid(), file, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.UploadPhotoAsync(It.IsAny<Guid>(), file, Guid.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePhoto_ReturnsOk()
    {
        var result = await _controller.DeletePhoto(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.DeletePhotoAsync(It.IsAny<Guid>(), Guid.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }
}
