using ManageEmployees.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManageEmployees.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.DocumentNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.BirthDate)
            .IsRequired();

        builder.Property(e => e.Enabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.ApprovedAt);

        // Índices únicos
        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.HasIndex(e => e.DocumentNumber)
            .IsUnique();

        // Índice para busca de pendentes de aprovação
        builder.HasIndex(e => e.Enabled);

        // Relacionamento com Role
        builder.HasOne(e => e.Role)
            .WithMany(r => r.Employees)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento hierárquico (gerente/subordinados)
        builder.HasOne(e => e.Manager)
            .WithMany(e => e.Subordinates)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Relacionamento com aprovador
        builder.HasOne(e => e.ApprovedBy)
            .WithMany()
            .HasForeignKey(e => e.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Relacionamento com telefones
        builder.HasMany(e => e.Phones)
            .WithOne(p => p.Employee)
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
