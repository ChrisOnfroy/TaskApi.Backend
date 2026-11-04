Plan de trabajo: API de Tareas en Azure
Objetivo: Desplegar una API REST en ASP.NET Core en Azure App Service, conectada a Azure SQL Database mediante Identidad Administrada de Entra ID (sin contraseñas en el código).

Herramientas: Rider, .NET 8 SDK, Azure CLI, cuenta de Azure (capa gratuita).

Duración estimada: 2 sesiones de trabajo (una para local, una para Azure).

Fase 0 — Preparación (5 min, antes de empezar)
□ Verificar .NET SDK: dotnet --version (debe mostrar 8.x)
□ Verificar Azure CLI: az --version
□ Iniciar sesión en Azure: az login
□ Confirmar que tienes una suscripción activa: az account show
Criterio de cierre: los tres comandos responden sin error.

Fase 1 — API local con SQLite (Sesión 1, 60–90 min)
Objetivo: API funcionando en local con Swagger y persistencia en SQLite.

1.1 Crear el proyecto
□ En Rider: New Solution → plantilla Web API → .NET 8 → nombre ApiTareas
□ Verificar que arranca: dotnet run y abrir /swagger
1.2 Agregar EF Core + SQLite
□ dotnet add package Microsoft.EntityFrameworkCore.Sqlite
□ dotnet add package Microsoft.EntityFrameworkCore.Design
1.3 Crear modelo y contexto
□ Crear Models/Tarea.cs con Id, Titulo, Completada
□ Crear Data/AppDbContext.cs con DbSet<Tarea>
□ Registrar el DbContext en Program.cs con la cadena de conexión SQLite
1.4 Migración inicial
□ dotnet ef migrations add InitialCreate
□ dotnet ef database update
1.5 Crear el controlador
□ Crear Controllers/TareasController.cs
□ Endpoints: GET /api/tareas, POST /api/tareas, DELETE /api/tareas/{id}
1.6 Probar
□ Crear una tarea con POST desde Swagger
□ Listarla con GET
□ Borrarla con DELETE
□ Reiniciar la API y confirmar que los datos persisten
Criterio de cierre: los tres endpoints funcionan en Swagger y los datos sobreviven al reinicio.

Fase 2 — Recursos en Azure (Sesión 2, 30–40 min)
Objetivo: tener grupo de recursos, SQL Database y App Service creados.

2.1 Grupo de recursos
□ az group create --name rg-tareas-dev --location eastus
2.2 Azure SQL Database
□ Crear servidor SQL lógico (usuario admin y contraseña)
□ Crear base de datos en capa gratuita o básica
□ Configurar firewall para permitir tu IP local (solo para pruebas)
2.3 App Service
□ Crear plan de App Service (F1 gratuito o B1)
□ Crear Web App con runtime .NET 8
□ Anotar la URL de la app
Criterio de cierre: los tres recursos aparecen en el portal dentro del grupo rg-tareas-dev.

Fase 3 — Identidad Administrada y conexión segura (30 min)
Objetivo: la app se conecta a SQL sin usuario ni contraseña.

3.1 Activar identidad en el App Service
□ En el portal: App Service → Identidad → activar asignada por el sistema
□ Copiar el Object ID de la identidad
3.2 Dar acceso a la identidad en SQL
□ Conectarse a la base de datos como admin
□ Crear usuario para la identidad administrada y asignar permisos (db_datareader, db_datawriter, 
db_ddladmin
)
3.3 Configurar la cadena de conexión
□ En App Service → Configuración → agregar connection string con Authentication=Active Directory Managed Identity
□ Sin usuario ni contraseña
Criterio de cierre: la cadena de conexión existe en App Service y no contiene secretos.

Fase 4 — Despliegue por línea de comandos (20 min)
Objetivo: publicar la API desde la terminal y probarla en la nube.

4.1 Publicar y empaquetar
□ dotnet publish -o pub
□ cd pub && zip -r site.zip *
4.2 Desplegar con Azure CLI
□ az webapp deploy --src-path site.zip --resource-group rg-tareas-dev --name <tu-app>
4.3 Probar en la nube
□ Abrir https://<tu-app>.azurewebsites.net/swagger
□ Repetir las pruebas de GET, POST, DELETE
□ Confirmar que los datos llegan a Azure SQL
Criterio de cierre: la API responde en la URL de Azure y los datos se guardan en SQL.

Fase 5 — Monitoreo y cierre (15 min)
□ Abrir Log Stream del App Service y revisar que no haya errores
□ Activar Application Insights (opcional pero recomendado)
□ Revisar Cost Management para ver el gasto real del proyecto
□ Documentar en un README: URL, comandos usados y aprendizajes
Criterio de cierre: puedes explicar qué hace cada recurso y cuánto cuesta.

Resumen de comandos clave
bash
# Local
dotnet new webapi -n ApiTareas
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run

# Azure
az login
az group create --name rg-tareas-dev --location eastus
az webapp deploy --src-path site.zip --resource-group rg-tareas-dev --name <tu-app>
Checklist de "proyecto terminado"
□ API corre en local con Swagger
□ Recursos creados en Azure dentro de un grupo de recursos
□ Identidad administrada activa y con permisos en SQL
□ Cadena de conexión sin secretos
□ Despliegue hecho por CLI
□ API responde en la URL de Azure
□ Logs revisados y costos consultados
