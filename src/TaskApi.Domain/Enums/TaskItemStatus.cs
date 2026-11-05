namespace TaskApi.Domain.Enums;

/// <summary>
/// Estado del ciclo de vida de una tarea.
/// Se llama TaskItemStatus para no colisionar con System.Threading.Tasks.TaskStatus.
/// </summary>
public enum TaskItemStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3
}
