namespace ManageEmployees.Domain.Exceptions;

public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }
    
    public ValidationException(IDictionary<string, string[]> errors) 
        : base("Ocorreram erros de validação.")
    {
        Errors = errors;
    }
    
    public ValidationException(string field, string error) 
        : base(error)
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }
}

