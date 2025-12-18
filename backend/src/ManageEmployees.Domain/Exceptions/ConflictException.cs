namespace ManageEmployees.Domain.Exceptions;

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
    
    public ConflictException(string entity, string field, object value) 
        : base($"{entity} com {field} '{value}' já existe.") { }
}

