using TaskApi.Domain.Enums;

namespace TaskApi.Application.Features.Tasks.DTOs;

/// <summary>
/// Datos para actualizar los detalles de una tarea existente.
/// El estado no se cambia aquí: tiene sus propios casos de uso (Start, Complete, Reopen).
/// </summary>
public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate);
