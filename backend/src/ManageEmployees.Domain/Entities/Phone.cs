namespace ManageEmployees.Domain.Entities;

public class Phone : BaseEntity
{
    public string Number { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Mobile, Home, Work, etc.
    
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}

