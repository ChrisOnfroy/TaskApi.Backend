using Microsoft.AspNetCore.Mvc;
using TaskApi.Application.DTOs;
using TaskApi.Application.Features.Tasks.DTOs;
using TaskApi.Application.Features.Tasks.Services;

namespace TaskApi.Api.Controllers;

/// <summary>
/// Controlador REST para el recurso Tasks.
/// No contiene lógica de negocio: solo recibe solicitudes HTTP, delega en ITaskService
/// y devuelve el código de estado HTTP correspondiente.
/// </summary>
[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    /// <summary>
    /// Lista las tareas de forma paginada, con filtros opcionales por estado y prioridad.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<TaskResponse>>> GetAll(
        [FromQuery] GetTasksRequest request,
        CancellationToken cancellationToken)
    {
        var response = await taskService.GetAllAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Obtiene una tarea por su identificador único.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await taskService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Crea una nueva tarea en estado Pending.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TaskResponse>> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var created = await taskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Actualiza el título, descripción, prioridad y fecha de vencimiento de una tarea existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TaskResponse>> Update(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await taskService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Inicia una tarea (transición de estado a InProgress).
    /// </summary>
    [HttpPatch("{id:guid}/start")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TaskResponse>> Start(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await taskService.StartAsync(id, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Marca una tarea como completada (transición de estado a Completed).
    /// </summary>
    [HttpPatch("{id:guid}/complete")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TaskResponse>> Complete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await taskService.CompleteAsync(id, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Reabre una tarea completada (transición de estado a Pending).
    /// </summary>
    [HttpPatch("{id:guid}/reopen")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TaskResponse>> Reopen(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await taskService.ReopenAsync(id, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Elimina permanentemente una tarea.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await taskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
