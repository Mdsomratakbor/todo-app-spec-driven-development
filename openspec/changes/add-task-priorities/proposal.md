## Project Vision

This change enhances the existing Todo application by adding priority levels to tasks, enabling users to organize work by importance.

## Problem Statement

Users currently cannot distinguish between urgent and low-importance tasks. All todos appear equal, making it difficult to decide what to work on first. This leads to disorganized task management and missed deadlines.

## Goals

- Allow users to assign Low/Medium/High priority to any todo
- Support filtering and sorting by priority
- Provide clear visual indicators for priority levels
- Maintain backward compatibility with existing data

## Non-Goals

- Custom or user-defined priority levels
- Automated priority escalation or suggestions
- Priority-based notifications or reminders
- Bulk priority updates

## Scope

**In Scope:**
- Priority field on Todo entity (Low/Medium/High, default Medium)
- Create, update, display, filter, and sort by priority
- Color-coded visual indicators in the UI
- API contract updates and database migration

**Out of Scope:**
- Changing existing todo-management endpoints (backward compatible)
- Real-time collaboration features
- Mobile app changes beyond responsive web

## Stakeholders

- **End Users**: People who manage tasks and need to prioritize their work
- **Development Team**: Responsible for implementing the priority feature

## Assumptions

- Existing todos without priority will default to Medium after migration
- Users understand the three-level priority system (Low/Medium/High)
- No changes required to authentication, categories, or other capabilities

## Risks

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| Existing todos have NULL priority after migration | High | Medium | Migration script sets default 'medium' for existing rows |
| Invalid priority values in API input | Low | Low | Enum validation on API layer; DB CHECK constraint |
| Frontend sort/filter adds complexity | Low | Medium | Keep sort simple (single-column only) |

## What Changes

- Add a `priority` field to the Todo entity with values: Low, Medium, High
- Update all Todo API contracts (create, update, response, filter) to include priority
- Update the Angular Todo form with a priority selector (dropdown/chips)
- Display priority in the Todo list with visual indicators (color/badge)
- Allow filtering/sorting by priority in the list view

## Capabilities

### New Capabilities

- *(none — priority is an enhancement to existing todo-management)*

### Modified Capabilities

- `todo-management`: Add priority requirement — todos gain a `priority` enum field (Low/Medium/High) affecting creation, display, filtering, and sorting.

## Impact

- **Backend**: `Todo` entity gets `Priority` enum column. DTOs (`TodoRequest`, `TodoResponse`, `TodoFilterRequest`) gain `priority` field. `TodoService` updated to handle and filter by priority. `MappingProfile` updated. New migration required.
- **Frontend**: `TodoFormComponent` gets priority selector. `TodoListComponent` displays priority badge and adds sort/filter controls. `Todo` model updated.
- **Database**: New `Priority` column on `Todos` table (string/enum). Index on `(UserId, Priority)` optional.
- **Tests**: Update existing todo service tests; add priority-specific scenarios.
