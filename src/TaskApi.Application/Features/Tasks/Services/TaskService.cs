using FluentValidation;
using TaskApi.Application.DTOs;
using TaskApi.Application.Exceptions;
using TaskApi.Application.Features.Tasks.DTOs;
using TaskApi.Application.Features.Tasks.Mappings;
using TaskApi.Application.Interfaces;
using TaskApi.Domain.Entities;
using TaskApi.Domain.ValueObjects;

namespace TaskApi.Application.Features.Tasks.Services;

/// <summary>
/// Orquesta los casos de uso de tareas: valida la entrada, carga el agregado,
/// delega las reglas de negocio al dominio y persiste mediante abstracciones.
/// </summary>
public sealed class TaskService(
    ITaskRepository taskRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateTaskRequest> createValidator,
    IValidator<UpdateTaskRequest> updateValidator,
    IValidator<GetTasksRequest> getTasksValidator) : ITaskService
{
    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(createValidator, request, cancellationToken);

        var task = new TaskItem(
            TaskTitle.Create(request.Title),
            request.Description,
            request.Priority,
            request.DueDate);

        await taskRepository.AddAsync(task, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return task.ToResponse();
    }

    public async Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetExistingTaskAsync(id, cancellationToken);
        return task.ToResponse();
    }

    public async Task<PagedResponse<TaskResponse>> GetAllAsync(GetTasksRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(getTasksValidator, request, cancellationToken);

        var (items, totalCount) = await taskRepository.GetPagedAsync(
            request.Status,
            request.Priority,
            request.Page,
            request.PageSize,
            cancellationToken);

        return new PagedResponse<TaskResponse>(
            items.Select(t => t.ToResponse()).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }

    public async Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(updateValidator, request, cancellationToken);

        var task = await GetExistingTaskAsync(id, cancellationToken);

        task.UpdateDetails(
            TaskTitle.Create(request.Title),
            request.Description,
            request.Priority,
            request.DueDate);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return task.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetExistingTaskAsync(id, cancellationToken);

        taskRepository.Remove(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<TaskResponse> StartAsync(Guid id, CancellationToken cancellationToken = default) =>
        ChangeStateAsync(id, task => task.Start(), cancellationToken);

    public Task<TaskResponse> CompleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        ChangeStateAsync(id, task => task.Complete(), cancellationToken);

    public Task<TaskResponse> ReopenAsync(Guid id, CancellationToken cancellationToken = default) =>
        ChangeStateAsync(id, task => task.Reopen(), cancellationToken);

    private async Task<TaskResponse> ChangeStateAsync(Guid id, Action<TaskItem> transition, CancellationToken cancellationToken)
    {
        var task = await GetExistingTaskAsync(id, cancellationToken);

        transition(task); // El dominio decide si la transición es válida (lanza DomainException si no).

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return task.ToResponse();
    }

    private async Task<TaskItem> GetExistingTaskAsync(Guid id, CancellationToken cancellationToken) =>
        await taskRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException("Tarea", id);

    private static async Task ValidateAsync<T>(IValidator<T> validator, T instance, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (result.IsValid)
            return;

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        throw new RequestValidationException(errors);
    }
}
