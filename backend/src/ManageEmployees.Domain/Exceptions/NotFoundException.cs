namespace ManageEmployees.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string entity, object key) 
        : base($"{entity} com identificador '{key}' não foi encontrado.") { }
}

