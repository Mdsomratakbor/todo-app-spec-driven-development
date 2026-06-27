# Retrospective — add-task-priorities

## What Went Well

- All 21 tasks completed in a single implementation pass
- Both backend (entity, config, migration, service, controller) and frontend (models, service, list, form) updated consistently
- All 73 tests pass (67 existing + 6 new), Angular build succeeds with 0 errors
- Spec-driven approach kept scope tightly focused on priority only

## What Could Be Improved

- Gate 3 criteria weren't defined as a standalone checklist file, making verification ambiguous
- Angular Material mat-chip required additional imports; switched to simple span to avoid dependency issues
- Setup sentinel value for EF Core enum default to eliminate migration warning

## Lessons Learned

- **Always check Angular Material imports before using components like mat-chip**: Not all Material components are available by default in newer versions.
- **Enum CLR defaults vs DB defaults need sentinel alignment**: EF Core emits a warning when the CLR default (Low=0) differs from the DB default (Medium). Either match them or add `.HasSentinel()`.
- **Filter/sort params need front-to-back wiring**: Adding a query param requires touching models, service, controller, and UI — easy to miss a layer.

## Action Items

- [ ] Run Gate 3 review after commit
- [ ] Consider adding query param contract tests for the new filters
