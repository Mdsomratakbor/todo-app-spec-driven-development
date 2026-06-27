## Context

Building a full-stack Todo List Web Application from scratch using .NET 10 Web API, Angular 20, PostgreSQL, Entity Framework Core, Cartographer.Mapper (object mapping), and FluentResponse.ApiWrapper (standardized API responses). This is a greenfield project with no existing codebase. The application requires user authentication, todo management with categories, and search/filter capabilities.

## Goals / Non-Goals

**Goals:**
- Clean architecture with separation of concerns (API, business logic, data access)
- RESTful API design following standard conventions
- JWT-based authentication with secure password hashing
- Responsive Angular SPA with component-based architecture
- PostgreSQL database with EF Core code-first migrations
- Search and filter capabilities for todos by status, category, due date, and text

**Non-Goals:**
- Real-time updates (WebSocket/SignalR)
- Mobile-native applications
- Third-party OAuth providers (Google, Microsoft, etc.)
- Role-based authorization beyond user-scoped data
- Performance optimization at scale (caching, indexing design)

## System Architecture

```
┌─────────────────────────┐     ┌──────────────────────────────┐
│    Angular 20 SPA       │────▶│   .NET 10 Web API            │
│                         │     │                              │
│  ┌───────────────────┐  │     │  ┌────────────────────────┐  │
│  │ Auth Components   │  │     │  │ Controllers             │  │
│  │ (Login/Register)  │  │     │  │  AuthController         │  │
│  ├───────────────────┤  │     │  │  TodoController         │  │
│  │ Todo Components   │  │     │  │  CategoryController     │  │
│  │ (List/Form/Filter)│  │     │  └──────────┬─────────────┘  │
│  ├───────────────────┤  │     │             │                 │
│  │ Category Comp.    │  │     │  ┌──────────▼─────────────┐  │
│  │ (List/Form)       │  │     │  │ Services                │  │
│  ├───────────────────┤  │     │  │  AuthService            │  │
│  │ Shared            │  │     │  │  TodoService            │  │
│  │ (AuthGuard,       │  │     │  │  CategoryService        │  │
│  │  Interceptor,     │  │     │  └──────────┬─────────────┘  │
│  │  Models)          │  │     │             │                 │
│  └───────────────────┘  │     │  ┌──────────▼─────────────┐  │
│                         │     │  │ Data Layer              │  │
│                         │     │  │  AppDbContext            │  │
│                         │     │  │  Entity Configurations   │  │
│                         │     │  └──────────┬─────────────┘  │
└─────────────────────────┘     │             │                 │
                                │  ┌──────────▼─────────────┐  │
                                │  │ PostgreSQL Database     │  │
                                │  └────────────────────────┘  │
                                └──────────────────────────────┘
```

**Layers:**
- **Presentation Layer**: Angular 20 SPA — handles UI rendering and user interaction
- **API Layer**: .NET 10 Web API controllers — handles HTTP concerns, request validation, response formatting via FluentResponse.ApiWrapper
- **Service Layer**: Business logic — orchestrates operations, enforces rules, uses Cartographer.Mapper for entity-to-DTO mapping
- **Data Layer**: EF Core DbContext with PostgreSQL provider — manages persistence and queries

## Decisions

