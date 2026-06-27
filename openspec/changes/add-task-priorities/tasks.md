## 1. Backend: Data Layer

- [ ] 1.1 Add `TodoPriority` enum (Low, Medium, High) in `Models/Enums/`
- [ ] 1.2 Add `Priority` property to `Todo` entity (type `TodoPriority`, default `Medium`)
- [ ] 1.3 Update `TodoConfiguration` with priority column config (`VARCHAR(6)`, not null, default 'medium')
- [ ] 1.4 Create EF Core migration for priority column

## 2. Backend: API Layer

- [ ] 2.1 Add `priority` to `TodoRequest` (optional string, validated against enum)
- [ ] 2.2 Add `priority` to `TodoResponse` (string)
- [ ] 2.3 Add `priority` and `sortBy`/`sortDirection` to `TodoFilterRequest`
- [ ] 2.4 Update `MappingProfile` with priority mapping (enum ↔ string)
- [ ] 2.5 Update `TodoService.CreateAsync` to handle priority (default Medium)
- [ ] 2.6 Update `TodoService.UpdateAsync` to handle priority changes
- [ ] 2.7 Update `TodoService.GetAllAsync` to support filter by priority and sort by priority
- [ ] 2.8 Update `TodoController` to pass filter/sort params to service

## 3. Frontend: Model & Service

- [ ] 3.1 Add `priority` field to `Todo` and `TodoFilter` models in `todo.models.ts`
- [ ] 3.2 Update `TodoService` to include priority in create/update requests and filter/sort params

## 4. Frontend: Todo Form

- [ ] 4.1 Add priority dropdown (Material select) to `TodoFormComponent` template with Low/Medium/High options
- [ ] 4.2 Wire priority form control in `TodoFormComponent` logic (default Medium on create)

## 5. Frontend: Todo List

- [ ] 5.1 Add priority column to `TodoListComponent` table with color-coded badge (Red=High, Yellow=Medium, Green=Low)
- [ ] 5.2 Add priority filter dropdown to filter toolbar
- [ ] 5.3 Add sort-by-priority option to list (e.g., clickable column header)

## 6. Tests

- [ ] 6.1 Update backend `TodoServiceTests` with priority scenarios (create with priority, default, filter, sort, invalid value)
- [ ] 6.2 Verify existing tests still pass after migration
