# Plan de trabajo: API de Tareas en Azure

**Objetivo**: Desplegar una API REST en ASP.NET Core (.NET 10) en Azure App Service, conectada a Azure SQL Database mediante Identidad Administrada de Entra ID (sin contraseñas en el código).

**Herramientas**: Rider, .NET 10 SDK, Azure CLI, cuenta de Azure (Azure for Students - capa gratuita).

**Duración estimada**: 2 sesiones de trabajo (una para local, una para Azure).

---

## Fase 0 — Preparación (5 min, antes de empezar)

- [x] Verificar .NET SDK: `dotnet --version` (.NET 10.0)
- [x] Verificar Azure CLI: `az --version`
- [x] Iniciar sesión en Azure: `az login`
- [x] Confirmar que tienes una suscripción activa: `az account show`

**Criterio de cierre**: los tres comandos responden sin error. ✅

---

## Fase 1 — API local con Clean Architecture (Sesión 1, 60–90 min)

**Objetivo**: API funcionando en local con Swagger y persistencia con EF Core.

### 1.1 Crear el proyecto

- [x] Solución en Clean Architecture (`TaskApi.Domain`, `TaskApi.Application`, `TaskApi.Infrastructure`, `TaskApi.Api`) en .NET 10
- [x] Verificar que arranca: `dotnet run` y abrir `/swagger`

### 1.2 Agregar EF Core + Proveedores de BD

- [x] `Npgsql.EntityFrameworkCore.PostgreSQL` (entorno local con Docker)
- [x] `Microsoft.EntityFrameworkCore.SqlServer` (entorno de producción en Azure SQL Database)
- [x] `Microsoft.EntityFrameworkCore.Design` y `EFCore.NamingConventions`

### 1.3 Crear modelo y contexto

- [x] Crear entidad de dominio `TaskItem`, Value Object `TaskTitle` y enums `TaskPriority`, `TaskItemStatus`
- [x] Crear `ApplicationDbContext` con `DbSet<TaskItem>` y configuración Fluent API (`TaskItemConfiguration`)
- [x] Registrar el `DbContext` en `DependencyInjection.cs` con detección automática de proveedor (PostgreSQL local / Azure SQL en la nube)

### 1.4 Migración / Esquema inicial

- [x] Migración inicial `InitialCreate` para desarrollo local y creación automática de esquema (`EnsureCreatedAsync`) en Azure SQL

### 1.5 Crear el controlador

- [x] Crear `Controllers/TasksController.cs`
- [x] Endpoints: `GET /api/tasks`, `GET /api/tasks/{id}`, `POST /api/tasks`, `PUT /api/tasks/{id}`, `PATCH /api/tasks/{id}/start`, `PATCH /api/tasks/{id}/complete`, `PATCH /api/tasks/{id}/reopen`, `DELETE /api/tasks/{id}`

### 1.6 Probar

- [x] Crear una tarea con `POST` desde Swagger
- [x] Listarla con `GET`
- [x] Borrarla con `DELETE`
- [x] Reiniciar la API y confirmar que los datos persisten

**Criterio de cierre**: los endpoints funcionan en Swagger y los datos sobreviven al reinicio. ✅

---

## Fase 2 — Recursos en Azure (Sesión 2, 30–40 min)

**Objetivo**: tener grupo de recursos, SQL Database y App Service creados.

### 2.1 Grupo de recursos

- [x] `az group create --name rg-tareas-dev --location eastus`

### 2.2 Azure SQL Database

- [x] Crear servidor SQL lógico `sql-tareas-ian1221` en `brazilsouth`
- [x] Crear base de datos `db-tareas` en capa gratuita (`GeneralPurpose` Serverless con `--use-free-limit true` y `AutoPause`)
- [x] Configurar firewall para permitir servicios de Azure (`0.0.0.0`) y tu IP local

### 2.3 App Service

- [x] Crear plan de App Service `plan-tareas-dev` (`F1` gratuito en Linux, `brazilsouth`)
- [x] Crear Web App `api-tareas-ian1221` con runtime `.NET 10` (`DOTNETCORE:10.0`)
- [x] Anotar la URL de la app: `https://api-tareas-ian1221.azurewebsites.net`

**Criterio de cierre**: los tres recursos aparecen en el portal dentro del grupo `rg-tareas-dev`. ✅

---

## Fase 3 — Identidad Administrada y conexión segura (30 min)

**Objetivo**: la app se conecta a SQL sin usuario ni contraseña.

### 3.1 Activar identidad en el App Service

- [x] Activar identidad asignada por el sistema: `az webapp identity assign --resource-group rg-tareas-dev --name api-tareas-ian1221`
- [x] Configurar administrador de Microsoft Entra ID en el servidor SQL (`az sql server ad-admin create`)

### 3.2 Dar acceso a la identidad en SQL

- [x] Conectarse a la base de datos `db-tareas` como admin de Entra ID
- [x] Crear usuario para la identidad administrada (`[api-tareas-ian1221]`) y asignar permisos (`db_datareader`, `db_datawriter`, `db_ddladmin`)

### 3.3 Configurar la cadena de conexión

- [x] En App Service → **Configuración** → agregar connection string `DefaultConnection` con `Authentication=Active Directory Default`
- [x] Sin usuario ni contraseña

**Criterio de cierre**: la cadena de conexión existe en App Service y no contiene secretos. ✅

---

## Fase 4 — Despliegue por línea de comandos (20 min)

**Objetivo**: publicar la API desde la terminal y probarla en la nube.

### 4.1 Publicar y empaquetar

- [x] `dotnet publish src/TaskApi.Api/TaskApi.Api.csproj -c Release -o pub`
- [x] `tar -a -c -f site.zip -C pub .`

### 4.2 Desplegar con Azure CLI

- [x] `az webapp config set --resource-group rg-tareas-dev --name api-tareas-ian1221 --startup-file "dotnet TaskApi.Api.dll"`
- [x] `az webapp deploy --resource-group rg-tareas-dev --name api-tareas-ian1221 --src-path site.zip --type zip`

### 4.3 Probar en la nube

- [x] Abrir `https://api-tareas-ian1221.azurewebsites.net/swagger`
- [x] Repetir las pruebas de `GET`, `POST`, `DELETE`
- [x] Confirmar que los datos llegan a Azure SQL (`200 OK`)

**Criterio de cierre**: la API responde en la URL de Azure y los datos se guardan en SQL. ✅

---

## Fase 5 — Monitoreo y cierre (15 min)

- [x] Revisar que la API y la base de datos respondan sin errores (`200 OK` en `/swagger` y `/api/tasks`)
- [x] Verificar costos en capa gratuita ($0.00 USD: App Service Plan `F1` + Azure SQL Database con `useFreeLimit: true`)
- [x] Documentar en `README.md`: arquitectura, stack, URL, comandos usados, despliegue y consultas de prueba

**Criterio de cierre**: puedes explicar qué hace cada recurso y cuánto cuesta. ✅

---

## ✅ Checklist de "proyecto terminado"

- [x] API corre en local con Swagger
- [x] Recursos creados en Azure dentro de un grupo de recursos (`rg-tareas-dev`)
- [x] Identidad administrada activa y con permisos en SQL
- [x] Cadena de conexión sin secretos (`Authentication=Active Directory Default`)
- [x] Despliegue hecho por CLI (`az webapp deploy`)
- [x] API responde en la URL de Azure (`https://api-tareas-ian1221.azurewebsites.net/swagger`)
- [x] Costos verificados en capa 100% gratuita y documentación en `README.md` completada
