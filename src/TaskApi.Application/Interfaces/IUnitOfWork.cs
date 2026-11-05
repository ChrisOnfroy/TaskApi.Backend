namespace TaskApi.Application.Interfaces;

/// <summary>
/// Confirma de forma atómica todos los cambios realizados durante un caso de uso.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