| Decision | Choice | Rationale | Alternatives Considered |
|---|---|---|---|
| Authentication | Custom JWT + ASP.NET Core Identity | Built-in Identity provides password hashing, lockout, and security best practices out of the box. JWT enables stateless API auth. | Cookie auth (not ideal for SPA-to-API); third-party auth (too complex for MVP) |
| API Architecture | Controller-based with service layer | Controllers handle HTTP concerns; services contain business logic. Keeps controllers thin and testable. | Minimal API (less structure for learning); MediatR/CQRS (overkill for simple CRUD) |
| Frontend State Management | Angular services with RxJS | Sufficient for this scale. No need for NgRx/state management libraries for a todo app. | NgRx (too much boilerplate for simple CRUD) |
| ORM Approach | Code-first EF Core | Full control over model design. Migrations track schema changes in version control. | Database-first (less suitable for new projects); Dapper (too low-level for learning) |
| Password Hashing | ASP.NET Core Identity PasswordHasher | Industry-standard bcrypt-style hashing, already integrated with Identity. | Manual bcrypt/Argon2 (reinventing the wheel) |
| Frontend Styling | CSS with Angular Material | Material Design provides professional look with minimal effort. | Bootstrap; Tailwind CSS; custom CSS |
| API Versioning | URL prefix (/api/v1/) | Simple, explicit, easy to route. | Header-based versioning (less visible); no versioning (future pain) |
| Object Mapping | Cartographer.Mapper | Type-safe, convention-based mapping between entities and DTOs. Keeps controllers clean. | AutoMapper (more complex API); manual mapping (verbose, error-prone) |
| API Response Format | FluentResponse.ApiWrapper | Consistent envelope format for all API responses (success/error, status codes, messages). Standardizes frontend error handling. | Manual response wrapping (inconsistent); Problem Details (RFC 7807 — different paradigm) |

## Folder Structure

```
TodoApp-OpenSpe/
│
├── TodoApi/                          # .NET 10 Web API project
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── TodoController.cs
│   │   └── CategoryController.cs
│   ├── Models/
│   │   ├── Entities/
│   │   │   ├── Todo.cs
│   │   │   └── Category.cs
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   │   ├── RegisterRequest.cs
│   │   │   │   ├── LoginRequest.cs
│   │   │   │   └── AuthResponse.cs
│   │   │   ├── Todos/
│   │   │   │   ├── TodoRequest.cs
│   │   │   │   ├── TodoResponse.cs
│   │   │   │   └── TodoFilterRequest.cs
│   │   │   └── Categories/
│   │   │       ├── CategoryRequest.cs
│   │   │       └── CategoryResponse.cs
│   │   └── Mappings/
│   │       └── MappingProfile.cs
│   ├── Services/
│   │   ├── Interfaces/
│   │   │   ├── IAuthService.cs
│   │   │   ├── ITodoService.cs
│   │   │   └── ICategoryService.cs
│   │   ├── AuthService.cs
│   │   ├── TodoService.cs
│   │   └── CategoryService.cs
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Configurations/
│   │       ├── TodoConfiguration.cs
│   │       └── CategoryConfiguration.cs
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── TodoApp/                          # Angular 20 project
│   ├── src/
│   │   ├── app/
│   │   │   ├── auth/
│   │   │   │   ├── login/
│   │   │   │   │   ├── login.component.ts
│   │   │   │   │   └── login.component.html
│   │   │   │   ├── register/
│   │   │   │   │   ├── register.component.ts
│   │   │   │   │   └── register.component.html
│   │   │   │   ├── auth.service.ts
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── auth.interceptor.ts
│   │   │   ├── todos/
│   │   │   │   ├── todo-list/
│   │   │   │   │   ├── todo-list.component.ts
│   │   │   │   │   └── todo-list.component.html
│   │   │   │   ├── todo-form/
│   │   │   │   │   ├── todo-form.component.ts
│   │   │   │   │   └── todo-form.component.html
│   │   │   │   ├── todo.service.ts
│   │   │   │   └── todo.models.ts
│   │   │   ├── categories/
│   │   │   │   ├── category-list/
│   │   │   │   │   ├── category-list.component.ts
│   │   │   │   │   └── category-list.component.html
│   │   │   │   ├── category-form/
│   │   │   │   │   ├── category-form.component.ts
│   │   │   │   │   └── category-form.component.html
│   │   │   │   ├── category.service.ts
│   │   │   │   └── category.models.ts
│   │   │   ├── shared/
│   │   │   │   ├── models/
│   │   │   │   │   └── api-response.ts
│   │   │   │   └── components/
│   │   │   ├── app.routes.ts
│   │   │   ├── app.component.ts
│   │   │   └── app.config.ts
│   │   ├── index.html
│   │   ├── main.ts
│   │   └── styles.scss
│   ├── angular.json
│   ├── package.json
│   └── tsconfig.json
│
├── openspec/                         # OpenSpec artifacts
│   ├── changes/todo-app/
│   └── specs/
│
└── README.md
```

