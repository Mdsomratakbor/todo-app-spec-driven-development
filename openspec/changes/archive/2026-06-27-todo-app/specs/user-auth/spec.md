## Capability Context

The User Authentication capability handles user registration and login. It provides JWT-based session management and enforces data isolation so authenticated users can only access their own data. This capability is a prerequisite for all other features.

**Preconditions:** System has ASP.NET Core Identity configured with a PostgreSQL store. JWT signing key is configured in application settings. Auth endpoints are unauthenticated (no JWT required).

**System Boundary:** Registration and login only. Password reset, email verification, and OAuth are out of scope.

## Business Rules

- BR-AUTH-001: Username SHALL be unique across all users (case-insensitive).
- BR-AUTH-002: Password SHALL be at least 6 characters. No maximum length limit is enforced.
- BR-AUTH-003: Password SHALL be hashed using ASP.NET Core Identity's PasswordHasher before storage. Plain-text passwords SHALL never be stored.
- BR-AUTH-004: JWT tokens SHALL include the user ID and username as claims. Token expiry SHALL be 24 hours from issuance.
- BR-AUTH-005: Unauthenticated requests to protected endpoints SHALL receive a 401 response with no data exposure.
- BR-AUTH-006: All authenticated requests SHALL be scoped to the requesting user's identity derived from the JWT token.
- BR-AUTH-007: Username SHALL NOT be empty, SHALL NOT be whitespace-only, and SHALL NOT exceed 256 characters.
- BR-AUTH-008: There SHALL be no rate limiting on login attempts for this iteration (known gap).

## Contracts

### POST /api/v1/auth/register

**Request:**
```
{
  "username": "string (required, 1-256 chars, no whitespace-only)",
  "password": "string (required, min 6 chars)"
}
```

**Success Response** (201 Created):
```
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "username": "john",
    "expiresAt": "2026-06-28T12:00:00Z"
  },
  "message": null,
  "errors": null
}
```

**Error Responses:**
- 400: Validation failure (e.g., password too short, empty username)
- 409: Username already exists

### POST /api/v1/auth/login

**Request:**
```
{
  "username": "string (required)",
  "password": "string (required)"
}
```

**Success Response** (200 OK):
Same envelope as register, with new JWT token.

**Error Responses:**
- 400: Validation failure (missing fields)
- 401: Invalid credentials (wrong username or password)

## ADDED Requirements

### REQ-AUTH-001: User can register a new account
The system SHALL allow a new user to register with a username and password.

#### Scenario: Successful registration
- **WHEN** a user submits a POST /api/v1/auth/register request with a unique username and a password of 6 or more characters
- **THEN** the system SHALL create the user account, SHALL hash the password, and SHALL return a 201 response with a JWT token, the username, and the token expiry timestamp

#### Scenario: Duplicate username
- **WHEN** a user submits a registration request with a username that already exists (case-insensitive)
- **THEN** the system SHALL return a 409 Conflict response with an error message indicating the username is taken

#### Scenario: Password too short
- **WHEN** a user submits a registration request with a password shorter than 6 characters
- **THEN** the system SHALL return a 400 Bad Request response with a validation error for the password field

#### Scenario: Empty username
- **WHEN** a user submits a registration request with an empty or whitespace-only username
- **THEN** the system SHALL return a 400 Bad Request response with a validation error for the username field

#### Scenario: Username exceeds maximum length
- **WHEN** a user submits a registration request with a username longer than 256 characters
- **THEN** the system SHALL return a 400 Bad Request response with a validation error

### REQ-AUTH-002: User can log in
The system SHALL allow a registered user to authenticate with their credentials.

#### Scenario: Successful login
- **WHEN** a registered user submits a POST /api/v1/auth/login request with valid username and password
- **THEN** the system SHALL return a 200 response with a JWT token valid for 24 hours, the username, and the token expiry timestamp

#### Scenario: Invalid username
- **WHEN** a user submits a login request with a username that does not exist
- **THEN** the system SHALL return a 401 Unauthorized response without revealing whether the username or password was incorrect

#### Scenario: Wrong password
- **WHEN** a user submits a login request with an existing username but incorrect password
- **THEN** the system SHALL return a 401 Unauthorized response (identical message to invalid username — no information leakage)

### REQ-AUTH-003: Authenticated requests are scoped to the user
The system SHALL restrict all authenticated endpoints to the requesting user's own data only.

#### Scenario: Valid token grants access
- **WHEN** a request includes a valid JWT token in the Authorization header
- **THEN** the system SHALL identify the user from the token claims and scope all data access to that user's data only

#### Scenario: Missing token is rejected
- **WHEN** a request does not include an Authorization header
- **THEN** the system SHALL return a 401 Unauthorized response

#### Scenario: Expired token is rejected
- **WHEN** a request includes a JWT token that is past its expiry time
- **THEN** the system SHALL return a 401 Unauthorized response

#### Scenario: Malformed token is rejected
- **WHEN** a request includes a JWT token that is malformed or tampered with
- **THEN** the system SHALL return a 401 Unauthorized response

## Validation Targets

- REQ-AUTH-001 is met when all 5 scenarios pass
- REQ-AUTH-002 is met when all 3 scenarios pass
- REQ-AUTH-003 is met when all 4 scenarios pass
