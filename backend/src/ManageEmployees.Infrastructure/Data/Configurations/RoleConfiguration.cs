using ManageEmployees.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManageEmployees.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.HierarchyLevel)
            .IsRequired();

        builder.Property(r => r.CanApproveRegistrations)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.CanCreateEmployees)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.CanEditEmployees)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.CanDeleteEmployees)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.CanManageRoles)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.HasIndex(r => r.HierarchyLevel);

        // Sem dados iniciais - cargos serão criados pelo DbSeeder
    }
}