## Database Design

### Tables

**AspNetUsers** (managed by ASP.NET Core Identity)
| Column | Type | Constraints |
|---|---|---|
| Id | TEXT (GUID) | PK |
| UserName | TEXT(256) | NOT NULL, UNIQUE |
| NormalizedUserName | TEXT(256) | UNIQUE INDEX |
| Email | TEXT(256) | NULLABLE |
| PasswordHash | TEXT | NOT NULL |
| SecurityStamp | TEXT | NOT NULL |
| ConcurrencyStamp | TEXT | NOT NULL |
| *Plus standard IdentityUser columns* | | |

**Categories**
| Column | Type | Constraints |
|---|---|---|
| Id | UUID | PK, DEFAULT gen_random_uuid() |
| Name | VARCHAR(50) | NOT NULL |
| UserId | TEXT | NOT NULL, FK → AspNetUsers.Id |
| CreatedAt | TIMESTAMPTZ | NOT NULL, DEFAULT NOW() |

**Unique constraint**: (Name, UserId)

**Todos**
| Column | Type | Constraints |
|---|---|---|
| Id | UUID | PK, DEFAULT gen_random_uuid() |
| Title | VARCHAR(200) | NOT NULL |
| Description | TEXT | NULLABLE |
| IsCompleted | BOOLEAN | NOT NULL, DEFAULT FALSE |
| DueDate | TIMESTAMPTZ | NULLABLE |
| CreatedAt | TIMESTAMPTZ | NOT NULL, DEFAULT NOW() |
| UpdatedAt | TIMESTAMPTZ | NOT NULL, DEFAULT NOW() |
| UserId | TEXT | NOT NULL, FK → AspNetUsers.Id |
| CategoryId | UUID | NULLABLE, FK → Categories.Id ON DELETE SET NULL |

### Indexes
- Todos: (UserId, CreatedAt DESC) — primary listing query
- Todos: (UserId, CategoryId) — filter by category
- Todos: (UserId, IsCompleted) — filter by status
- Todos: (UserId, DueDate) — filter by date range

## Entity Relationships

```
AspNetUsers ──1:N──▶ Todos
AspNetUsers ──1:N──▶ Categories
Categories  ──1:N──▶ Todos  (nullable, SET NULL on delete)
```

- A User has many Todos and many Categories.
- A Category belongs to one User and can be assigned to many Todos.
- A Todo optionally belongs to one Category. Deleting a Category sets CategoryId to NULL on associated Todos.
- All data is user-scoped: every query filters by UserId.

## API Endpoints

All endpoints are prefixed with `/api/v1`. Authenticated endpoints require `Authorization: Bearer <token>`.

### Authentication

| Method | Path | Auth | Request Body | Response |
|---|---|---|---|---|
| POST | /api/v1/auth/register | No | `{ username, password }` | `AuthResponse` with JWT |
| POST | /api/v1/auth/login | No | `{ username, password }` | `AuthResponse` with JWT |

**AuthResponse envelope** (via FluentResponse.ApiWrapper):
```
{
  "success": true,
  "data": {
    "token": "eyJ...",
    "username": "john",
    "expiresAt": "2026-06-28T12:00:00Z"
  },
  "message": null,
  "errors": null
}
```

### Categories

| Method | Path | Auth | Request Body | Response |
|---|---|---|---|---|
| GET | /api/v1/categories | Yes | — | `CategoryResponse[]` |
| POST | /api/v1/categories | Yes | `{ name }` | `CategoryResponse` |
| PUT | /api/v1/categories/{id} | Yes | `{ name }` | `CategoryResponse` |
| DELETE | /api/v1/categories/{id} | Yes | — | 204 No Content |

**CategoryResponse**:
```
{
  "id": "uuid",
  "name": "Work",
  "todoCount": 5,
  "createdAt": "2026-06-27T10:00:00Z"
}
```

### Todos

