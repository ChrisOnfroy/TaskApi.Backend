using TaskApi.Application.Features.Tasks.DTOs;
using TaskApi.Domain.Entities;

namespace TaskApi.Application.Features.Tasks.Mappings;

/// <summary>
/// Mapeo manual Entidad -> DTO. Explícito, rápido y sin dependencias (no se usa AutoMapper).
/// </summary>
public static class TaskMappings
{
    public static TaskResponse ToResponse(this TaskItem task) => new(
        task.Id,
        task.Title.Value,
        task.Description,
        task.Priority,
        task.Status,
        task.IsOverdue(DateTime.UtcNow),
        task.DueDate,
        task.CreatedAt,
        task.UpdatedAt,
        task.CompletedAt);
}
