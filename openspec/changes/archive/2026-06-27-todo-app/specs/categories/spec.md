## Capability Context

The Categories capability allows authenticated users to organize their todos into named groups. Categories are user-scoped — each user sees and manages only their own categories. Categories can be assigned to multiple todos, but deleting a category only removes the association (todos are preserved).

**Preconditions:** User is authenticated (valid JWT token). Category endpoints require Authorization header.

**System Boundary:** CRUD operations on category names only. Category color, icon, or ordering are out of scope.

## Business Rules

- BR-CAT-001: Category name SHALL be unique per user (case-insensitive comparison).
- BR-CAT-002: Category name SHALL be required and SHALL NOT exceed 50 characters.
- BR-CAT-003: Category name SHALL NOT be empty or whitespace-only.
- BR-CAT-004: A category SHALL NOT be deleted if it has associated todos. The caller MUST reassign or delete those todos first.
- BR-CAT-005: Deleting a category sets `CategoryId` to NULL on associated todos (SET NULL referential action).
- BR-CAT-006: All category operations SHALL be scoped to the authenticated user. A user SHALL NOT see or modify another user's categories.

## Contracts

### POST /api/v1/categories

**Request:**
```
{
  "name": "string (required, 1-50 chars)"
}
```

**Success Response** (201 Created):
```
{
  "success": true,
  "data": {
    "id": "3a4b5c6d-...",
    "name": "Work",
    "todoCount": 0,
    "createdAt": "2026-06-27T10:00:00Z"
  },
  "message": null,
  "errors": null
}
```

### GET /api/v1/categories

**Success Response** (200 OK):
```
{
  "success": true,
  "data": [
    { "id": "...", "name": "Personal", "todoCount": 3, "createdAt": "..." },
    { "id": "...", "name": "Work", "todoCount": 5, "createdAt": "..." }
  ],
  "message": null,
  "errors": null
}
```

### PUT /api/v1/categories/{id}

**Request:** Same as create (`{ "name": "..." }`)

**Success Response** (200 OK): Updated `CategoryResponse`

### DELETE /api/v1/categories/{id}

**Success Response:** 204 No Content

**Error Responses:**
- 400: Validation failure (missing/empty name)
- 404: Category not found or not owned by user
- 409: Category has associated todos (include count in message)

## ADDED Requirements

### REQ-CAT-001: User can create a category
The system SHALL allow an authenticated user to create a new category.

#### Scenario: Create category with valid name
- **WHEN** an authenticated user submits a POST /api/v1/categories request with a name between 1 and 50 characters
- **THEN** the system SHALL create the category scoped to that user and return a 201 response with the created category's id, name (todoCount = 0), and createdAt timestamp

#### Scenario: Create category with duplicate name
- **WHEN** an authenticated user submits a POST request with a category name that already exists for that user (case-insensitive)
- **THEN** the system SHALL return a 409 Conflict response with an error message

#### Scenario: Create category with empty or whitespace name
- **WHEN** an authenticated user submits a POST request with an empty or whitespace-only name
- **THEN** the system SHALL return a 400 Bad Request response

#### Scenario: Create category with name exceeding max length
- **WHEN** an authenticated user submits a POST request with a name longer than 50 characters
- **THEN** the system SHALL return a 400 Bad Request response

### REQ-CAT-002: User can list their categories
The system SHALL allow an authenticated user to retrieve all their categories.

#### Scenario: List all categories
- **WHEN** an authenticated user sends a GET /api/v1/categories request
- **THEN** the system SHALL return a 200 response with an array of all categories belonging to that user, ordered alphabetically by name, each including id, name, todoCount, and createdAt

#### Scenario: List categories when none exist
- **WHEN** an authenticated user sends a GET /api/v1/categories request and has no categories
- **THEN** the system SHALL return a 200 response with an empty array

### REQ-CAT-003: User can update a category
The system SHALL allow an authenticated user to rename an existing category.

#### Scenario: Update category name to a unique value
- **WHEN** an authenticated user sends a PUT /api/v1/categories/{id} request with a new name that is unique for that user
- **THEN** the system SHALL update the category name and return a 200 response with the updated category

#### Scenario: Update category name to a duplicate
- **WHEN** an authenticated user sends a PUT request renaming a category to a name that already exists for that user
- **THEN** the system SHALL return a 409 Conflict response

#### Scenario: Update non-existent category
- **WHEN** an authenticated user sends a PUT request for a category ID that does not exist or belongs to another user
- **THEN** the system SHALL return a 404 Not Found response

### REQ-CAT-004: User can delete a category
The system SHALL allow an authenticated user to delete a category, but only if it has no associated todos.

#### Scenario: Delete category with no todos
- **WHEN** an authenticated user sends a DELETE /api/v1/categories/{id} request for their own category that has zero associated todos
- **THEN** the system SHALL delete the category and return a 204 No Content response

#### Scenario: Delete category with associated todos
- **WHEN** an authenticated user sends a DELETE request for their own category that has one or more associated todos
- **THEN** the system SHALL return a 409 Conflict response with a message indicating the category still has todos and the count

#### Scenario: Delete non-existent category
- **WHEN** an authenticated user sends a DELETE request for a category ID that does not exist or belongs to another user
- **THEN** the system SHALL return a 404 Not Found response

## Validation Targets

- REQ-CAT-001 is met when all 4 scenarios pass
- REQ-CAT-002 is met when both scenarios pass
- REQ-CAT-003 is met when all 3 scenarios pass
- REQ-CAT-004 is met when all 3 scenarios pass
