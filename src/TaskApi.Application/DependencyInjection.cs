using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskApi.Application.Features.Tasks.Services;

namespace TaskApi.Application;

/// <summary>
/// Registro de los servicios de la capa Application.
/// Se invoca desde Program.cs con builder.Services.AddApplication().
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<ITaskService, TaskService>();

        return services;
    }
}
