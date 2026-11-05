namespace TaskApi.Application.Exceptions;

/// <summary>
/// Se lanza cuando un recurso solicitado no existe. La API la traduce a HTTP 404.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} con identificador '{key}' no fue encontrado.")
    {
    }
}
