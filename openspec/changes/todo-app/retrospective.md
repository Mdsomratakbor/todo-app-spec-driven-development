# Retrospective — todo-app

**Change:** todo-app
**Date:** 2026-06-27
**Participants:** AI (opencode agent) + Human user

## What Went Well

- **Spec-first workflow**: Writing proposal → specs → design → tasks before any code ensured clear direction and reduced ambiguity during implementation. Each phase built on the previous one naturally.
- **Gate reviews caught issues early**: Gate 2 and 3 reviews identified gaps (missing logging, constraint documentation) before they became entrenched.
- **Test coverage**: 67 unit tests across 9 test files with 91.5% scenario coverage provided confidence during refactoring and additions.
- **Structured logging**: Adding `ILogger<T>` with structured templates from the start made debugging and verification straightforward.
- **Consistent response format**: FluentResponse.ApiWrapper envelope across all endpoints simplified frontend error handling.
- **Traceability**: Every code change was traceable back to a specific requirement and task, making reviews faster and more reliable.

## What Could Be Improved

- **Frontend tests**: Only 1 Angular test exists (default scaffold). Future changes should include component and service tests for the frontend.
- **Deployment artifacts**: No Dockerfile, docker-compose, or CI/CD configuration. While out of scope for this learning project, adding these would improve production readiness.
- **E2E testing**: Tasks 10.1–10.4 describe manual E2E tests that were not formally executed. Adding automated E2E tests (e.g., Playwright or Cypress) would strengthen validation.
- **Environmental configuration**: Connection string and JWT key use placeholder/secret management, but no documentation exists for developers setting up the project from scratch.

## Action Items

| Action | Priority | Owner |
|---|---|---|
| Add frontend unit tests for AuthService, TodoService, CategoryService | Medium | Future change |
| Create docker-compose.yml for local development with PostgreSQL | Low | Future change |
| Document local setup steps (README) for new developers | Medium | Future change |
| Execute and document E2E manual tests (Tasks 10.1–10.4) | High | Before close |
| Add GitHub Actions CI pipeline (build + test) | Low | Future change |

## Lessons Learned

- **Spec-driven development reduces rework**: Having the full specification approved before coding prevented scope creep and kept implementation focused. The spec acted as a single source of truth throughout.
- **Gate reviews are most valuable early**: Gate 1 and 2 caught structural issues. Later gates focused more on verification. The investment in early gates paid off in fewer late-stage surprises.
- **AI-assisted development works well with clear boundaries**: The combination of AI-generated code and human review/approval at each task was effective. The GLOBAL_RULES.md (spec-first, no scope creep, one task at a time) was essential for keeping AI on track.
- **Midpoint reports keep stakeholders aligned**: Periodic progress snapshots helped maintain visibility of completion status, deviations, and remaining work.
- **Documenting deviations immediately prevents drift**: The Swagger addition was documented as a deviation the moment it was introduced, which kept the Gate 3 review clean and avoided confusion about what was/wasn't in the spec.
