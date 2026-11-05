# TaskApi.Backend — API REST de Gestión de Tareas en .NET 10 y Azure

API RESTful profesional para la gestión de tareas (**Task Management**), construida bajo los principios de **Clean Architecture** y **Domain-Driven Design (DDD)** con **.NET 10**, desplegada en **Azure App Service (Linux)** y conectada de forma 100% segura (sin contraseñas en código ni en configuración) a **Azure SQL Database** mediante **Identidad Administrada (Microsoft Entra ID)**.

---

## 🌐 Demo en Vivo (Azure Cloud)

- **Swagger UI (Documentación interactiva):** [https://api-tareas-ian1221.azurewebsites.net/swagger](https://api-tareas-ian1221.azurewebsites.net/swagger)
- **Endpoint base de Tareas:** [https://api-tareas-ian1221.azurewebsites.net/api/tasks](https://api-tareas-ian1221.azurewebsites.net/api/tasks)

---

## 💡 Idea y Propósito del Proyecto

El objetivo del proyecto es diseñar y desplegar un backend moderno y escalable para administrar el ciclo de vida completo de tareas (`Pending` → `InProgress` → `Completed`), cumpliendo con estándares empresariales tanto en arquitectura de software como en seguridad en la nube:

1. **Dominio Rico y Validado**: Las reglas de negocio viven en la capa de Dominio (por ejemplo, una tarea completada no puede editarse sin reabrirse primero, ni iniciarse dos veces).
2. **Persistencia Híbrida (Local + Cloud)**:
   - En **desarrollo local**, se conecta a **PostgreSQL** en Docker.
   - En **producción (Azure)**, se conecta automáticamente a **Azure SQL Database**.
3. **Seguridad *Zero-Secret* en Azure**: La conexión entre el App Service y Azure SQL utiliza **Managed Identity (`Active Directory Default`)**, eliminando por completo usuarios y contraseñas de las cadenas de conexión.
4. **Costo \$0.00 USD (Optimizado para Azure for Students)**: Todos los recursos en la nube fueron aprovisionados utilizando las capas gratuitas (`F1 Free Plan` en App Service y `Serverless Free Limit` con `AutoPause` en Azure SQL Database).

---

## 🛠️ Stack Tecnológico Explicado

| Tecnología / Librería | Versión | ¿Para qué se usó en el proyecto? |
| :--- | :--- | :--- |
| **.NET (C#)** | `.NET 10.0 (LTS)` | Plataforma principal de ejecución (Long-Term Support). |
| **ASP.NET Core Web API** | `10.0` | Construcción de los controladores REST (`TasksController`), inyección de dependencias, manejo global de errores (`ProblemDetails` RFC 7807) y pipeline HTTP. |
| **Entity Framework Core** | `10.0.4` | ORM para acceso a datos mediante patrón Repositorio + Unidad de Trabajo (`IUnitOfWork`), mapeo con *Fluent API* e índices optimizados. |
| **Microsoft.EntityFrameworkCore.SqlServer** | `10.0.4` | Proveedor de base de datos para **Azure SQL Database** en producción, con soporte nativo para `Azure.Identity` (Entra ID) y reintentos automáticos ante fallos transitorios (`EnableRetryOnFailure`). |
| **Npgsql.EntityFrameworkCore.PostgreSQL** | `10.0.3` | Proveedor de base de datos para desarrollo local sobre contenedores Docker con **PostgreSQL 17**. |
| **EFCore.NamingConventions** | `10.0.1` | Convierte automáticamente nombres de tablas y columnas a formato `snake_case` (`created_at`, `due_date`, `completed_at`). |
| **FluentValidation** | `12.2.0` | Validación declarativa de los DTOs de entrada (`CreateTaskRequestValidator`, `UpdateTaskRequestValidator`, `GetTasksRequestValidator`) antes de llegar a los casos de uso. |
| **Swashbuckle (Swagger / OpenAPI)** | `10.3.1` | Generación automática de documentación interactiva OpenAPI en `/swagger` para probar todos los endpoints desde el navegador. |
| **Azure App Service (Linux)** | Plan `F1` | Servidor de aplicaciones PaaS en la nube donde corre el binario publicado de `.NET 10`. |
| **Azure SQL Database** | `GP_S_Gen5_1` (Free Tier) | Base de datos relacional Serverless en la nube con límite gratuito mensual activado y pausa automática (`AutoPause`). |
| **Microsoft Entra ID (Managed Identity)** | System-Assigned | Identidad administrada del App Service para autenticarse contra Azure SQL sin usar contraseñas. |

---

## 🏛️ Arquitectura del Proyecto (Clean Architecture)

La solución está dividida en **4 capas** estrictamente desacopladas dentro de `src/`:

```text
TaskApi.Backend/
├── src/
│   ├── TaskApi.Domain/           # Núcleo del negocio (sin dependencias externas)
│   │   ├── Entities/             # Entidad agregada TaskItem con transiciones de estado
│   │   ├── ValueObjects/         # TaskTitle (inmutable y autovalidado)
│   │   ├── Enums/                # TaskPriority (Low, Medium, High) y TaskItemStatus
│   │   └── Exceptions/           # Excepciones de dominio (DomainException, NotFoundException)
│   │
│   ├── TaskApi.Application/      # Casos de uso y contratos de la aplicación
│   │   ├── Features/Tasks/       # DTOs, Validadores (FluentValidation) y TaskService
│   │   └── Interfaces/           # Contratos ITaskRepository e IUnitOfWork
│   │
│   ├── TaskApi.Infrastructure/   # Implementación de persistencia y acceso a datos
│   │   ├── Persistence/          # ApplicationDbContext, Configurations (Fluent API) y Migrations
│   │   └── DependencyInjection.cs# Selección dinámica PostgreSQL (local) / SQL Server (Azure)
│   │
│   └── TaskApi.Api/              # Capa de presentación HTTP (Entrypoint)
│       ├── Controllers/          # TasksController (/api/tasks)
│       ├── Middleware/           # ExceptionHandlingMiddleware (traduce excepciones a HTTP 400/404/422/500)
│       └── Program.cs            # Configuración del host, Swagger y creación de esquema en Azure SQL
├── docker-compose.yml            # Contenedor PostgreSQL 17 para entorno local
├── Plan.md                       # Checklist de fases del proyecto completadas
└── README.md                     # Documentación general del proyecto
```

---

## ☁️ Cómo se Creó y Desplegó en Azure (Paso a Paso con Azure CLI)

A continuación se documentan todos los comandos reales ejecutados desde PowerShell para aprovisionar la infraestructura, configurar la seguridad sin contraseñas y desplegar la API.

### 1. Instalación de Azure CLI e Inicio de Sesión
```powershell
winget install -e --id Microsoft.AzureCLI
az login
az account show
```

### 2. Creación del Grupo de Recursos
```powershell
az group create --name rg-tareas-dev --location eastus
```

### 3. Creación del Servidor SQL Lógico y Base de Datos Gratuita (`brazilsouth`)
> En suscripciones *Azure for Students*, la política de regiones permitidas habilita `brazilsouth`. Además, la base de datos se creó activando `--use-free-limit true` y `AutoPause` para garantizar costo **\$0.00 USD**.

```powershell
# 1. Crear el servidor SQL lógico
az sql server create `
  --name sql-tareas-ian1221 `
  --resource-group rg-tareas-dev `
  --location brazilsouth `
  --admin-user sqladmin `
  --admin-password "TuPasswordSegura2026*!"

# 2. Crear la base de datos en capa Serverless 100% Gratuita con AutoPause
az sql db create `
  --resource-group rg-tareas-dev `
  --server sql-tareas-ian1221 `
  --name db-tareas `
  --edition GeneralPurpose `
  --family Gen5 `
  --capacity 1 `
  --compute-model Serverless `
  --use-free-limit true `
  --free-limit-exhaustion-behavior AutoPause

# 3. Abrir el firewall para servicios de Azure y para la IP local de administración
az sql server firewall-rule create --resource-group rg-tareas-dev --server sql-tareas-ian1221 --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
az sql server firewall-rule create --resource-group rg-tareas-dev --server sql-tareas-ian1221 --name AllowMyIP --start-ip-address 167.0.197.167 --end-ip-address 167.0.197.167
```

### 4. Creación del Plan de App Service (`F1` Gratuito) y Web App (`.NET 10`)
```powershell
# 1. Crear Plan en Linux capa gratuita F1 ($0.00 USD)
az appservice plan create `
  --name plan-tareas-dev `
  --resource-group rg-tareas-dev `
  --location brazilsouth `
  --sku F1 `
  --is-linux

# 2. Crear el App Service con runtime .NET 10 LTS
az webapp create `
  --name api-tareas-ian1221 `
  --resource-group rg-tareas-dev `
  --plan plan-tareas-dev `
  --runtime "DOTNETCORE:10.0"
```

### 5. Configuración de Identidad Administrada (Conexión sin Contraseñas)
```powershell
# 1. Activar System-Assigned Managed Identity en el App Service
az webapp identity assign --resource-group rg-tareas-dev --name api-tareas-ian1221

# 2. Asignar al usuario actual de Microsoft Entra ID como Administrador del servidor SQL
$miId = az ad signed-in-user show --query id -o tsv
$miCorreo = az ad signed-in-user show --query userPrincipalName -o tsv
az sql server ad-admin create --resource-group rg-tareas-dev --server-name sql-tareas-ian1221 --display-name $miCorreo --object-id $miId
```

Luego, en el **Editor de consultas (Query Editor)** de la base de datos `db-tareas` en el Portal de Azure, se otorgaron permisos a la identidad del App Service:
```sql
CREATE USER [api-tareas-ian1221] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [api-tareas-ian1221];
ALTER ROLE db_datawriter ADD MEMBER [api-tareas-ian1221];
ALTER ROLE db_ddladmin ADD MEMBER [api-tareas-ian1221];
```

Y se configuró la cadena de conexión libre de secretos en el App Service:
```powershell
az webapp config connection-string set `
  --resource-group rg-tareas-dev `
  --name api-tareas-ian1221 `
  --connection-string-type SQLAzure `
  --settings DefaultConnection="Server=tcp:sql-tareas-ian1221.database.windows.net,1433;Database=db-tareas;Authentication=Active Directory Default;Encrypt=True;"
```

### 6. Compilación, Empaquetado y Despliegue por CLI
```powershell
# 1. Publicar el proyecto en modo Release hacia la carpeta pub/
dotnet publish src/TaskApi.Api/TaskApi.Api.csproj -c Release -o pub

# 2. Empaquetar en formato ZIP compatible con rutas Linux
tar -a -c -f site.zip -C pub .

# 3. Configurar el comando de arranque y desplegar el paquete ZIP
az webapp config set --resource-group rg-tareas-dev --name api-tareas-ian1221 --startup-file "dotnet TaskApi.Api.dll"
az webapp deploy --resource-group rg-tareas-dev --name api-tareas-ian1221 --src-path site.zip --type zip
```

---

## 🧪 Endpoints y Consultas de Prueba

Puedes probar todos los endpoints directamente desde **Swagger UI** en [https://api-tareas-ian1221.azurewebsites.net/swagger](https://api-tareas-ian1221.azurewebsites.net/swagger) o desde tu terminal con PowerShell / `curl`.

### Tabla de Endpoints Disponibles

| Método | Ruta | Descripción | Código Éxito |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/tasks` | Lista tareas paginadas (`page`, `pageSize`) con filtros opcionales (`status`, `priority`). | `200 OK` |
| `GET` | `/api/tasks/{id}` | Obtiene el detalle de una tarea por su `Guid`. | `200 OK` |
| `POST` | `/api/tasks` | Crea una nueva tarea en estado `Pending`. | `201 Created` |
| `PUT` | `/api/tasks/{id}` | Actualiza título, descripción, prioridad y fecha límite de una tarea. | `200 OK` |
| `PATCH` | `/api/tasks/{id}/start` | Cambia el estado de la tarea a `InProgress`. | `200 OK` |
| `PATCH` | `/api/tasks/{id}/complete` | Marca la tarea como `Completed` y registra `completedAt`. | `200 OK` |
| `PATCH` | `/api/tasks/{id}/reopen` | Reabre una tarea completada pasándola de nuevo a `Pending`. | `200 OK` |
| `DELETE` | `/api/tasks/{id}` | Elimina permanentemente una tarea de la base de datos. | `204 No Content` |

---

### Ejemplos Prácticos de Prueba

#### 1. Crear una tarea (`POST /api/tasks`)
**JSON para pegar en Swagger:**
```json
{
  "title": "Entregar proyecto de Azure con Clean Architecture",
  "description": "Desplegar API en App Service conectada a Azure SQL con Managed Identity",
  "priority": "High",
  "dueDate": "2026-12-01T18:00:00Z"
}
```
*(Valores válidos para `priority`: `"Low"`, `"Medium"`, `"High"`).*

**Comando en PowerShell:**
```powershell
$body = @{
  title = "Entregar proyecto de Azure con Clean Architecture"
  description = "Desplegar API en App Service conectada a Azure SQL con Managed Identity"
  priority = "High"
  dueDate = "2026-12-01T18:00:00Z"
} | ConvertTo-Json

$tarea = Invoke-RestMethod -Method Post `
  -Uri "https://api-tareas-ian1221.azurewebsites.net/api/tasks" `
  -ContentType "application/json" `
  -Body $body

$tarea
```

#### 2. Listar tareas con paginación y filtros (`GET /api/tasks`)
```powershell
# Listar todas (página 1, 10 elementos por página)
Invoke-RestMethod -Method Get -Uri "https://api-tareas-ian1221.azurewebsites.net/api/tasks?page=1&pageSize=10"

# Filtrar solo tareas pendientes de prioridad alta
Invoke-RestMethod -Method Get -Uri "https://api-tareas-ian1221.azurewebsites.net/api/tasks?status=Pending&priority=High"
```

#### 3. Iniciar y Completar una tarea (`PATCH`)
```powershell
# Pasar a InProgress
Invoke-RestMethod -Method Patch -Uri "https://api-tareas-ian1221.azurewebsites.net/api/tasks/$($tarea.id)/start"

# Marcar como Completed
Invoke-RestMethod -Method Patch -Uri "https://api-tareas-ian1221.azurewebsites.net/api/tasks/$($tarea.id)/complete"
```

#### 4. Actualizar una tarea (`PUT /api/tasks/{id}`)
**JSON para Swagger:**
```json
{
  "title": "Proyecto de Azure entregado y documentado",
  "description": "Actualización de prueba en Azure SQL Database",
  "priority": "Medium",
  "dueDate": "2026-12-05T18:00:00Z"
}
```

#### 5. Eliminar una tarea (`DELETE /api/tasks/{id}`)
```powershell
Invoke-RestMethod -Method Delete -Uri "https://api-tareas-ian1221.azurewebsites.net/api/tasks/$($tarea.id)"
```

---

## 💻 Cómo Ejecutar en Local (con Docker y PostgreSQL)

1. Levantar el contenedor de PostgreSQL:
   ```powershell
   docker compose up -d
   ```
2. Aplicar migraciones y ejecutar la API:
   ```powershell
   dotnet run --project src/TaskApi.Api/TaskApi.Api.csproj
   ```
3. Abrir Swagger local en `http://localhost:5220/swagger`.
