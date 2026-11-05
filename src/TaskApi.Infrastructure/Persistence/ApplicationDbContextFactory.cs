using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskApi.Infrastructure.Persistence;

/// <summary>
/// Usada SOLO por las herramientas de EF Core (dotnet ef) en tiempo de diseño.
/// Permite generar migraciones directamente desde Infrastructure sin necesitar el proyecto API.
/// Lee la variable de entorno ConnectionStrings__DefaultConnection; si no existe,
/// usa la del contenedor Docker local de desarrollo (docker-compose.yml).
/// No se utiliza en tiempo de ejecución.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string LocalDevelopmentConnection =
        "Host=localhost;Port=5433;Database=taskapi;Username=postgres;Password=123456";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? LocalDevelopmentConnection;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ApplicationDbContext(options);
    }
}
