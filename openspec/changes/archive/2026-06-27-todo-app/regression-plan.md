# Non-Regression Test Plan — todo-app

**Change:** todo-app  
**Date:** 2026-06-27

## Affected Areas

This change introduces a full-stack Todo List application. The following existing systems/functions are affected or could be affected by regressions:

| Area | Risk Level | Reason |
|---|---|---|
| Authentication | High | JWT token generation, validation, and user identity are foundational to all other features. A regression here breaks the entire application. |
| Data Isolation | High | All data access is scoped to the authenticated user. A regression could expose data between users. |
| API Response Format | Medium | FluentResponse.ApiWrapper envelope must be consistent across all endpoints. A regression could break frontend response parsing. |
| Database Schema | High | EF Core migration creates the initial schema. A regression in migration could cause data loss or corruption on future schema changes. |
| CORS Configuration | Low | Changes to CORS policy could break Angular dev server connectivity. |

## Regression Test Checklist

### Authentication (run before each release)
- [ ] User registration returns 201 with JWT
- [ ] Duplicate username returns 409
- [ ] Invalid password returns 400
- [ ] Login with valid credentials returns 200 with JWT
- [ ] Login with invalid credentials returns 401
- [ ] Protected endpoint without token returns 401
- [ ] Protected endpoint with expired token returns 401

### Data Isolation
- [ ] User A cannot access User B's todos (returns 404)
- [ ] User A cannot access User B's categories (returns 404)
- [ ] User A cannot see User B's data in list endpoints
- [ ] Category name uniqueness is per-user only

### Categories
- [ ] Create, list, update, delete categories
- [ ] Delete category with associated todos returns 409
- [ ] Delete category without todos returns 204

### Todos
- [ ] Create, list (with filters), get-by-id, update, delete todos
- [ ] Filter by completion status, category, date range, text search
- [ ] Combined filters produce AND logic
- [ ] Update category assignment (set and remove)
- [ ] Delete non-existent todo returns 404

### API Response Format
- [ ] All success responses use FluentResponse.ApiWrapper envelope (`{ success, data, message, errors }`)
- [ ] All error responses include appropriate HTTP status code
- [ ] Validation errors include field-level details

### Database Migration
- [ ] `dotnet ef database update` succeeds from clean state
- [ ] All expected tables exist (AspNetUsers, AspNetRoles, AspNetRoleClaims, AspNetUserClaims, AspNetUserLogins, AspNetUserRoles, AspNetUserTokens, Categories, Todos)
- [ ] All indexes and foreign keys are created

## Test Suites

- **Backend Unit Tests**: `TodoApi.Tests` project — run `dotnet test TodoApi.Tests`
- **Manual E2E Tests**: See Tasks 10.1–10.4 in `tasks.md` for detailed manual test scenarios
