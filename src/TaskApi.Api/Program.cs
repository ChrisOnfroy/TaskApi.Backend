using TaskApi.Api.Extensions;
using TaskApi.Api.Middleware;
using TaskApi.Application;
using TaskApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Registro modular de servicios por capa.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

var app = builder.Build();

// 2. Pipeline de middlewares HTTP.
// El middleware de excepciones va primero para capturar cualquier fallo en el resto del pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskApi v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(PresentationServiceExtensions.DefaultCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
