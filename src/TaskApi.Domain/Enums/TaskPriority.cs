namespace TaskApi.Domain.Enums;

/// <summary>
/// Nivel de prioridad de una tarea.
/// Los valores numéricos son explícitos para que se persistan de forma estable.
/// </summary>
public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3
}
