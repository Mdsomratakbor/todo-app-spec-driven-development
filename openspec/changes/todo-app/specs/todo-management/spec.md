## Capability Context

The Todo Management capability provides full CRUD operations for todo items, including search and filter. Each todo belongs to exactly one user and optionally belongs to one category. Todos track their completion status and due dates.

**Preconditions:** User is authenticated (valid JWT token). All todo endpoints require Authorization header. Category endpoints (for category assignment) must exist before a todo can reference a category.

**System Boundary:** Single-user todo items only. No shared/assigned todos, no recurring todos, no subtasks, no file attachments.

## Business Rules

- BR-TODO-001: Todo title SHALL be required and SHALL NOT exceed 200 characters.
- BR-TODO-002: Todo description SHALL be optional and SHALL NOT exceed 2000 characters.
- BR-TODO-003: New todos SHALL default to IsCompleted = false.
- BR-TODO-004: Completed todos MAY be updated (title, description, due date, category) and MAY be re-opened (set IsCompleted = false).
- BR-TODO-005: Deleting a todo is permanent. No undo or soft-delete in this iteration.
- BR-TODO-006: All todo operations SHALL be scoped to the authenticated user. A user SHALL NOT see or modify another user's todos. Attempting to access another user's todo SHALL return 404 (not 403 — no information leakage).
- BR-TODO-007: When a CategoryId is provided on create or update, the category MUST exist and MUST belong to the same user. If it does not, return 404.
- BR-TODO-008: Setting CategoryId to null on update SHALL remove the category association from the todo.
- BR-TODO-009: Todo listing without filters SHALL return all user's todos ordered by CreatedAt descending. No pagination is implemented in this iteration (known gap).

## Contracts

### POST /api/v1/todos

**Request:**
```
{
  "title": "string (required, 1-200 chars)",
  "description": "string (optional, max 2000 chars)",
  "dueDate": "ISO 8601 datetime (optional)",
  "categoryId": "uuid (optional)"
}
```

**Success Response** (201 Created):
```
{
  "success": true,
  "data": {
    "id": "uuid",
    "title": "Buy groceries",
    "description": "Milk, eggs, bread",
    "isCompleted": false,
    "dueDate": "2026-07-01T00:00:00Z",
    "categoryId": "uuid-or-null",
    "categoryName": "Personal-or-null",
    "createdAt": "2026-06-27T10:00:00Z",
    "updatedAt": "2026-06-27T10:00:00Z"
  },
  "message": null,
  "errors": null
}
```

### GET /api/v1/todos

**Query Parameters** (all optional, combined with AND):
- `isCompleted`: boolean
- `categoryId`: uuid
- `dueBefore`: ISO 8601 datetime
- `dueAfter`: ISO 8601 datetime
- `search`: string (case-insensitive match on title or description)

**Success Response** (200 OK): Array of `TodoResponse`

### GET /api/v1/todos/{id}

**Success Response** (200 OK): Single `TodoResponse`
**Error Response:** 404 Not Found

### PUT /api/v1/todos/{id}

**Request:** Same fields as create (all optional on update):
```
{
  "title": "string (optional, 1-200 chars)",
  "description": "string (optional, nullable)",
  "dueDate": "ISO 8601 datetime (optional, nullable)",
  "isCompleted": "boolean (optional)",
  "categoryId": "uuid (optional, nullable)"
}
```

**Success Response** (200 OK): Updated `TodoResponse`

### DELETE /api/v1/todos/{id}

**Success Response:** 204 No Content
**Error Response:** 404 Not Found

## ADDED Requirements

### REQ-TODO-001: User can create a todo
The system SHALL allow an authenticated user to create a new todo item.

#### Scenario: Create todo with title only
- **WHEN** an authenticated user submits a POST /api/v1/todos request with only a title (required)
- **THEN** the system SHALL create a todo with that title, IsCompleted = false, no description, no due date, no category, and return a 201 response with the complete TodoResponse including the generated id

#### Scenario: Create todo with all optional fields
- **WHEN** an authenticated user submits a POST request with title, description, dueDate, and a valid categoryId that belongs to the user
- **THEN** the system SHALL create a todo with all provided fields and return a 201 response with the complete TodoResponse including categoryName

#### Scenario: Create todo without title
- **WHEN** an authenticated user submits a POST request without a title or with an empty/whitespace title
- **THEN** the system SHALL return a 400 Bad Request response with validation errors

#### Scenario: Create todo with title exceeding max length
- **WHEN** an authenticated user submits a POST request with a title longer than 200 characters
- **THEN** the system SHALL return a 400 Bad Request response

