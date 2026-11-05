using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskApi.Domain.Entities;
using TaskApi.Domain.Enums;
using TaskApi.Domain.ValueObjects;

namespace TaskApi.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeo Fluent API de TaskItem. Mantiene al dominio libre de atributos de EF Core.
/// </summary>
public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    private const int EnumMaxLength = 20;

    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        // CHECK constraints: PostgreSQL solo acepta valores válidos de los enums (defensa en profundidad).
        builder.ToTable("tasks", table =>
        {
            table.HasCheckConstraint("ck_tasks_priority", BuildInConstraint<TaskPriority>("priority"));
            table.HasCheckConstraint("ck_tasks_status", BuildInConstraint<TaskItemStatus>("status"));
        });

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever(); // El Id lo genera el dominio.

        // Value Object -> columna varchar(150). Al leer, se reconstruye pasando por la validación.
        builder.Property(t => t.Title)
            .HasConversion(title => title.Value, value => TaskTitle.Create(value))
            .HasMaxLength(TaskTitle.MaxLength)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(TaskItem.DescriptionMaxLength);

        // Enums como texto: legibles en PostgreSQL y estables si se reordenan los miembros.
        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(EnumMaxLength)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(EnumMaxLength)
            .IsRequired();

        builder.Property(t => t.CreatedAt).IsRequired();

        // Propiedad calculada en memoria: no se persiste.
        builder.Ignore(t => t.IsCompleted);

        // Índices para los filtros y el ordenamiento usados en GetPagedAsync.
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.CreatedAt);
    }

    private static string BuildInConstraint<TEnum>(string column) where TEnum : struct, Enum =>
        $"{column} IN ({string.Join(", ", Enum.GetNames<TEnum>().Select(name => $"'{name}'"))})";
}
