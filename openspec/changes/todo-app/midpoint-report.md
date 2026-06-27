# Mid-Point Report — todo-app

**Date:** 2026-06-27 (updated — Gate 2 re-check after Phase 4)
**Change:** todo-app

## Progress Summary

| Metric | Value |
|---|---|
| Tasks completed | 21 / 52 |
| Tasks remaining | 31 |
| Completion | 40.4% |

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
| `TodoApi/Data/AppDbContext.cs` | Created (Task 2.3) |
| `TodoApi/Data/Configurations/CategoryConfiguration.cs` | Created (Task 2.1) |
| `TodoApi/Data/Configurations/TodoConfiguration.cs` | Created (Task 2.2) |
| `TodoApi/Data/Migrations/20260627093223_InitialCreate.cs` | Created (Task 2.4) |
| `TodoApi/Models/Entities/Category.cs` | Created (Task 2.1) |
| `TodoApi/Models/Entities/Todo.cs` | Created (Task 2.2) |
| `TodoApi/Models/DTOs/Auth/RegisterRequest.cs` | Created (Task 3.1) |
| `TodoApi/Models/DTOs/Auth/LoginRequest.cs` | Created (Task 3.1) |
| `TodoApi/Models/DTOs/Auth/AuthResponse.cs` | Created (Task 3.1) |
| `TodoApi/Services/Interfaces/IAuthService.cs` | Created (Task 3.2) |
| `TodoApi/Services/AuthService.cs` | Created (Tasks 3.2, 3.3) |
| `TodoApi/Controllers/AuthController.cs` | Created (Tasks 3.4, 3.5) |
| `TodoApi/Extensions/ClaimsPrincipalExtensions.cs` | Created (Task 3.6) |
| `TodoApi.Tests/TodoApi.Tests.csproj` | Created (Gate 3 fix) |
| `TodoApi.Tests/Entities/CategoryTests.cs` | Created (Gate 3 fix) |
| `TodoApi.Tests/Entities/TodoTests.cs` | Created (Gate 3 fix) |
| `TodoApi.Tests/Data/AppDbContextTests.cs` | Created (Gate 3 fix) |
| `TodoApi.Tests/Mappings/MappingProfileTests.cs` | Created (Gate 3 fix) |
| `TodoApi.Tests/Services/AuthServiceTests.cs` | Created (Task 3.2) |
| `TodoApi.Tests/Extensions/ClaimsPrincipalExtensionsTests.cs` | Created (Task 3.6) |
| `openspec/changes/todo-app/regression-plan.md` | Created (Gate 3 fix) |
| `GLOBAL_RULES.md` | Created |
| `openspec/changes/todo-app/midpoint-report.md` | Created, updated |

### Task 2.1 — Create Entity: Category
- Created `Models/Entities/Category.cs` with Id, Name, UserId, CreatedAt properties
- EF Core configuration sets table name, max length 50, unique index on (Name, UserId)

### Task 2.2 — Create Entity: Todo
- Created `Models/Entities/Todo.cs` with all 9 properties + navigation properties
- EF Core configuration sets 4 composite indexes on (UserId + CreatedAt/IsCompleted/CategoryId/DueDate)

### Task 2.3 — Create AppDbContext
- Created `Data/AppDbContext.cs` extending `IdentityDbContext<IdentityUser>`
- DbSets for Todos and Categories, applies entity configurations in OnModelCreating

### Task 2.4 — Create and apply initial EF Core migration
- Created `Data/Migrations/20260627093223_InitialCreate.cs` with all 9 tables
- Includes Identity tables, Categories, Todos with correct columns, indexes, FK constraints

### Task 3.1 — Create Auth DTOs
- Created `Models/DTOs/Auth/RegisterRequest.cs`, `LoginRequest.cs`, `AuthResponse.cs`
- RegisterRequest: Username with [StringLength(256, MinimumLength=1)], Password with [MinLength(6)]
- LoginRequest: Username and Password with [Required]
- AuthResponse: Token, Username, ExpiresAt

### Task 3.2 — Implement IAuthService and AuthService: Register
- Created `Services/Interfaces/IAuthService.cs` with RegisterAsync and LoginAsync
- Created `Services/AuthService.cs` — RegisterAsync uses UserManager, checks duplicates, throws ConflictException (409)
- Registered in DI via `AddScoped<IAuthService, AuthService>()`

### Task 3.3 — Implement AuthService: Login
- LoginAsync validates credentials via UserManager.FindByNameAsync + CheckPasswordAsync
- Throws UnauthorizedException (401) for invalid username or wrong password (same message — no info leakage)
- JWT generation extracted as `GenerateAuthResponse` helper with user ID/name claims, 24h expiry

### Task 3.4 — Create AuthController: Register endpoint
- Created `Controllers/AuthController.cs` with [Route("api/v1/auth")]
- POST `/api/v1/auth/register` returns 201 Created with FluentResponse.ApiWrapper envelope
- Validation errors handled by [ApiController] model validation -> 400
- Duplicate username -> ConflictException middleware -> 409

### Task 3.5 — Create AuthController: Login endpoint
- POST `/api/v1/auth/login` returns 200 OK with FluentResponse.ApiWrapper envelope
- Invalid credentials -> UnauthorizedException middleware -> 401

### Task 3.6 — Add user-scoped data access helper
- Created `Extensions/ClaimsPrincipalExtensions.cs` with GetUserId() extension method
- Extracts ClaimTypes.NameIdentifier from JWT claims, returns empty string if unauthenticated

### Task 4.1 — Create Category DTOs
- Created `Models/DTOs/Categories/CategoryRequest.cs` with Name (string, required, 1-50 chars via [StringLength(50, MinimumLength=1)])
- Created `Models/DTOs/Categories/CategoryResponse.cs` with Id, Name, TodoCount, CreatedAt
- Updated `MappingProfile.cs` with CreateMap<Category, CategoryResponse>, ignoring TodoCount (computed in service)

### Task 4.2 — Implement ICategoryService: Create and List
- Created `Services/Interfaces/ICategoryService.cs` with CreateAsync, GetAllAsync
- Created `Services/CategoryService.cs` — CreateAsync checks duplicate name per user (case-insensitive), throws ConflictException (409)
- GetAllAsync returns categories ordered alphabetically with TodoCount computed via GroupBy query
- Added `AddHttpContextAccessor()` and scoped `ICategoryService` registration in Program.cs

### Task 4.3 — Implement CategoryService: Update and Delete
- UpdateAsync checks ownership (404 if not found/not owned), checks duplicate name (409 if conflict)
- DeleteAsync checks for associated todos (409 if todos exist), removes category (204)
- Both throw NotFoundException (404) for missing or unowned categories

### Task 4.4 — Create CategoryController: Create and List
- Created `Controllers/CategoryController.cs` with [Authorize] and [Route("api/v1/categories")]
- POST `/api/v1/categories` returns 201 with ApiResponse<CategoryResponse>
- GET `/api/v1/categories` returns 200 with ApiResponse<CategoryResponse[]>

### Task 4.5 — Create CategoryController: Update and Delete
- PUT `/api/v1/categories/{id}` returns 200 with ApiResponse<CategoryResponse>
- DELETE `/api/v1/categories/{id}` returns 204 No Content
- Error responses via FluentResponse exception middleware: 404 (NotFoundException), 409 (ConflictException), 400 (model validation)
- Created `CategoryServiceTests.cs` with 11 tests covering all 12 spec scenarios
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
