using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;

namespace TaskApi.Api.Extensions;

/// <summary>
/// Métodos de extensión para organizar el registro de servicios propios de la capa Presentation (API).
/// Mantiene Program.cs limpio y fácil de leer.
/// </summary>
public static class PresentationServiceExtensions
{
    public const string DefaultCorsPolicy = "DefaultCorsPolicy";

    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                // Serializa y deserializa los enums como texto ("Low", "Pending", etc.).
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddProblemDetails();

        // Configuración de CORS (lee orígenes permitidos de appsettings.json o permite cualquiera en desarrollo).
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicy, policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
                else
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
            });
        });

        // Configuración de Swagger / OpenAPI.
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TaskApi - Clean Architecture API",
                Version = "v1",
                Description = "API REST para la gestión de tareas construida con ASP.NET Core, EF Core, PostgreSQL y Clean Architecture."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }
}
