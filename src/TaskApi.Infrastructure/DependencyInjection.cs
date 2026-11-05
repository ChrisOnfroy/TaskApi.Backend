using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskApi.Application.Interfaces;
using TaskApi.Infrastructure.Persistence;
using TaskApi.Infrastructure.Persistence.Repositories;

namespace TaskApi.Infrastructure;

/// <summary>
/// Registro de los servicios de la capa Infrastructure.
/// Se invoca desde Program.cs con builder.Services.AddInfrastructure(builder.Configuration).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");

        // AddDbContext registra ApplicationDbContext como Scoped (una instancia por petición HTTP).
        // Soporta PostgreSQL en local (Host=...) y Azure SQL Database en la nube (Server=...).
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3));
            }
            else
            {
                options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(maxRetryCount: 3));
            }

            options.UseSnakeCaseNamingConvention();
        });

        // Misma instancia del DbContext dentro de la petición => repositorio y UoW comparten el tracking.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}
