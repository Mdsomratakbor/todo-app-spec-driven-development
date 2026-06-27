## 1. Backend Solution Setup

### Task 1.1 — Create .NET solution and Web API project [x]
- **Requirement**: N/A (infrastructure)
- **Dependencies**: None
- **Definition of Done**: Solution file `TodoApp.sln` and Web API project `TodoApi.csproj` exist. Project runs with default template.
- **Deliverables**: `TodoApp.sln`, `TodoApi/TodoApi.csproj`, default `Program.cs`

### Task 1.2 — Add NuGet packages [x]
- **Requirement**: N/A (infrastructure)
- **Dependencies**: 1.1
- **Definition of Done**: All packages restored successfully. `.csproj` includes: `Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Cartographer.Mapper`, `FluentResponse.ApiWrapper`.
- **Deliverables**: Updated `TodoApi.csproj`

### Task 1.3 — Configure JWT authentication in Program.cs [x]
- **Requirement**: REQ-AUTH-003 (User can access their data)
- **Dependencies**: 1.2
- **Definition of Done**: `Program.cs` contains JWT bearer token configuration with token validation parameters (issuer, audience, signing key from config). `Authentication` and `Authorization` middleware added to the pipeline.
- **Deliverables**: Updated `Program.cs`, JWT section in `appsettings.json`

