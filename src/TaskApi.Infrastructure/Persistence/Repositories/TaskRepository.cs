using Microsoft.EntityFrameworkCore;
using TaskApi.Application.Interfaces;
using TaskApi.Domain.Entities;
using TaskApi.Domain.Enums;

namespace TaskApi.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación de ITaskRepository con EF Core.
/// Solo traduce operaciones a consultas; no contiene reglas de negocio
/// ni llama a SaveChanges (eso lo decide el caso de uso vía IUnitOfWork).
/// </summary>
public sealed class TaskRepository(ApplicationDbContext context) : ITaskRepository
{
    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetPagedAsync(
        TaskItemStatus? status,
        TaskPriority? priority,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Solo lectura: sin tracking para mejor rendimiento.
        var query = context.Tasks.AsNoTracking();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .ThenBy(t => t.Id) // Orden determinista para una paginación estable.
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default) =>
        await context.Tasks.AddAsync(task, cancellationToken);

    public void Remove(TaskItem task) => context.Tasks.Remove(task);
}
