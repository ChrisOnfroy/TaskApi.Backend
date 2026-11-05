using Microsoft.EntityFrameworkCore;
using TaskApi.Api.Extensions;
using TaskApi.Api.Middleware;
using TaskApi.Application;
using TaskApi.Infrastructure;
using TaskApi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Registro modular de servicios por capa.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

var app = builder.Build();

// Crear esquema automáticamente en Azure SQL Database si aún no existe.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.IsSqlServer())
    {
        await db.Database.EnsureCreatedAsync();
    }
}

// 2. Pipeline de middlewares HTTP.
// El middleware de excepciones va primero para capturar cualquier fallo en el resto del pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskApi v1");
    options.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(PresentationServiceExtensions.DefaultCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
