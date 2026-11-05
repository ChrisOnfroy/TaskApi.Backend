namespace TaskApi.Application.Exceptions;

/// <summary>
/// Se lanza cuando los datos de entrada no superan la validación.
/// Agrupa los errores por propiedad. La API la traduce a HTTP 400 (ValidationProblemDetails).
/// </summary>
public sealed class RequestValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Se produjeron uno o más errores de validación.")
    {
        Errors = errors;
    }
}