#### Scenario: Create todo with non-existent category
- **WHEN** an authenticated user submits a POST request with a categoryId that does not exist or belongs to another user
- **THEN** the system SHALL return a 404 Not Found response

### REQ-TODO-002: User can list todos with filters
The system SHALL allow an authenticated user to retrieve their todos with optional filtering.

#### Scenario: List all todos
- **WHEN** an authenticated user sends a GET /api/v1/todos request without query parameters
- **THEN** the system SHALL return a 200 response with all todos belonging to that user, ordered by createdAt descending

#### Scenario: Filter by completion status
- **WHEN** an authenticated user sends a GET request with `?isCompleted=true`
- **THEN** the system SHALL return only todos where IsCompleted = true

#### Scenario: Filter by category
- **WHEN** an authenticated user sends a GET request with `?categoryId=<valid-uuid>`
- **THEN** the system SHALL return only todos in that category

#### Scenario: Filter by due date range
- **WHEN** an authenticated user sends a GET request with `?dueBefore=2026-07-01T00:00:00Z&dueAfter=2026-06-01T00:00:00Z`
- **THEN** the system SHALL return only todos with dueDate on or after dueAfter and on or before dueBefore

#### Scenario: Search by text
- **WHEN** an authenticated user sends a GET request with `?search=grocery`
- **THEN** the system SHALL return todos whose title or description contains "grocery" (case-insensitive match)

#### Scenario: Combined filters with AND logic
- **WHEN** an authenticated user sends a GET request with multiple filter parameters (e.g., `?isCompleted=false&categoryId=<id>&search=meeting`)
- **THEN** the system SHALL return only todos matching ALL filter criteria simultaneously

#### Scenario: No matching todos
- **WHEN** an authenticated user sends a GET request with filters that match no todos
- **THEN** the system SHALL return a 200 response with an empty array

### REQ-TODO-003: User can get a single todo
The system SHALL allow an authenticated user to retrieve a specific todo by ID.

#### Scenario: Get own existing todo
- **WHEN** an authenticated user sends a GET /api/v1/todos/{id} request with a valid todo ID that belongs to them
- **THEN** the system SHALL return a 200 response with the full TodoResponse

#### Scenario: Get non-existent todo
- **WHEN** an authenticated user sends a GET request with a non-existent todo ID
- **THEN** the system SHALL return a 404 Not Found response

#### Scenario: Get another user's todo
- **WHEN** an authenticated user sends a GET request for a todo that exists but belongs to a different user
- **THEN** the system SHALL return a 404 Not Found response (no information leakage)

### REQ-TODO-004: User can update a todo
The system SHALL allow an authenticated user to update any field of an existing todo.

#### Scenario: Update title and description
- **WHEN** an authenticated user sends a PUT /api/v1/todos/{id} request with updated title and description for their own todo
- **THEN** the system SHALL update those fields, set updatedAt to the current time, and return a 200 response with the updated TodoResponse

#### Scenario: Toggle completion status
- **WHEN** an authenticated user sends a PUT request with `isCompleted: true` for an incomplete todo (or vice versa)
- **THEN** the system SHALL toggle the completion status and return the updated todo

#### Scenario: Update category assignment
- **WHEN** an authenticated user sends a PUT request with a valid categoryId
- **THEN** the system SHALL assign the todo to that category and return the updated todo with categoryName

#### Scenario: Remove category assignment
- **WHEN** an authenticated user sends a PUT request with `categoryId: null`
- **THEN** the system SHALL remove the category association and return the updated todo with categoryId = null and categoryName = null

#### Scenario: Update non-existent todo
- **WHEN** an authenticated user sends a PUT request for a todo ID that does not exist or belongs to another user
- **THEN** the system SHALL return a 404 Not Found response

#### Scenario: Update with invalid category
- **WHEN** an authenticated user sends a PUT request with a categoryId that does not exist
- **THEN** the system SHALL return a 404 Not Found response

### REQ-TODO-005: User can delete a todo
The system SHALL allow an authenticated user to permanently delete a todo.

#### Scenario: Delete own todo
- **WHEN** an authenticated user sends a DELETE /api/v1/todos/{id} request for their own todo
- **THEN** the system SHALL permanently delete the todo and return a 204 No Content response

#### Scenario: Delete non-existent todo
- **WHEN** an authenticated user sends a DELETE request for a todo ID that does not exist or belongs to another user
- **THEN** the system SHALL return a 404 Not Found response

## Validation Targets

- REQ-TODO-001 is met when all 5 scenarios pass
- REQ-TODO-002 is met when all 7 scenarios pass
- REQ-TODO-003 is met when all 3 scenarios pass
- REQ-TODO-004 is met when all 6 scenarios pass
- REQ-TODO-005 is met when both scenarios pass
