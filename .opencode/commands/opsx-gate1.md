---
description: Run Gate 1 review on a change — validates Context, Contract, Architecture, Tech Stack, Commit, and Metrics (Experimental)
---

Run Gate 1 review on an OpenSpec change.

**Input**: Optionally specify a change name (e.g., `/opsx-gate1 add-auth`). If omitted, infer from context or auto-select.

**Steps**

1. **Select the change**

   If a name is provided, use it. Otherwise:
   - Infer from conversation context if the user mentioned a change
   - Auto-select if only one active change exists
   - If ambiguous, run `openspec list --json` and use the **AskUserQuestion tool** to let the user select

   Always announce: "Gate 1 review for change: <name>"

2. **Load artifacts**

   ```bash
   openspec status --change "<name>" --json
   ```

   Parse the JSON to get `changeRoot` and artifact paths for: proposal, specs, design, tasks.

   Read all artifact files:
   - proposal.md
   - design.md
   - All spec files under specs/ (glob `specs/**/*.md`)
   - tasks.md
   - metrics.md (if exists)

3. **Run Gate 1 checks**

   Run each check and record pass/fail with evidence.

   ### Check 1: Context Complete

   Verify:
   - `proposal.md` has: Project Vision, Problem Statement, Goals, Non-Goals, Scope (In/Out), Stakeholders, Assumptions, Risks
   - Each spec file has a `## Capability Context` section (or equivalent) describing purpose, preconditions, and system boundary
   - `design.md` has a `## Context` section with background

   **PASS if**: All three documents have adequate context. **FAIL if**: Any section is missing.

   ### Check 2: Contract Complete

   Verify:
   - Each spec file has a `## Contracts` section (or equivalent) with request/response JSON schemas
   - All endpoints in the spec have defined HTTP methods, paths, request bodies, response formats, and status codes
   - Error response format is defined

   **PASS if**: Every spec has complete contract definitions. **FAIL if**: Any endpoint or response is undocumented.

   ### Check 3: Architecture Defined

   Verify:
   - `design.md` has a `## System Architecture` section with a diagram or description showing layers/components
   - `design.md` has a `## Folder Structure` section showing the project directory layout
   - `design.md` has a `## Component Diagram` or equivalent showing component relationships
   - `design.md` has an `## API Endpoints` section or route configuration

   **PASS if**: Architecture, folder structure, component relationships, and routing are all defined.

   ### Check 4: Tech Stack Defined

   Verify:
   - `proposal.md` mentions all technologies used (backend framework, frontend framework, database, ORM, key libraries)
   - `design.md` has a `## Decisions` table or equivalent documenting key technology choices with rationale
   - Database technology is specified (PostgreSQL, SQL Server, etc.)

   **PASS if**: Full tech stack is documented with rationale for key decisions.

   ### Check 5: Specification Committed

   Verify:
   - Run `git log --oneline -5` and check for commits containing spec files
   - If no git repo: run `git status` to check if `git init` has been run
   - Spec files should exist and be under version control

   **PASS if**: Spec files are committed to git. **FAIL if**: No git repo or uncommitted spec files.

   ### Check 6: Baseline Metrics Recorded

   Verify:
   - `metrics.md` exists in the change directory (or equivalent metrics record)
   - It contains: requirement count, scenario count, task count, business rule count
   - It is committed to git

   **PASS if**: `metrics.md` exists, has counts, and is committed.

4. **Report results**

   Display results in this format:

   ```
   ## Gate 1 Review: <change-name>

   | Criterion | Result | Details |
   |---|---|---|
   | Context Complete | ✅ / ❌ | <brief evidence or gap> |
   | Contract Complete | ✅ / ❌ | <brief evidence or gap> |
   | Architecture Defined | ✅ / ❌ | <brief evidence or gap> |
   | Tech Stack Defined | ✅ / ❌ | <brief evidence or gap> |
   | Specification committed | ✅ / ❌ | <commit hash or gap> |
   | Baseline metrics recorded | ✅ / ❌ | <file found or gap> |

   **Verdict**: PASS / FAIL
   ```

5. **Fix failures or report summary**

   If any check failed:
   - For each failure, explain what is missing and why
   - Suggest specific improvements
   - Use the **AskUserQuestion tool** to ask: "Should I fix the failing checks now?"

   If the user agrees, fix each failure:
   - **Context**: Add missing sections to the relevant artifact files
   - **Contract**: Add Contracts sections with JSON schemas to spec files
   - **Architecture**: Add architecture diagram, folder structure to design.md
   - **Tech Stack**: Add missing technology documentation
   - **Commit**: Run `git init` → `git add` → `git commit` to version-control the specs
   - **Metrics**: Create `metrics.md` with requirement/scenario/task counts, then commit

   After fixing, re-run checks and report updated verdict.

6. **If all pass**

   ```
   ## Gate 1 — PASS ✅

   All 6 criteria verified. Ready to proceed.
   ```
