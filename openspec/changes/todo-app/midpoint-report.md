# Mid-Point Report — todo-app

**Date:** 2026-06-27 (updated — Gate 2 re-check after Phases 1-7)
**Change:** todo-app
**Branch:** change/frontend-setup (based on develop)

## Progress Summary

| Metric | Value |
|---|---|
| Tasks completed | 41 / 52 |
| Tasks remaining | 11 |
| Completion | 78.8% |

## What Was Implemented

### Phase 1 — Backend Solution Setup (Tasks 1.1-1.6) ✅
- .NET 10 solution `TodoApp.slnx` + `TodoApi.csproj`
- 6 NuGet packages: EF Core, PostgreSQL, JWT Bearer, Identity, Cartographer.Mapper 0.2.0, FluentResponse.ApiWrapper 1.0.0
- JWT authentication configured (issuer, audience, signing key, 24h expiry)
- CORS policy `AllowAngularDev` for `http://localhost:4200`
- Cartographer.Mapper mapping profile registered
- FluentResponse.ApiWrapper middleware (exception handler, correlation ID)

### Phase 2 — Data Layer (Tasks 2.1-2.4) ✅
- `Category` entity with unique index on (Name, UserId), max length 50
- `Todo` entity with 4 composite indexes (UserId + CreatedAt/IsCompleted/CategoryId/DueDate)
- `AppDbContext` extending `IdentityDbContext<IdentityUser>`
- EF Core migration with 9 tables (Identity + Categories + Todos)

### Phase 3 — Auth API (Tasks 3.1-3.6) ✅
- DTOs: RegisterRequest, LoginRequest, AuthResponse (with data annotations)
- IAuthService + AuthService: RegisterAsync (ConflictException 409 on duplicate), LoginAsync (UnauthorizedException 401), JWT generation with user ID/name claims
- AuthController: POST `/api/v1/auth/register` (201), `/api/v1/auth/login` (200)
- `ClaimsPrincipalExtensions.GetUserId()` helper

### Phase 4 — Categories API (Tasks 4.1-4.5) ✅
- DTOs: CategoryRequest (Name required, 1-50 chars), CategoryResponse (Id, Name, TodoCount, CreatedAt)
- ICategoryService + CategoryService: full CRUD with duplicate check per user, cascade protection, TodoCount computed
- CategoryController: POST/GET/PUT/DELETE `/api/v1/categories`

### Phase 5 — Todos API (Tasks 5.1-5.7) ✅
- DTOs: TodoRequest, TodoResponse, TodoFilterRequest, TodoUpdateRequest
- ITodoService + TodoService: full CRUD with AND filter logic (5 dimensions), partial updates, Guid.Empty sentinel for category removal
- TodoController: POST/GET `/api/v1/todos`, GET/PUT/DELETE `/api/v1/todos/{id}`
- 67 tests (AuthService 6, CategoryService 11, TodoService 16, validation 9, ClaimsPrincipal 3, entities 8, DbContext 5, mappings 2) — all passing; 46/47 spec scenarios covered

### Phase 6 — Frontend Setup (Tasks 6.1-6.5) ✅
- Angular 21 project (`ng new TodoApp --routing --style=scss --standalone`)
- Angular Material 21.2.14 with M3 theme (azure/blue)
- Shared models: `ApiResponse<T>`, `AuthResponse`, `ApiConfig` (base URL `http://localhost:5000/api/v1`)
- Auth interceptor (attaches JWT, 401 → redirect to login)
- Route config + AuthGuard: `/login`, `/register`, `/todos` (guarded), `/categories` (guarded), `/` → `/todos`

### Phase 7 — Authentication UI (Tasks 7.1-7.4) ✅
- AuthService: login/register HTTP calls with token storage in localStorage
- LoginComponent: reactive form, username/password fields, 401→"Invalid credentials", snackbar on network error
- RegisterComponent: reactive form with password match validator, MinLength(6), 409→"Username already taken", 400→field-level errors

## Additional Changes
- Swagger/OpenAPI: installed `Swashbuckle.AspNetCore` 10.2.3, configured `AddSwaggerGen()` + `UseSwagger()`/`UseSwaggerUI()`
- Backend port changed from 5141 → 5000 to match frontend ApiConfig
- `.gitkeep` added to `TodoApi/Middleware/` to track empty directory
- Default Angular welcome template replaced with clean `<router-outlet />`
- Frontend branch `change/frontend-setup` created from `develop`

## Deviations from Spec

- **None.** All implementation follows the approved design.md, tasks.md, and spec files.
- Swagger config was not in original spec — added at user request for API testing.

## Remaining Work

### Phase 8 — Categories UI (4 tasks) ✅
- CategoryService, CategoryListComponent, CategoryFormComponent, delete confirmation dialog — all done

### Phase 9 — Todo UI (6 tasks)
- TodoService, TodoListComponent, TodoFormComponent/dialog, completion toggle, filter bar

### Phase 10 — Validation (5 tasks)
- Validation tests for auth UI, categories UI, todos UI, filters, spec coverage verification

## Known Issues
- None at this point.
