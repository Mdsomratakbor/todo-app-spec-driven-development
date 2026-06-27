## Context

Adding priority levels (Low/Medium/High) to the existing Todo entity. The current todo model supports title, description, completion status, due date, and category. Priority is a new field that affects creation, display, filtering, and sorting.

## System Architecture

This change operates within the existing layered architecture (Angular SPA → .NET Web API → PostgreSQL). No new layers or services are introduced. The priority field flows through all existing layers:

```
Angular Component (TodoForm) ──▶ TodoService ──HTTP──▶ TodoController ──▶ TodoService ──▶ AppDbContext ──▶ PostgreSQL
                                UI Display ◀───────────────────────────────────────────────────────────────────
```

The priority value is added to existing request/response DTOs and the database schema with no architectural changes.

## Folder Structure

No new folders are added. Modified files within the existing structure:

```
TodoApi/
├── Models/
│   ├── Entities/Todo.cs           (+ Priority property)
│   ├── DTOs/Todos/
│   │   ├── TodoRequest.cs         (+ priority field)
│   │   ├── TodoResponse.cs        (+ priority field)
│   │   └── TodoFilterRequest.cs   (+ priority, sortBy, sortDirection)
│   └── Mappings/MappingProfile.cs (+ priority mapping)
├── Services/TodoService.cs        (+ priority handling, filter, sort)
├── Controllers/TodoController.cs  (+ new query params)
└── Data/
    ├── Configurations/TodoConfiguration.cs (+ priority column config)
    └── Migrations/                (+ new migration)

TodoApp/src/app/todos/
├── todo.models.ts                 (+ priority field)
├── todo.service.ts                (+ priority in requests)
├── todo-list/                     (+ priority column, filter, sort)
└── todo-form/                     (+ priority selector)
```

## Goals / Non-Goals

**Goals:**
- Add a `Priority` field to the Todo entity with values: Low, Medium, High (default: Medium)
- Update all API contracts (request, response, filter) to include priority
- Add priority filter and sort options to the GET /api/v1/todos endpoint
- Display priority in the Angular Todo list with visual indicators
- Add priority selector to the Todo create/edit form

**Non-Goals:**
- Custom priority levels (values are fixed to Low/Medium/High)
- Priority-based push notifications or reminders
- Automated priority suggestions or escalation
- Bulk priority updates

## API Endpoints

No new endpoints are added. Existing endpoints are updated to accept/return the priority field:

| Method | Path | Change |
|---|---|---|
| POST | /api/v1/todos | Request body gains optional `priority` field. Response includes `priority`. |
| GET | /api/v1/todos | New query params: `priority`, `sortBy`, `sortDirection`. Response includes `priority`. |
| GET | /api/v1/todos/{id} | Response includes `priority`. |
| PUT | /api/v1/todos/{id} | Request body gains optional `priority` field. Response includes `priority`. |
| DELETE | /api/v1/todos/{id} | Unchanged. |

## Component Diagram

```
                    TodoFormComponent
                    ┌─────────────────────┐
                    │ Title  [________]   │
                    │ Desc   [________]   │
                    │ Due    [____]       │
                    │ Cat    [v Select]   │
                    │ Priority [v Select] │◄── NEW
                    │ [Save]              │
                    └─────────┬───────────┘
                              │
                    TodoListComponent
    ┌──────────────────────────────────────────┐
    │ 🔍 Search  [Status v] [Cat v] [Pri v]   │◄── NEW filter
    │ ┌────┬────────┬──────┬──────┬──────────┐│
    │ │ ☐  │ Title  │ Cat  │ Due  │ Priority ││◄── NEW column
    │ │ ☑  │ Buy... │ Pers │ 7/1  │ 🔴 High  ││
    │ │ ☐  │ Read.. │ Work │ 7/5  │ 🟡 Med   ││
    │ └────┴────────┴──────┴──────┴──────────┘│
    └──────────────────────────────────────────┘
```

## Decisions

| Decision | Choice | Rationale | Alternatives Considered |
|---|---|---|---|
| Priority data type | String column in DB (`VARCHAR(6)`), enum in C# (`TodoPriority` enum) | Simple, readable in DB, enum provides type safety in code. Low/Medium/High fit in 6 chars. | Integer (0/1/2) — less readable in DB queries; PostgreSQL enum type — adds migration complexity for no benefit |
| Default priority | Medium | Neutral default — doesn't force users to choose but provides sensible baseline. | Low (too conservative); High (encourages over-prioritization); null/required (forces choice on every create) |
| API representation | String in JSON (`"priority": "high"`) | Consistent with how enums serialize in ASP.NET Core. Case-insensitive on input. | Integer (0/1/2) — less self-documenting in API responses |
| Filter by priority | Query param `?priority=high` on GET /api/v1/todos | Follows existing filter pattern (query params, AND logic). | Separate endpoint — inconsistent with existing design |
| Sort by priority | Query param `?sortBy=priority&sortDirection=desc` | Extends existing listing. No sort exists yet, so this lays groundwork for future sort options. | Not implementing — users may want priority-sorted views |
| Visual indicator | Color-coded badge (Red=High, Yellow=Medium, Green=Low) in Angular Material chips | Intuitive, accessible, matches common patterns. | Icons only (less scannable); background colors (harder to read) |
| Frontend selector | Material select dropdown in TodoFormComponent | Matches existing form patterns (category select). | Radio buttons (takes more space); segmented buttons (Material 3 pattern — less familiar) |

## Risks / Trade-offs

- **String-based priority in DB** could allow invalid values if not validated. Mitigation: C# enum validation on the API layer, DB constraint `CHECK (priority IN ('low', 'medium', 'high'))` for defense in depth.
- **Adding priority to existing todos** — existing records will get NULL priority. Migration must set default 'medium' for existing rows.
- **Sort parameter design** introduces a new pattern (sortBy/sortDirection). Keep it simple — no multi-column sort in this iteration.
