# Global Implementation Rules

These rules apply to **every implementation task** unless explicitly overridden by the approved specification.

## 1. Specification First

- Follow the approved Proposal, Requirements, Design, and Task documents.
- Never make assumptions that are not documented.
- If any requirement is ambiguous, stop and ask for clarification before implementing.

## 2. Scope Control

- Implement **only** the assigned task.
- Do not implement future tasks.
- Do not modify unrelated files.
- Do not introduce undocumented features or optimizations.

## 3. Respect Dependencies

- Verify that all prerequisite tasks are completed.
- If a dependency is missing, stop and report it.
- Do not work around incomplete dependencies.

## 4. Code Quality

- Write clean, readable, and maintainable code.
- Follow SOLID principles where applicable.
- Keep methods small and focused.
- Avoid duplicated code.
- Use meaningful naming conventions.

## 5. Project Standards

- Follow the approved project architecture.
- Follow the approved folder structure.
- Follow established naming conventions.
- Use the approved technology stack only.

## 6. Safety Rules

- Never remove existing functionality unless required by the specification.
- Never delete files unrelated to the current task.
- Never commit secrets, passwords, tokens, or connection strings.
- Never introduce breaking changes outside the task scope.

## 7. Validation

Before completing the task:

- Ensure the project builds successfully.
- Resolve any compilation errors introduced by the task.
- Verify that the implementation satisfies the Definition of Done.
- Verify that the implementation satisfies the related Acceptance Criteria.

## 8. Traceability

For every implementation, identify:

- Task ID
- Related Requirement(s)
- Related Design Section(s)

Every code change must be traceable back to the approved specification.

## 9. Completion Report

At the end of every task, provide:

1. Task ID
2. Requirement(s) implemented
3. Files created
4. Files modified
5. Commands executed
6. Summary of changes
7. Definition of Done verification
8. Acceptance Criteria verification
9. Known limitations (if any)
10. Confirmation that no future tasks were implemented

## 10. Approval Gate

After completing the task:

- Stop immediately.
- Wait for explicit approval before starting the next task.
- Do not continue automatically.

---

# Mandatory Principles

- Specification before Code
- No Specification Drift
- One Task at a Time
- Small, Reviewable Changes
- No Hidden Assumptions
- No Scope Creep
- Every Change Must Be Traceable
- Every Task Must Be Verifiable
- Every Task Must Be Reversible if Necessary
- Human Approval Required Before Proceeding
