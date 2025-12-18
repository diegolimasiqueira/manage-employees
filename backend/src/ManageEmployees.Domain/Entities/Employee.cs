namespace ManageEmployees.Domain.Entities;

/// <summary>
/// Representa um funcionário da empresa
/// </summary>
public class Employee : BaseEntity
{
    /// <summary>
    /// Nome completo do funcionário
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// E-mail do funcionário (usado para login)
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Número do documento (CPF/RG)
    /// </summary>
    public string DocumentNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Hash da senha para autenticação
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Data de nascimento
    /// </summary>
    public DateTime BirthDate { get; set; }
    
    /// <summary>
    /// ID do cargo do funcionário
    /// </summary>
    public Guid RoleId { get; set; }
    
    /// <summary>
    /// Cargo do funcionário
    /// </summary>
    public Role Role { get; set; } = null!;
    
    /// <summary>
    /// ID do gerente/superior (null para diretores)
    /// </summary>
    public Guid? ManagerId { get; set; }
    
    /// <summary>
    /// Gerente/Superior do funcionário
    /// </summary>
    public Employee? Manager { get; set; }
    
    /// <summary>
    /// Funcionários subordinados a este
    /// </summary>
    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    
    /// <summary>
    /// Telefones do funcionário
    /// </summary>
    public ICollection<Phone> Phones { get; set; } = new List<Phone>();
    
    /// <summary>
    /// Indica se o cadastro do funcionário foi aprovado
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Data em que o cadastro foi aprovado
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    
    /// <summary>
    /// ID do funcionário que aprovou o cadastro
    /// </summary>
    public Guid? ApprovedById { get; set; }
    
    /// <summary>
    /// Funcionário que aprovou o cadastro
    /// </summary>
    public Employee? ApprovedBy { get; set; }
    
    /// <summary>
    /// Caminho da foto de perfil do funcionário
    /// </summary>
    public string? PhotoPath { get; set; }
    
    /// <summary>
    /// Calcula a idade do funcionário
    /// </summary>
    public int GetAge()
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - BirthDate.Year;
        if (BirthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
    
    /// <summary>
    /// Verifica se este funcionário pode criar outro com o cargo especificado
    /// </summary>
    public bool CanCreateEmployeeWithRole(Role targetRole)
    {
        return Role.CanCreateEmployeeWithRole(targetRole);
    }
    
    /// <summary>
    /// Verifica se este funcionário pode aprovar outro com o cargo especificado
    /// </summary>
    public bool CanApproveEmployee(Employee targetEmployee)
    {
        return Role.CanApproveEmployeeWithRole(targetEmployee.Role);
    }
}
