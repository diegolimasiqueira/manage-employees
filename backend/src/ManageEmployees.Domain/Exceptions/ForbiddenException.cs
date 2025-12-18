namespace ManageEmployees.Domain.Exceptions;

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "Acesso negado.") : base(message) { }
}

