# Mid-Point Report — todo-app

**Date:** 2026-06-27 (updated)
**Change:** todo-app

## Progress Summary

| Metric | Value |
|---|---|
| Tasks completed | 6 / 52 |
| Tasks remaining | 46 |
| Completion | 11.5% |

## What Was Implemented

### Task 1.1 — Create .NET solution and Web API project
- Created `TodoApp.slnx` (.NET 10 solution file in `.slnx` format)
- Created `TodoApi/` project with `TodoApi.csproj` targeting net10.0
- Created full folder structure per design.md: `Controllers/`, `Models/Entities/`, `Models/DTOs/Auth/`, `Models/DTOs/Todos/`, `Models/DTOs/Categories/`, `Models/Mappings/`, `Services/Interfaces/`, `Data/Configurations/`, `Middleware/`
- Removed default template files (`WeatherForecast.cs`, `WeatherForecastController.cs`, `TodoApi.http`)
- Cleaned `Program.cs` (removed OpenAPI references per design)
- Solution builds with 0 warnings, 0 errors

### Task 1.2 — Add NuGet packages
All 6 required packages added to `TodoApi.csproj`:
- `Microsoft.EntityFrameworkCore` 10.0.9
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.2
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.9
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.9
- `Cartographer.Mapper` 0.2.0
- `FluentResponse.ApiWrapper` 1.0.0
- All packages restored successfully, build passes

### Task 1.3 — Configure JWT authentication in Program.cs
- JWT bearer token configured in `Program.cs` with `AddAuthentication().AddJwtBearer()`
- Token validation parameters: ValidateIssuer, ValidateAudience, ValidateLifetime, ValidateIssuerSigningKey
- Issuer, Audience, SigningKey read from `appsettings.json` configuration
- `UseAuthentication()` and `UseAuthorization()` middleware added to pipeline
- JWT section added to `appsettings.json` with Key, Issuer, Audience, ExpiryInHours (24h)

### Task 1.4 — Configure CORS and controller routing
- CORS policy `AllowAngularDev` configured for `http://localhost:4200`
- `UseCors()` placed before `UseAuthentication()` in middleware pipeline
- `AddControllers()` and `MapControllers()` already present from template

### Task 1.5 — Create Cartographer.Mapper mapping profile
- `Models/Mappings/MappingProfile.cs` created with Profile class and `ConfigureMappings` method
- Cartographer registered via `AddCartographer()` with profile applied
- Actual entity-to-DTO mappings will be added in Tasks 4.1 and 5.1

### Task 1.6 — Configure FluentResponse.ApiWrapper
- `AddFluentResponse()` registered in DI
- `UseFluentResponseExceptionHandler()` middleware added (global exception handling)
- `UseFluentResponseCorrelationId()` middleware added

## Deviations from Spec

- **None.** All implementation follows the approved design.md, tasks.md, and spec files.

## Files Created/Modified

| File | Action |
|---|---|
| `TodoApp.slnx` | Created |
| `TodoApi/TodoApi.csproj` | Created |
| `TodoApi/Program.cs` | Created, modified (Tasks 1.3, 1.4, 1.5, 1.6) |
| `TodoApi/appsettings.json` | Created, modified (Task 1.3) |
| `TodoApi/Properties/launchSettings.json` | Created (template default) |
| `TodoApi/Models/Mappings/MappingProfile.cs` | Created (Task 1.5) |
| `TodoApi/Controllers/` (empty) | Created |
| `TodoApi/Models/Entities/` (empty) | Created |
| `TodoApi/Models/DTOs/Auth/` (empty) | Created |
| `TodoApi/Models/DTOs/Todos/` (empty) | Created |
| `TodoApi/Models/DTOs/Categories/` (empty) | Created |
| `TodoApi/Models/Mappings/` | Created |
| `TodoApi/Services/Interfaces/` (empty) | Created |
| `TodoApi/Data/Configurations/` (empty) | Created |
| `TodoApi/Middleware/` (empty) | Created |
| `GLOBAL_RULES.md` | Created |
| `openspec/changes/todo-app/midpoint-report.md` | Created (Gate 2) |

## Remaining Work

### Data Layer (4 tasks)
- Tasks 2.1-2.4: Category entity, Todo entity, AppDbContext, EF Core migration

### Data Layer (4 tasks)
- Tasks 2.1-2.4: Category entity, Todo entity, AppDbContext, EF Core migration

### Authentication API (6 tasks)
- Tasks 3.1-3.6: Auth DTOs, AuthService (register/login), AuthController, claims helper

### Categories API (5 tasks)
- Tasks 4.1-4.5: Category DTOs, CategoryService, CategoryController

### Todos API (7 tasks)
- Tasks 5.1-5.7: Todo DTOs, TodoService, TodoController

### Frontend Setup (5 tasks)
- Tasks 6.1-6.5: Angular project, Material, API models, HttpClient/interceptor, routes/guard

### Authentication UI (4 tasks)
- Tasks 7.1-7.4: AuthService, LoginComponent, RegisterComponent

### Categories UI (4 tasks)
- Tasks 8.1-8.4: CategoryService, CategoryListComponent, CategoryFormComponent, delete confirmation

### Todo UI (6 tasks)
- Tasks 9.1-9.6: TodoService, TodoListComponent, TodoFormComponent, completion toggle

### Validation (5 tasks)
- Tasks 10.1-10.5: E2E tests for auth, categories, todos, filters, spec coverage

## Known Issues
- None at this point.
