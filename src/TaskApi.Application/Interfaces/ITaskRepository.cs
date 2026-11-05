using TaskApi.Domain.Entities;
using TaskApi.Domain.Enums;

namespace TaskApi.Application.Interfaces;

/// <summary>
/// Abstracción de persistencia para el agregado TaskItem.
/// La implementación concreta (EF Core + PostgreSQL) vive en Infrastructure.
/// </summary>
public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetPagedAsync(
        TaskItemStatus? status,
        TaskPriority? priority,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    void Remove(TaskItem task);
}
