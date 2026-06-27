# Regression Plan — add-task-priorities

## Scope

Adding priority field to existing Todo entity and API. Existing todo-management behavior must remain unchanged.

## Regression Test Areas

| Area | Risk | What to Verify |
|---|---|---|
| Create todo (no priority) | Medium | Existing create without priority defaults to Medium; all other fields unchanged |
| Create todo (all fields) | Medium | Existing optional fields (description, dueDate, categoryId) still work |
| Update todo (no priority) | Medium | Existing update without priority field doesn't change priority |
| Update todo (existing fields) | Medium | Title, description, dueDate, isCompleted, categoryId updates still work |
| List todos (no filter) | Low | All user's todos returned, ordered by createdAt desc |
| Filter by existing params | Low | isCompleted, categoryId, dueBefore, dueAfter, search filters still work |
| Get by ID | Low | Existing get-by-ID returns full response with new priority field |
| Delete todo | Low | Delete behavior unchanged |
| Data isolation | Medium | User A cannot see/modify User B's todos; 404 returned |
| Category operations | Low | Category CRUD unaffected by priority addition |
| Auth (register/login/JWT) | Low | Authentication flow unchanged |

## Test Execution

1. Run existing unit test suite: `dotnet test TodoApi.Tests/`
2. Run Angular build: `ng build`
3. Manual smoke test of all CRUD operations for todos and categories
4. Verify all existing 67 tests pass before and after migration
5. Verify new priority tests pass: create default Medium, create with explicit priority, update priority, filter by priority, sort by priority asc/desc (73 total tests)

## New Priority-Specific Regression Cases

| Scenario | Expected |
|---|---|
| Create todo without priority | Priority defaults to "medium" |
| Create todo with priority "high" | Priority stored as "high" |
| Update todo priority from medium to low | Priority changes to "low" |
| Update todo without sending priority field | Priority unchanged (not reset to default) |
| Filter by priority=high | Only high priority todos returned |
| Sort by priority asc | Order: Low < Medium < High |
| Sort by priority desc | Order: High > Medium > Low |
| Backward compatibility: existing todos without priority in DB | Migration assigns "Medium" default |

## Rollback

- Revert migration: `dotnet ef migrations remove`
- Revert code changes: `git revert <commit-hash>`
