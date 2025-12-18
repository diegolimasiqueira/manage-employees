using FluentAssertions;
using ManageEmployees.Domain.Exceptions;
using Xunit;

namespace ManageEmployees.Tests.Domain;

public class ExceptionTests
{
    [Fact]
    public void NotFoundException_FormatsMessage()
    {
        var ex = new NotFoundException("Employee", 123);
        ex.Message.Should().Contain("Employee com identificador '123'");
    }

    [Fact]
    public void UnauthorizedException_DefaultMessage()
    {
        var ex = new UnauthorizedException();
        ex.Message.Should().Be("Não autorizado.");
    }

    [Fact]
    public void ForbiddenException_DefaultMessage()
    {
        var ex = new ForbiddenException();
        ex.Message.Should().Be("Acesso negado.");
    }

    [Fact]
    public void ConflictException_CustomMessageAndFormattedMessage()
    {
        var direct = new ConflictException("conflict");
        direct.Message.Should().Be("conflict");

        var formatted = new ConflictException("Funcionário", "e-mail", "test@example.com");
        formatted.Message.Should().Be("Funcionário com e-mail 'test@example.com' já existe.");
    }

    [Fact]
    public void ValidationException_SetsErrors()
    {
        var errors = new Dictionary<string, string[]> { { "Name", new[] { "Required" } } };
        var ex = new ValidationException(errors);

        ex.Message.Should().Be("Ocorreram erros de validação.");
        ex.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public void ValidationException_SingleFieldOverload()
    {
        var ex = new ValidationException("Field", "Error message");

        ex.Errors.Should().ContainKey("Field");
        ex.Message.Should().Be("Error message");
    }
}
