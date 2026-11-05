using TaskApi.Domain.Common;
using TaskApi.Domain.Enums;
using TaskApi.Domain.Exceptions;
using TaskApi.Domain.ValueObjects;

namespace TaskApi.Domain.Entities;

/// <summary>
/// Representa una tarea del usuario. Es la raíz de agregado del dominio.
/// Todo cambio de estado pasa por métodos que validan las reglas del negocio.
/// </summary>
public sealed class TaskItem : BaseEntity
{
    public const int DescriptionMaxLength = 1000;

    public TaskTitle Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public TaskPriority Priority { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public bool IsCompleted => Status == TaskItemStatus.Completed;

    // Requerido por el ORM para materializar la entidad. No usar desde código de negocio.
    private TaskItem()
    {
    }

    public TaskItem(TaskTitle title, string? description, TaskPriority priority, DateTime? dueDate)
    {
        Title = title ?? throw new DomainException("El título es obligatorio.");
        Description = ValidateDescription(description);
        Priority = ValidatePriority(priority);
        DueDate = ValidateDueDate(dueDate);
        Status = TaskItemStatus.Pending;
    }

    public void UpdateDetails(TaskTitle title, string? description, TaskPriority priority, DateTime? dueDate)
    {
        EnsureNotCompleted();

        Title = title ?? throw new DomainException("El título es obligatorio.");
        Description = ValidateDescription(description);
        Priority = ValidatePriority(priority);
        DueDate = ValidateDueDate(dueDate);
        MarkAsUpdated();
    }

    public void Start()
    {
        EnsureNotCompleted();
        if (Status == TaskItemStatus.InProgress)
            throw new DomainException("La tarea ya está en progreso.");

        Status = TaskItemStatus.InProgress;
        MarkAsUpdated();
    }

    public void Complete()
    {
        EnsureNotCompleted();

        Status = TaskItemStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Reopen()
    {
        if (!IsCompleted)
            throw new DomainException("Solo se pueden reabrir tareas completadas.");

        Status = TaskItemStatus.Pending;
        CompletedAt = null;
        MarkAsUpdated();
    }

    public bool IsOverdue(DateTime utcNow) =>
        !IsCompleted && DueDate.HasValue && DueDate.Value < utcNow;

    private void EnsureNotCompleted()
    {
        if (IsCompleted)
            throw new DomainException("No se puede modificar una tarea completada. Reábrela primero.");
    }

    private static string? ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var trimmed = description.Trim();
        if (trimmed.Length > DescriptionMaxLength)
            throw new DomainException($"La descripción no puede superar {DescriptionMaxLength} caracteres.");

        return trimmed;
    }

    private static TaskPriority ValidatePriority(TaskPriority priority)
    {
        if (!Enum.IsDefined(priority))
            throw new DomainException("La prioridad no es válida.");

        return priority;
    }

    private static DateTime? ValidateDueDate(DateTime? dueDate)
    {
        if (dueDate is null)
            return null;

        var utc = dueDate.Value.Kind == DateTimeKind.Utc ? dueDate.Value : dueDate.Value.ToUniversalTime();
        if (utc < DateTime.UtcNow)
            throw new DomainException("La fecha de vencimiento no puede estar en el pasado.");

        return utc;
    }
}
