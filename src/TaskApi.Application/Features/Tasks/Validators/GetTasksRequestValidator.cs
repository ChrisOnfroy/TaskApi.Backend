using FluentValidation;
using TaskApi.Application.Features.Tasks.DTOs;

namespace TaskApi.Application.Features.Tasks.Validators;

public sealed class GetTasksRequestValidator : AbstractValidator<GetTasksRequest>
{
    public GetTasksRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue)
            .WithMessage("El estado no es válido.");

        RuleFor(x => x.Priority)
            .IsInEnum().When(x => x.Priority.HasValue)
            .WithMessage("La prioridad no es válida.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("La página debe ser mayor o igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, GetTasksRequest.MaxPageSize)
            .WithMessage($"El tamaño de página debe estar entre 1 y {GetTasksRequest.MaxPageSize}.");
    }
}
