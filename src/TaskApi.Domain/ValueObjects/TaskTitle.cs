using TaskApi.Domain.Exceptions;

namespace TaskApi.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el título de una tarea.
/// Es inmutable, se compara por valor y garantiza que nunca exista un título inválido.
/// </summary>
public sealed record TaskTitle
{
    public const int MaxLength = 150;

    public string Value { get; }

    private TaskTitle(string value) => Value = value;

    public static TaskTitle Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El título es obligatorio.");

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new DomainException($"El título no puede superar {MaxLength} caracteres.");

        return new TaskTitle(trimmed);
    }

    public override string ToString() => Value;
}