| Method | Path | Auth | Query Params | Request Body | Response |
|---|---|---|---|---|---|
| GET | /api/v1/todos | Yes | `?isCompleted=&categoryId=&dueBefore=&dueAfter=&search=` | — | `TodoResponse[]` |
| GET | /api/v1/todos/{id} | Yes | — | — | `TodoResponse` |
| POST | /api/v1/todos | Yes | — | `{ title, description?, dueDate?, categoryId? }` | `TodoResponse` |
| PUT | /api/v1/todos/{id} | Yes | — | `{ title, description?, dueDate?, isCompleted?, categoryId? }` | `TodoResponse` |
| DELETE | /api/v1/todos/{id} | Yes | — | — | 204 No Content |

**Filter query parameters** (all optional, combined with AND):
- `isCompleted`: boolean — filter by completion status
- `categoryId`: UUID — filter by category
- `dueBefore`: ISO date — todos due on or before this date
- `dueAfter`: ISO date — todos due on or after this date
- `search`: text — case-insensitive match on title or description

**TodoResponse**:
```
{
  "id": "uuid",
  "title": "Buy groceries",
  "description": "Milk, eggs, bread",
  "isCompleted": false,
  "dueDate": "2026-07-01T00:00:00Z",
  "categoryId": "uuid",
  "categoryName": "Personal",
  "createdAt": "2026-06-27T10:00:00Z",
  "updatedAt": "2026-06-27T10:00:00Z"
}
```

### Error Response Format

All errors use FluentResponse.ApiWrapper envelope:
```
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": {
    "title": ["Title is required"],
    "dueDate": ["Due date must be in the future"]
  }
}
```

HTTP status codes used: 200 (success), 201 (created), 204 (no content), 400 (validation), 401 (unauthorized), 404 (not found), 409 (conflict), 500 (server error).

## Angular Architecture

### Module Structure (standalone components)

The Angular 20 application uses standalone components (default in Angular 20) organized by feature.

```
AppComponent
├── AuthFeature
│   ├── LoginComponent        (route: /login)
│   └── RegisterComponent     (route: /register)
├── TodoFeature
│   ├── TodoListComponent     (route: /todos — default)
│   └── TodoFormComponent     (dialog, opened from list)
└── CategoryFeature
    ├── CategoryListComponent (route: /categories)
    └── CategoryFormComponent (dialog, opened from list)
```

### Route Configuration

| Path | Component | Auth Guard |
|---|---|---|
| /login | LoginComponent | No |
| /register | RegisterComponent | No |
| /todos | TodoListComponent | Yes |
| /categories | CategoryListComponent | Yes |
| / (default) | Redirects to /todos | — |
| ** | Redirects to /todos | — |

### Services

- **AuthService**: Handles login/register HTTP calls, stores JWT in localStorage, exposes `isLoggedIn$` observable
- **TodoService**: CRUD + filter HTTP calls, returns typed observables
- **CategoryService**: CRUD HTTP calls for categories

### Interceptors

- **AuthInterceptor**: Attaches JWT token from localStorage to all outgoing HTTP requests
- **(Optional) ErrorInterceptor**: Catches HTTP errors and transforms them for UI display

## Component Diagram

```
                        AppComponent
                      ┌──────────────┐
                      │  Toolbar     │
                      │  (nav links) │
                      └──────┬───────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
       ┌──────▼──────┐ ┌────▼─────┐ ┌──────▼──────┐
       │ AuthFeature │ │TodoFeature│ │CategoryFeat.│
       │             │ │          │ │             │
       │ Login       │ │TodoList  │ │CategoryList │
       │ Register    │ │ └►TodoForm│ │ └►Category  │
       │             │ │ (dialog) │ │   Form      │
       └─────────────┘ └──────────┘ │   (dialog)  │
                                    └─────────────┘

Data Flow:
  Component ──calls──▶ Service ──HTTP──▶ API ──▶ Database
                                  ◀──JSON──
       ▲                                    │
       └────────── Observable ──────────────┘
```

## Error Handling

### Backend

