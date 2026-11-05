using Microsoft.EntityFrameworkCore;
using TaskApi.Application.Interfaces;
using TaskApi.Domain.Entities;

namespace TaskApi.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core para PostgreSQL.
/// Implementa IUnitOfWork: DbContext ya es una unidad de trabajo, así que no hace falta
/// una clase adicional que solo reenvíe la llamada a SaveChangesAsync.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica automáticamente todas las clases IEntityTypeConfiguration<T> de este ensamblado.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
