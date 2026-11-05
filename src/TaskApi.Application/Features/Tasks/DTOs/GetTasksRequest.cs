using TaskApi.Domain.Enums;

namespace TaskApi.Application.Features.Tasks.DTOs;

/// <summary>
/// Filtros y paginación para listar tareas.
/// Usa propiedades init con valores por defecto para poder enlazarse desde la query string.
/// </summary>
public sealed record GetTasksRequest
{
    public const int MaxPageSize = 100;

    public TaskItemStatus? Status { get; init; }
    public TaskPriority? Priority { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
