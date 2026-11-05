using TaskApi.Domain.Enums;

namespace TaskApi.Application.Features.Tasks.DTOs;

/// <summary>
/// Datos necesarios para crear una tarea.
/// </summary>
public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate);
