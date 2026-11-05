using FluentValidation;
using TaskApi.Application.Features.Tasks.DTOs;
using TaskApi.Domain.Entities;
using TaskApi.Domain.ValueObjects;

namespace TaskApi.Application.Features.Tasks.Validators;

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(TaskTitle.MaxLength)
            .WithMessage($"El título no puede superar {TaskTitle.MaxLength} caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(TaskItem.DescriptionMaxLength)
            .WithMessage($"La descripción no puede superar {TaskItem.DescriptionMaxLength} caracteres.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("La prioridad no es válida.");

        RuleFor(x => x.DueDate)
            .Must(BeInTheFuture).When(x => x.DueDate.HasValue)
            .WithMessage("La fecha de vencimiento no puede estar en el pasado.");
    }

    private static bool BeInTheFuture(DateTime? dueDate) =>
        dueDate!.Value.ToUniversalTime() > DateTime.UtcNow;
}
