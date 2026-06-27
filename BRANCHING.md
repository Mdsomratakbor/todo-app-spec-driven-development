# Branching Convention

```
main ───────────────────────────────────── (stable, release-ready)
  │
  └─── develop ──────────────────────────── (integration, all features merged)
       │
       ├── change/user-auth          Team A: user registration & login
       ├── change/todo-management    Team B: todo CRUD & filters
       └── change/categories         Team C: category management
```

## Branch Lifecycle

| Branch | Purpose | Base Branch | Merges To |
|---|---|---|---|
| `main` | Production-ready. Only merge reviewed, fully gated changes. | — | — |
| `develop` | Integration branch. All completed features merge here. | `main` | `main` |
| `change/<name>` | Feature work. One per OpenSpec change. | `develop` | `develop` |

## Per-Feature Workflow

```mermaid
git checkout develop
git checkout -b change/my-feature

# Phase 1-4: Planning
openspec new change my-feature
# → proposal → specs → design → tasks

# Phase 5-6: Implementation & Validation
# → implement code
/opsx-gate1   # spec complete?
/opsx-gate2   # implementation progressing?
/opsx-gate3   # validation passing?

# Phase 7: Close
git checkout develop
git merge --squash change/my-feature
git branch -d change/my-feature

# Release to main
git checkout main
git merge develop
/opsx-archive my-feature   # sync delta specs to main specs
```

## Gate Triggers

| Gate | Trigger | Who |
|---|---|---|
| Gate 1 | Before any code is written | Feature author |
| Gate 2 | After first module implemented | Feature author |
| Gate 3 | Before merging to develop | Feature author + reviewer |
| Gate 4 | Before merging to main | All stakeholders |

## Conflict Resolution

- Spec files (`openspec/changes/<name>/`) are isolated per branch — no conflicts
- Source files may conflict when two branches modify the same file
- Resolve conflicts on `develop` when merging feature branches
- Main spec files (`openspec/specs/`) are only touched by `/opsx-archive` — single-writer

## Remote Setup

When the remote is accessible:
```bash
git push -u origin develop
git push -u origin main
```
