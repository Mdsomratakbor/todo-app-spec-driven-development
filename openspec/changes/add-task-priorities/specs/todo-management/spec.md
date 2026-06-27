## Capability Context

The Todo Management capability provides full CRUD operations for todo items, including search, filter, and sort. This delta spec adds priority assignment (Low/Medium/High) to the existing capability.

**Preconditions:** User is authenticated (valid JWT token). All todo endpoints require Authorization header.

**System Boundary:** Priority is limited to three fixed levels (Low, Medium, High). No custom priority levels, automated escalation, or priority-based notifications.

## Contracts

All priority-related changes to existing API contracts are documented below. Existing fields remain unchanged.

### POST /api/v1/todos

**Request (updated with priority):**
```
{
  "title": "string (required, 1-200 chars)",
  "description": "string (optional, max 2000 chars)",
  "dueDate": "ISO 8601 datetime (optional)",
  "categoryId": "uuid (optional)",
  "priority": "string (optional, one of: low, medium, high, default: medium)"
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
    "priority": "high",
    "createdAt": "2026-06-27T10:00:00Z",
    "updatedAt": "2026-06-27T10:00:00Z"
  },
  "message": null,
  "errors": null
}
```

### GET /api/v1/todos

**Additional Query Parameters (all optional, combined with AND):**
- `priority`: string (one of: low, medium, high) — filter by priority level
- `sortBy`: string (values: priority) — field to sort by
- `sortDirection`: string (values: asc, desc) — sort direction (default: asc)

### PUT /api/v1/todos/{id}

**Request (updated with priority):**
```
{
  "title": "string (optional, 1-200 chars)",
  "description": "string (optional, nullable)",
  "dueDate": "ISO 8601 datetime (optional, nullable)",
  "isCompleted": "boolean (optional)",
  "categoryId": "uuid (optional, nullable)",
  "priority": "string (optional, one of: low, medium, high)"
}
```

### Error Response Format

All errors use FluentResponse.ApiWrapper envelope (unchanged):
```
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": {
    "priority": ["Priority must be one of: low, medium, high"]
  }
}
```

## ADDED Requirements

### REQ-TODO-006: User can set and view priority on a todo
The system SHALL allow an authenticated user to assign a priority (Low, Medium, High) to a todo item.

**Business Rules:**
- BR-TODO-010: Priority SHALL be one of: `low`, `medium`, `high`. Values SHALL be case-insensitive on input.
- BR-TODO-011: New todos SHALL default to `medium` priority when no priority is provided.
- BR-TODO-012: Priority MAY be updated independently of other fields.

#### Scenario: Create todo with priority
- **WHEN** an authenticated user submits a POST /api/v1/todos request with `"priority": "high"`
- **THEN** the system SHALL create a todo with priority set to "high" and return a 201 response with the priority field in the TodoResponse

#### Scenario: Create todo without priority (default)
- **WHEN** an authenticated user submits a POST /api/v1/todos request without a priority field
- **THEN** the system SHALL create a todo with priority set to "medium" and return a 201 response

#### Scenario: Create todo with invalid priority
- **WHEN** an authenticated user submits a POST /api/v1/todos request with `"priority": "urgent"`
- **THEN** the system SHALL return a 400 Bad Request response with validation errors

#### Scenario: Update todo priority
- **WHEN** an authenticated user sends a PUT /api/v1/todos/{id} request with `"priority": "low"` for their own todo
- **THEN** the system SHALL update the priority to "low" and return a 200 response with the updated TodoResponse

#### Scenario: Filter todos by priority
- **WHEN** an authenticated user sends a GET /api/v1/todos request with `?priority=high`
- **THEN** the system SHALL return only todos with priority "high"

#### Scenario: Sort todos by priority
- **WHEN** an authenticated user sends a GET /api/v1/todos request with `?sortBy=priority&sortDirection=desc`
- **THEN** the system SHALL return todos ordered by priority descending (High → Medium → Low)

#### Scenario: View priority in todo list
- **WHEN** an authenticated user views their todo list
- **THEN** each todo SHALL display a visual priority indicator (color-coded badge: Red=High, Yellow=Medium, Green=Low)

#### Scenario: Priority field in todo response
- **WHEN** the system returns any TodoResponse
- **THEN** the response SHALL include a `"priority"` field with value `"low"`, `"medium"`, or `"high"`