- **Global Exception Middleware** (`Middleware/ExceptionMiddleware.cs`): Catches all unhandled exceptions, logs them, and returns a standardized FluentResponse.ApiWrapper error envelope with status 500.
- **FluentResponse.ApiWrapper**: All controller actions return wrapped responses via the library's fluent API. Success responses include data payload. Validation errors include field-level error details. Business rule violations return appropriate status codes (404, 409) with descriptive messages.
- **Model State Validation**: ASP.NET Core's built-in `[ApiController]` attribute automatically validates request DTOs and returns 400 with field-level errors before controller code executes.

### Frontend

- **AuthInterceptor**: On 401 responses, clears stored token and redirects to login.
- **Component-level error handling**: Services catch errors via RxJS `catchError`, surface user-friendly messages via Angular Material snackbar or inline form errors.
- **API response envelope parsing**: All services deserialize the FluentResponse.ApiWrapper envelope and extract the `data` field or throw with the `message`/`errors` content.

## Logging

### Backend

- **Provider**: `Microsoft.Extensions.Logging` with Console provider (development)
- **Log Levels**:
  - Controllers/Services: `Information` for key operations (user registered, todo created/deleted, auth failure)
  - EF Core: `Warning` for slow queries, `Information` for executed SQL (development only)
  - Exception Middleware: `Error` for unhandled exceptions
- **Structured logging**: Use structured message templates (`LogInformation("User {UserId} created todo {TodoId}", userId, todoId)`) for future integration with log aggregation tools.

### Frontend

- **Console logging**: `console.log`/`console.error` for service calls during development.
- **No production-grade logging** (out of scope). If needed, add Angular's `Logger` service or integrate with a service like Sentry.

## Validation Strategy

### Backend

- **DTO-level**: Data annotations on request DTOs (`[Required]`, `[StringLength(200)]`, `[MinLength(1)]`). Enforced automatically by `[ApiController]` model validation.
- **Service-level**: Business rule validation (duplicate category name, todo existence, category delete with active todos). Throws domain exceptions mapped to appropriate HTTP status codes by the controller.
- **Entity-level**: EF Core configurations enforce column constraints (max length, required, indexes) at the database level.

### Frontend

- **Template-driven forms** with Angular Material validation directives: `required`, `minlength`, `maxlength`.
- **Custom validators** for cross-field rules (e.g., due date must be in the future).
- **Server error display**: Field-level validation errors from the API are mapped back to form controls for inline display.

## Testing Strategy

### Backend

- **Unit Tests**: xUnit project (`TodoApi.Tests/`) testing service layer in isolation. Services are tested with mocked DbContext (using EF Core InMemory provider).
  - AuthService: registration, login, duplicate username, invalid credentials
  - TodoService: CRUD operations, filter scenarios, data isolation (user A cannot see user B's todos)
  - CategoryService: CRUD operations, duplicate name, delete with/without associated todos
- **Integration Tests**: Optional — test controllers via `WebApplicationFactory<Program>` with a test PostgreSQL database. Verify full request/response pipeline including authentication and middleware.

### Frontend

- **Unit Tests**: Jasmine + Karma for component and service tests.
  - AuthService: login/register HTTP calls, token storage
  - TodoService: CRUD + filter HTTP calls
  - AuthGuard: redirect behavior for authenticated vs unauthenticated states
- This is a learning project; test coverage focuses on critical business logic rather than UI rendering.

### Test Data

- Factories/seeds for creating test entities in both backend (EF Core seed data) and frontend (mock HTTP interceptors with `HttpClientTestingController`).

## Risks / Trade-offs

- **No refresh token mechanism** → JWT is valid for 24 hours. If compromised, token cannot be revoked. Acceptable for a learning project.
- **No Swagger/OpenAPI** → Frontend-backend contract must be kept in sync manually. The API design section above serves as the contract reference.
- **EF Core InMemory provider for unit tests** → Does not enforce relational constraints. Mitigation: supplement with a small set of integration tests against real PostgreSQL.
- **Angular standalone components** → Angular 20 defaults to standalone. Simpler setup but team may be unfamiliar with the pattern.
- **Cartographer.Mapper / FluentResponse.ApiWrapper** → Third-party NuGet packages with potentially limited community adoption. Mitigation: evaluate packages during setup phase; fall back to AutoMapper/manual mapping and custom response wrapping if issues arise.
