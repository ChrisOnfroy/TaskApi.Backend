namespace TaskApi.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se viola una regla o invariante del negocio.
/// La capa de presentación la traducirá a un HTTP 400/422.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
