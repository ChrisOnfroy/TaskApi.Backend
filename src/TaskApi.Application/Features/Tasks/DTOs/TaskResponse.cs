using TaskApi.Domain.Enums;

namespace TaskApi.Application.Features.Tasks.DTOs;

/// <summary>
/// Representación de una tarea que se devuelve al cliente.
/// Aplana el Value Object TaskTitle a string y expone datos calculados (IsOverdue).
/// </summary>
public sealed record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    TaskPriority Priority,
    TaskItemStatus Status,
    bool IsOverdue,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? CompletedAt);