### Task 1.4 — Configure CORS and controller routing [x]
- **Requirement**: REQ-AUTH-003
- **Dependencies**: 1.3
- **Definition of Done**: CORS configured to allow Angular dev server (http://localhost:4200). `AddControllers()` registered. `MapControllers()` added to pipeline. Application compiles and starts without errors.
- **Deliverables**: Updated `Program.cs`

### Task 1.5 — Create Cartographer.Mapper mapping profile [x]
- **Requirement**: N/A (infrastructure)
- **Dependencies**: 1.2
- **Definition of Done**: `MappingProfile.cs` created with entity-to-DTO mappings for Todo/Category. Profile registered in DI. Mapper can be injected into services.
- **Deliverables**: `Models/Mappings/MappingProfile.cs`

### Task 1.6 — Configure FluentResponse.ApiWrapper [x]
- **Requirement**: REQ-TODO-001, REQ-TODO-003, REQ-TODO-004, REQ-CAT-001, REQ-CAT-003 (error responses)
- **Dependencies**: 1.2
- **Definition of Done**: FluentResponse.ApiWrapper middleware/service registered. Controllers return wrapped responses in consistent envelope format. `IAppResponse` or equivalent is available for DI.
- **Deliverables**: Updated `Program.cs`

## 2. Data Layer

### Task 2.1 — Create Entity: Category [x]
- **Requirement**: REQ-CAT-001, REQ-CAT-002, REQ-CAT-003, REQ-CAT-004
- **Dependencies**: 1.2
- **Definition of Done**: `Category` entity class exists with properties: `Id` (Guid), `Name` (string, max 50), `UserId` (string), `CreatedAt` (DateTime). Entity configuration sets table name, max length, index on (Name, UserId).
- **Deliverables**: `Models/Entities/Category.cs`

### Task 2.2 — Create Entity: Todo [x]
- **Requirement**: REQ-TODO-001, REQ-TODO-002, REQ-TODO-003, REQ-TODO-004, REQ-TODO-005
- **Dependencies**: 2.1
- **Definition of Done**: `Todo` entity class exists with properties: `Id` (Guid), `Title` (string, max 200), `Description` (string?, max 2000), `IsCompleted` (bool, default false), `DueDate` (DateTime?), `CreatedAt` (DateTime), `UpdatedAt` (DateTime), `UserId` (string), `CategoryId` (Guid?). Navigation properties to `Category` and `User`. Entity configuration sets indexes on (UserId, CreatedAt), (UserId, IsCompleted), (UserId, CategoryId), (UserId, DueDate).
- **Deliverables**: `Models/Entities/Todo.cs`

### Task 2.3 — Create AppDbContext [x]
- **Requirement**: REQ-AUTH-003 (data isolation), REQ-CAT-001, REQ-TODO-001
- **Dependencies**: 2.1, 2.2
- **Definition of Done**: `AppDbContext` extends `IdentityDbContext<IdentityUser>`. DbSets for `Todos` and `Categories`. `OnModelCreating` applies entity configurations. Connection string uses PostgreSQL via Npgsql.
- **Deliverables**: `Data/AppDbContext.cs`

### Task 2.4 — Create and apply initial EF Core migration [x]
- **Requirement**: REQ-CAT-001, REQ-TODO-001
- **Dependencies**: 2.3
- **Definition of Done**: `dotnet ef migrations add InitialCreate` runs successfully. `dotnet ef database update` creates tables in PostgreSQL. Database contains `AspNetUsers`, `Categories`, `Todos` tables with correct columns, keys, and indexes.
- **Deliverables**: `Data/Migrations/<timestamp>_InitialCreate.cs`

## 3. Backend: Authentication API

### Task 3.1 — Create Auth DTOs [x]
- **Requirement**: REQ-AUTH-001, REQ-AUTH-002
- **Dependencies**: 1.5
- **Definition of Done**: `RegisterRequest` (Username, Password) with `[Required]`, `[StringLength]`, `[MinLength(6)]` on Password. `LoginRequest` (Username, Password) with `[Required]`. `AuthResponse` (Token, Username, ExpiresAt). DTOs in `Models/DTOs/Auth/` folder.
- **Deliverables**: `Models/DTOs/Auth/RegisterRequest.cs`, `LoginRequest.cs`, `AuthResponse.cs`

### Task 3.2 — Implement IAuthService and AuthService: Register [x]
- **Requirement**: REQ-AUTH-001 (register, duplicate username, invalid password)
- **Dependencies**: 2.3, 3.1
- **Definition of Done**: `IAuthService` defines `RegisterAsync(RegisterRequest)` returning `AuthResponse`. `AuthService` uses `UserManager<IdentityUser>` to create user. Returns JWT on success. Throws appropriate exceptions for duplicate username (409) and invalid password (400).
- **Deliverables**: `Services/Interfaces/IAuthService.cs`, `Services/AuthService.cs`

### Task 3.3 — Implement AuthService: Login [x]
- **Requirement**: REQ-AUTH-002 (successful login, invalid credentials)
- **Dependencies**: 3.2
- **Definition of Done**: `LoginAsync(LoginRequest)` method checks credentials via `UserManager.FindByNameAsync` and `CheckPasswordAsync`. Generates JWT with user ID claim, username claim, 24-hour expiry. Returns 401 for invalid credentials. JWT generation extracted as reusable helper.
- **Deliverables**: Updated `Services/AuthService.cs`

### Task 3.4 — Create AuthController: Register endpoint [x]
- **Requirement**: REQ-AUTH-001 (all 3 scenarios)
- **Dependencies**: 3.2, 1.6
- **Definition of Done**: POST `/api/v1/auth/register` accepts `RegisterRequest`, calls `AuthService.RegisterAsync`, returns wrapped `AuthResponse` with 201 status. Returns 409 for duplicate username. Returns 400 for validation errors (FluentResponse.ApiWrapper format).
- **Deliverables**: `Controllers/AuthController.cs`

### Task 3.5 — Create AuthController: Login endpoint [x]
- **Requirement**: REQ-AUTH-002 (both scenarios)
- **Dependencies**: 3.3, 3.4
- **Definition of Done**: POST `/api/v1/auth/login` accepts `LoginRequest`, calls `AuthService.LoginAsync`, returns wrapped `AuthResponse`. Returns 401 for invalid credentials. Uses same response envelope as register.
- **Deliverables**: Updated `Controllers/AuthController.cs`

### Task 3.6 — Add user-scoped data access helper [x]
- **Requirement**: REQ-AUTH-003 (data isolation, unauthenticated request)
- **Dependencies**: 3.5
- **Definition of Done**: Extension method `GetUserId()` on `ClaimsPrincipal` extracts user ID from JWT claim. Returns `string`. All future service methods use this to scope queries. Returns empty string if unauthenticated.
- **Deliverables**: `Extensions/ClaimsPrincipalExtensions.cs`

## 4. Backend: Categories API

### Task 4.1 — Create Category DTOs [x]
- **Requirement**: REQ-CAT-001, REQ-CAT-002, REQ-CAT-003
- **Dependencies**: 2.1, 1.5
- **Definition of Done**: `CategoryRequest` with `Name` (string, required, max 50). `CategoryResponse` with `Id`, `Name`, `TodoCount`, `CreatedAt`. Cartographer.Mapper mapping added in `MappingProfile`.
- **Deliverables**: `Models/DTOs/Categories/CategoryRequest.cs`, `CategoryResponse.cs`, updated `MappingProfile.cs`

### Task 4.2 — Implement ICategoryService: Create and List [x]
- **Requirement**: REQ-CAT-001 (create with name, duplicate name, missing name), REQ-CAT-002 (list all)
- **Dependencies**: 2.3, 4.1, 3.6
- **Definition of Done**: `ICategoryService` defines `CreateAsync(CategoryRequest)` and `GetAllAsync()`. `CategoryService` scopes queries to current user. `CreateAsync` checks for duplicate name per user (409). Returns created `CategoryResponse`.
- **Deliverables**: `Services/Interfaces/ICategoryService.cs`, `Services/CategoryService.cs`

### Task 4.3 — Implement CategoryService: Update and Delete [x]
- **Requirement**: REQ-CAT-003 (update name), REQ-CAT-004 (delete with/without todos, delete non-existent)
- **Dependencies**: 4.2
- **Definition of Done**: `UpdateAsync(Guid id, CategoryRequest)` updates name, returns updated response. Returns 404 if not found or not owned by user. `DeleteAsync(Guid id)` checks for associated todos — returns 409 if todos exist, 204 if deleted. Returns 404 if not found.
- **Deliverables**: Updated `Services/CategoryService.cs`

### Task 4.4 — Create CategoryController: Create and List [x]
- **Requirement**: REQ-CAT-001 (scenario: create, duplicate, missing name), REQ-CAT-002 (scenario: list all)
- **Dependencies**: 4.2, 1.6
- **Definition of Done**: POST `/api/v1/categories` returns wrapped `CategoryResponse` (201). GET `/api/v1/categories` returns wrapped `CategoryResponse[]` (200). Controllers use `[Authorize]` attribute. User ID extracted via claims helper.
- **Deliverables**: `Controllers/CategoryController.cs`

### Task 4.5 — Create CategoryController: Update and Delete [x]
- **Requirement**: REQ-CAT-003 (scenario: update name), REQ-CAT-004 (all 3 delete scenarios)
- **Dependencies**: 4.3, 4.4
- **Definition of Done**: PUT `/api/v1/categories/{id}` returns wrapped `CategoryResponse`. DELETE `/api/v1/categories/{id}` returns 204. Returns 404 for not found, 409 for category with todos. Error responses use FluentResponse.ApiWrapper format.
- **Deliverables**: Updated `Controllers/CategoryController.cs`

## 5. Backend: Todos API

### Task 5.1 — Create Todo DTOs [x]
- **Requirement**: REQ-TODO-001, REQ-TODO-002, REQ-TODO-003, REQ-TODO-004
- **Dependencies**: 2.2, 1.5
- **Definition of Done**: `TodoRequest` (Title required, max 200; Description optional; DueDate optional DateTime; CategoryId optional Guid). `TodoResponse` (Id, Title, Description, IsCompleted, DueDate, CategoryId, CategoryName, CreatedAt, UpdatedAt). `TodoFilterRequest` (IsCompleted? bool, CategoryId? Guid, DueBefore? DateTime, DueAfter? DateTime, Search? string). Cartographer.Mapper mappings added.
- **Deliverables**: `Models/DTOs/Todos/TodoRequest.cs`, `TodoResponse.cs`, `TodoFilterRequest.cs`, updated `MappingProfile.cs`

### Task 5.2 — Implement ITodoService: Create [x]
- **Requirement**: REQ-TODO-001 (create with required fields, create with all fields, create without title)
- **Dependencies**: 2.3, 5.1, 3.6
- **Definition of Done**: `ITodoService` defines `CreateAsync(TodoRequest)`. `CreateAsync` sets `IsCompleted = false`, `CreatedAt = DateTime.UtcNow`. If `CategoryId` provided, verifies category exists and belongs to user (404 if not). Returns `TodoResponse` with generated `Id`.
- **Deliverables**: `Services/Interfaces/ITodoService.cs`, `Services/TodoService.cs`

### Task 5.3 — Implement TodoService: GetById and List (all) [x]
- **Requirement**: REQ-TODO-003 (get existing, get non-existent, get another user's), REQ-TODO-002 (list all)
- **Dependencies**: 5.2
- **Definition of Done**: `GetByIdAsync(Guid id)` returns `TodoResponse` or throws 404 (hides existence from other users). `GetAllAsync()` returns all user's todos ordered by `CreatedAt` descending. Both methods filter by current user ID.
- **Deliverables**: Updated `Services/TodoService.cs`

### Task 5.4 — Implement TodoService: Update and Delete [x]
- **Requirement**: REQ-TODO-004 (update fields, update non-existent), REQ-TODO-005 (delete existing, delete non-existent)
- **Dependencies**: 5.3
- **Definition of Done**: `UpdateAsync(Guid id, TodoRequest)` updates all provided fields, sets `UpdatedAt`, returns updated response. Returns 404 if not found or not owned by user. `DeleteAsync(Guid id)` removes todo, returns 204. Returns 404 if not found.
- **Deliverables**: Updated `Services/TodoService.cs`

### Task 5.5 — Implement TodoService: Filter and search [x]
- **Requirement**: REQ-TODO-002 (filter by status, category, date range, search text, combined filters)
- **Dependencies**: 5.3
- **Definition of Done**: `GetAllAsync(TodoFilterRequest)` applies filters as AND conditions: `IsCompleted` boolean match, `CategoryId` exact match, `DueBefore`/`DueAfter` date range, `Search` case-insensitive match on Title or Description. Returns filtered `TodoResponse[]`. All filters are optional — omitted filters are not applied.
- **Deliverables**: Updated `Services/TodoService.cs`

### Task 5.6 — Create TodoController: Create and List [x]
- **Requirement**: REQ-TODO-001 (all create scenarios), REQ-TODO-002 (all filter scenarios)
- **Dependencies**: 5.2, 5.5, 1.6
- **Definition of Done**: POST `/api/v1/todos` returns wrapped `TodoResponse` (201). GET `/api/v1/todos` accepts query parameters mapped to `TodoFilterRequest`, returns wrapped `TodoResponse[]` (200). Returns 400 for validation errors. Controllers use `[Authorize]`.
- **Deliverables**: `Controllers/TodoController.cs`

### Task 5.7 — Create TodoController: GetById, Update, Delete [x]
- **Requirement**: REQ-TODO-003 (all 3 scenarios), REQ-TODO-004 (both scenarios), REQ-TODO-005 (both scenarios)
- **Dependencies**: 5.3, 5.4, 5.6
- **Definition of Done**: GET `/api/v1/todos/{id}` returns wrapped `TodoResponse`. PUT `/api/v1/todos/{id}` returns wrapped updated `TodoResponse`. DELETE `/api/v1/todos/{id}` returns 204. All return 404 for not found. Error responses use FluentResponse.ApiWrapper format.
- **Deliverables**: Updated `Controllers/TodoController.cs`

## 6. Frontend: Angular Project Setup

### Task 6.1 — Create Angular project with routing [x]
- **Requirement**: N/A (infrastructure)
- **Dependencies**: None
- **Definition of Done**: `ng new TodoApp --routing` creates project. `ng serve` starts without errors. Basic `AppComponent` renders. Angular 20 configured.
- **Deliverables**: `TodoApp/` project directory, `package.json`, `angular.json`, `src/main.ts`, `src/index.html`

### Task 6.2 — Add Angular Material and configure theme [x]
- **Requirement**: N/A (infrastructure)
- **Dependencies**: 6.1
- **Definition of Done**: `ng add @angular/material` completes. Theme configured (indigo-pink). `MatToolbarModule`, `MatTableModule`, `MatDialogModule`, `MatFormFieldModule`, `MatInputModule`, `MatButtonModule`, `MatIconModule`, `MatSelectModule`, `MatDatepickerModule`, `MatNativeDateModule`, `MatSnackBarModule`, `MatCheckboxModule` imported. Global styles in `styles.scss`.
- **Deliverables**: Updated `package.json`, `src/styles.scss`, `src/app/app.config.ts`

### Task 6.3 — Create shared API response model and AppConfig [x]
- **Requirement**: REQ-AUTH-003, REQ-TODO-001, REQ-CAT-001 (API response envelope)
- **Dependencies**: 6.1
- **Definition of Done**: `ApiResponse<T>` interface with `success`, `data`, `message`, `errors`. `ApiConfig` constant with base URL (`http://localhost:5000/api/v1`).
- **Deliverables**: `src/app/shared/models/api-response.ts`

### Task 6.4 — Set up HttpClient and AuthInterceptor [x]
- **Requirement**: REQ-AUTH-003 (authenticated request, unauthenticated request)
- **Dependencies**: 6.1, 6.3
- **Definition of Done**: `provideHttpClient(withInterceptors(...))` configured in `app.config.ts`. `AuthInterceptor` (functional interceptor) reads JWT from localStorage, attaches `Authorization: Bearer <token>` header to all requests. On 401 response, clears token and redirects to `/login`.
- **Deliverables**: `src/app/auth/auth.interceptor.ts`, updated `app.config.ts`

### Task 6.5 — Configure routes and AuthGuard [x]
- **Requirement**: REQ-AUTH-003 (unauthenticated request redirect)
- **Dependencies**: 6.4
- **Definition of Done**: Route config: `/login` → LoginComponent, `/register` → RegisterComponent, `/todos` → TodoListComponent (canActivate: AuthGuard), `/categories` → CategoryListComponent (canActivate: AuthGuard), `/` → redirect to `/todos`, `**` → redirect to `/todos`. `AuthGuard` returns `true` if token exists, else redirect to `/login`.
- **Deliverables**: `src/app/app.routes.ts`, `src/app/auth/auth.guard.ts`

## 7. Frontend: Authentication UI

### Task 7.1 — Create AuthService with login and register [x]
- **Requirement**: REQ-AUTH-001, REQ-AUTH-002
- **Dependencies**: 6.4
- **Definition of Done**: `AuthService` injectable with `login(username, password): Observable<AuthResponse>`, `register(username, password): Observable<AuthResponse>`. POSTs to `/auth/login` and `/auth/register`. On success, stores token in `localStorage`. Exposes `isLoggedIn(): boolean` and `getToken(): string | null`.
- **Deliverables**: `src/app/auth/auth.service.ts`

### Task 7.2 — Create LoginComponent — template [x]
- **Requirement**: REQ-AUTH-002 (successful login, invalid credentials)
- **Dependencies**: 7.1, 6.5, 6.2
- **Definition of Done**: Login form with Material fields for username and password. Submit button. Error message area. Router link to register page. Reactive form with `Validators.required`.
- **Deliverables**: `src/app/auth/login/login.component.html`, `login.component.scss`

### Task 7.3 — Create LoginComponent — logic [x]
- **Requirement**: REQ-AUTH-002 (both scenarios)
- **Dependencies**: 7.2
- **Definition of Done**: `onSubmit()` calls `AuthService.login()`. On success, navigates to `/todos`. On 401 error, displays "Invalid credentials" message. On network error, displays generic error via MatSnackBar. Form disabled during HTTP call.
- **Deliverables**: `src/app/auth/login/login.component.ts`

### Task 7.4 — Create RegisterComponent [x]
- **Requirement**: REQ-AUTH-001 (successful registration, duplicate username, invalid password)
- **Dependencies**: 7.1, 6.5, 6.2
- **Definition of Done**: Register form with username, password, confirm password fields. Password match validator. `[MinLength(6)]` on password. `onSubmit()` calls `AuthService.register()`. On success, navigates to `/todos`. On 409, shows "Username already taken". On 400, shows validation errors. On network error, generic error via snackbar.
- **Deliverables**: `src/app/auth/register/register.component.ts`, `register.component.html`, `register.component.scss`

## 8. Frontend: Categories UI

### Task 8.1 — Create CategoryService [x]
- **Requirement**: REQ-CAT-001, REQ-CAT-002, REQ-CAT-003, REQ-CAT-004
- **Dependencies**: 6.4
- **Definition of Done**: `CategoryService` injectable with `getAll(): Observable<Category[]>`, `create(name): Observable<Category>`, `update(id, name): Observable<Category>`, `delete(id): Observable<void>`. `Category` interface with `id`, `name`, `todoCount`, `createdAt`. Methods use HttpClient with correct HTTP verbs.
- **Deliverables**: `src/app/categories/category.service.ts`, `src/app/categories/category.models.ts`

### Task 8.2 — Create CategoryListComponent [x]
- **Requirement**: REQ-CAT-002 (list all categories)
- **Dependencies**: 8.1, 6.2
- **Definition of Done**: Material table listing all categories with columns: Name, Todo Count, Created Date, Actions (Edit, Delete). Calls `CategoryService.getAll()` on init. Empty state message when no categories. Loading indicator during fetch.
- **Deliverables**: `src/app/categories/category-list/category-list.component.ts`, `category-list.component.html`, `category-list.component.scss`

### Task 8.3 — Create CategoryFormComponent (dialog) [x]
- **Requirement**: REQ-CAT-001 (create with name, duplicate name, missing name), REQ-CAT-003 (update name)
- **Dependencies**: 8.1, 6.2
- **Definition of Done**: Material dialog with name field. `[Validators.required, Validators.maxLength(50)]`. Supports both add and edit modes (receives optional category data via `MAT_DIALOG_DATA`). On submit, calls create or update. Displays server errors on conflict. Returns created/updated category on close.
- **Deliverables**: `src/app/categories/category-form/category-form.component.ts`, `category-form.component.html`, `category-form.component.scss`

### Task 8.4 — Add delete confirmation with todo warning [x]
- **Requirement**: REQ-CAT-004 (delete with/without todos)
- **Dependencies**: 8.2, 8.3
- **Definition of Done**: Delete button opens confirmation dialog. If category has `todoCount > 0`, dialog warns "This category has N todos. Deleting it will remove the category from those todos." On confirm, calls `CategoryService.delete()`. Shows success snackbar. On 409, shows error snackbar. Refreshes list after delete.
- **Deliverables**: Updated `category-list.component.ts`, `category-list.component.html`

## 9. Frontend: Todo List UI

### Task 9.1 — Create TodoService [x]
- **Requirement**: REQ-TODO-001, REQ-TODO-002, REQ-TODO-003, REQ-TODO-004, REQ-TODO-005
- **Dependencies**: 6.4
- **Definition of Done**: `TodoService` injectable with `getAll(filter?): Observable<Todo[]>`, `getById(id): Observable<Todo>`, `create(request): Observable<Todo>`, `update(id, request): Observable<Todo>`, `delete(id): Observable<void>`. `Todo` interface with all response fields. `TodoFilter` interface mirroring backend filter params. Methods construct query params from filter object.
- **Deliverables**: `src/app/todos/todo.service.ts`, `src/app/todos/todo.models.ts`

### Task 9.2 — Create TodoListComponent — template [x]
- **Requirement**: REQ-TODO-002 (list all)
- **Dependencies**: 9.1, 6.2
- **Definition of Done**: Material table with columns: checkbox (completion), Title, Category, Due Date, Created, Actions (Edit, Delete). Filter toolbar above table with: search input, status toggle (All/Pending/Completed), category dropdown, date range picker. Empty state when no todos. Loading spinner.
- **Deliverables**: `src/app/todos/todo-list/todo-list.component.html`, `todo-list.component.scss`

### Task 9.3 — Create TodoListComponent — logic [x]
- **Requirement**: REQ-TODO-002 (all filter scenarios including combined), REQ-TODO-003 (display details)
- **Dependencies**: 9.2
- **Definition of Done**: `ngOnInit` loads todos via `TodoService.getAll()`. Filter changes trigger reload with combined filter params. Debounce search input (300ms). Clicking a row navigates or expands detail. Category column displays category name. Due date formatted nicely. Overdue items visually distinct.
- **Deliverables**: `src/app/todos/todo-list/todo-list.component.ts`

### Task 9.4 — Create TodoFormComponent — template [x]
- **Requirement**: REQ-TODO-001 (create with/without optional fields), REQ-TODO-004 (update fields)
- **Dependencies**: 9.1, 8.1, 6.2
- **Definition of Done**: Material dialog form with: Title (required, max 200), Description (textarea, optional), Due Date (date picker, optional), Category (select dropdown populated from CategoryService, optional). Supports add and edit modes. Form validation with required, maxlength, minlength.
- **Deliverables**: `src/app/todos/todo-form/todo-form.component.html`, `todo-form.component.scss`

### Task 9.5 — Create TodoFormComponent — logic [x]
- **Requirement**: REQ-TODO-001 (all create scenarios), REQ-TODO-004 (update scenarios)
- **Dependencies**: 9.4
- **Definition of Done**: On dialog open in add mode, form is empty. In edit mode, form pre-filled with existing data. On submit, calls create or update. Displays server validation errors (400) mapped back to form fields. Displays generic error on failure. Returns created/updated todo on close. Loads categories on init for dropdown.
- **Deliverables**: `src/app/todos/todo-form/todo-form.component.ts`

### Task 9.6 — Implement completion toggle and delete [x]
- **Requirement**: REQ-TODO-004 (update completion status), REQ-TODO-005 (delete existing)
- **Dependencies**: 9.3
- **Definition of Done**: Checkbox in table row toggles `isCompleted` status inline (calls `TodoService.update()` with only `isCompleted` toggled). Delete button opens confirmation dialog. On confirm, calls `TodoService.delete()`. Shows success snackbar. Refreshes list. Handles 404 errors.
- **Deliverables**: Updated `todo-list.component.ts`, `todo-list.component.html`

## 10. Validation & Final Integration

### Task 10.1 — End-to-end test: Authentication flow [x]
- **Requirement**: REQ-AUTH-001 (all 3 scenarios), REQ-AUTH-002 (both scenarios)
- **Dependencies**: 3.5, 7.3, 7.4
- **Definition of Done**: Manual test: Register new user — success. Register same user again — 409 error. Register with short password — 400 error. Login with valid credentials — JWT returned. Login with wrong password — 401 error. Protected endpoint without token — 401 error.
- **Deliverables**: Test results (documented or screenshots)

### Task 10.2 — End-to-end test: Categories CRUD [x]
- **Requirement**: REQ-CAT-001 (all 3 scenarios), REQ-CAT-002, REQ-CAT-003, REQ-CAT-004 (all 3 scenarios)
- **Dependencies**: 4.5, 8.4
- **Definition of Done**: Manual test: Create category — success. Create duplicate — 409. List categories — shows created. Update name — success. Create todo in category. Delete category with todo — 409. Delete category without todos — 204. Delete non-existent — 404.
- **Deliverables**: Test results (documented or screenshots)

### Task 10.3 — End-to-end test: Todos CRUD [x]
- **Requirement**: REQ-TODO-001 (all 3 scenarios), REQ-TODO-003 (all 3 scenarios), REQ-TODO-004 (both scenarios), REQ-TODO-005 (both scenarios)
- **Dependencies**: 5.7, 9.5, 9.6
- **Definition of Done**: Manual test: Create todo without category — success. Create with all fields — success. Create without title — 400. Get by ID — success. Get non-existent — 404. Get another user's todo — 404. Update fields — success. Update non-existent — 404. Delete — 204. Delete non-existent — 404.
- **Deliverables**: Test results (documented or screenshots)

### Task 10.4 — End-to-end test: Todo filters [x]
- **Requirement**: REQ-TODO-002 (all 6 filter scenarios)
- **Dependencies**: 5.5, 9.3
- **Definition of Done**: Manual test: List all — returns all user's todos. Filter by completed — only completed shown. Filter by category — only category matches. Filter by date range — correct range. Search by text — matches title/description. Combined filters — AND logic working.
- **Deliverables**: Test results (documented or screenshots)

### Task 10.5 — Spec coverage verification [x]
- **Requirement**: All
- **Dependencies**: 10.1, 10.2, 10.3, 10.4
- **Definition of Done**: Every scenario from all 3 spec files maps to at least one passing test. Missing coverage documented as gaps for future iterations. Traceability matrix created or confirmed.
- **Deliverables**: Spec coverage report
