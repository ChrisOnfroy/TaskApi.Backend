using TaskApi.Application.DTOs;
using TaskApi.Application.Features.Tasks.DTOs;

namespace TaskApi.Application.Features.Tasks.Services;

/// <summary>
/// Casos de uso de la funcionalidad Tasks. Es el contrato que consume la capa Presentation.
/// </summary>
public interface ITaskService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<TaskResponse>> GetAllAsync(GetTasksRequest request, CancellationToken cancellationToken = default);
    Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskResponse> StartAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskResponse> CompleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskResponse> ReopenAsync(Guid id, CancellationToken cancellationToken = default);
}
