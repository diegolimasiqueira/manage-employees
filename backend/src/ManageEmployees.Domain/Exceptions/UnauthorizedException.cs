namespace ManageEmployees.Domain.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Não autorizado.") : base(message) { }
}

