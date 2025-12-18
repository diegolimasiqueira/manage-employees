namespace ManageEmployees.Domain.Entities;

/// <summary>
/// Representa um cargo/função na hierarquia da empresa
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Nome do cargo (ex: Diretor, Líder, Funcionário)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição do cargo e suas responsabilidades
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Nível hierárquico (quanto maior, mais permissões)
    /// Ex: Diretor = 100, Líder = 50, Funcionário = 10
    /// </summary>
    public int HierarchyLevel { get; set; }
    
    /// <summary>
    /// Indica se este cargo pode aprovar cadastros de novos funcionários
    /// </summary>
    public bool CanApproveRegistrations { get; set; }
    
    /// <summary>
    /// Indica se este cargo pode criar outros funcionários
    /// </summary>
    public bool CanCreateEmployees { get; set; }
    
    /// <summary>
    /// Indica se este cargo pode editar outros funcionários
    /// </summary>
    public bool CanEditEmployees { get; set; }
    
    /// <summary>
    /// Indica se este cargo pode excluir outros funcionários
    /// </summary>
    public bool CanDeleteEmployees { get; set; }
    
    /// <summary>
    /// Indica se este cargo pode gerenciar cargos
    /// </summary>
    public bool CanManageRoles { get; set; }
    
    /// <summary>
    /// Funcionários com este cargo
    /// </summary>
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    
    /// <summary>
    /// Verifica se este cargo pode criar um funcionário com o cargo especificado
    /// </summary>
    public bool CanCreateEmployeeWithRole(Role targetRole)
    {
        // Só pode criar funcionários com nível hierárquico menor
        return CanCreateEmployees && HierarchyLevel > targetRole.HierarchyLevel;
    }
    
    /// <summary>
    /// Verifica se este cargo pode aprovar um funcionário com o cargo especificado
    /// </summary>
    public bool CanApproveEmployeeWithRole(Role targetRole)
    {
        // Só pode aprovar funcionários com nível hierárquico menor ou igual
        return CanApproveRegistrations && HierarchyLevel >= targetRole.HierarchyLevel;
    }
}



