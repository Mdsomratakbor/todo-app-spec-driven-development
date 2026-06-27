---
description: Run Gate 2 review on a change — validates Behavior, Constraints, AI Module, Review, Mid-point Report, and Spec-Code Drift (Experimental)
---

Run Gate 2 review on an OpenSpec change.

**Input**: Optionally specify a change name (e.g., `/opsx-gate2 todo-app`). If omitted, infer from context or auto-select.

**Steps**

1. **Select the change**

   If a name is provided, use it. Otherwise infer from context or list available changes if ambiguous.

   Announce: "Gate 2 review for change: <name>"

2. **Load artifacts and implementation**

   ```bash
   openspec status --change "<name>" --json
   ```

   Read all artifacts:
   - All spec files (`specs/**/*.md`)
   - `design.md`
   - `tasks.md`
   - `metrics.md` (if exists)
   - Any mid-point report (look for `midpoint-report.md` or similar in the change directory)
   - Existing source code files (glob `src/**/*.*`, `TodoApi/**/*.*`, `TodoApp/**/*.*`, etc.)

3. **Run Gate 2 checks**

   Run each check and record pass/fail with evidence.

   ### Check 1: Behavior Complete (minimum 5 scenarios per capability)

   For each spec file:
   - Count the total scenarios under all requirements
   - Verify each capability has **at least 5 scenarios** across all its requirements
   - Verify scenarios follow proper format: `#### Scenario:` with `**WHEN**` / `**THEN**` clauses

   **PASS if**: Every capability spec has >= 5 scenarios. **FAIL if**: Any spec has fewer than 5.

   ### Check 2: Constraints Complete

   Verify the specification defines constraints beyond functional behavior:
   - Search for constraint-related sections in spec files: Business Rules, Constraints, or equivalent
   - Check for documented constraints:
     - Field length limits (e.g., title max 200, name max 50, password min 6)
     - Security constraints (e.g., password hashing, JWT expiry, data isolation)
     - Behavioural constraints (e.g., no delete if associated data exists, no pagination)
   - Known gaps should be explicitly documented (e.g., `(known gap)` annotations)

   **PASS if**: Key constraints are documented. Known gaps are explicitly marked. **FAIL if**: No constraints section exists or critical constraints are missing without documentation.

   ### Check 3: First AI-Generated Module

   Verify that at least one implementation module exists:
   - Check for generated source files in the project (e.g., `*.cs`, `*.ts`, `*.html`)
   - Check task progress: count `- [x]` completed tasks vs total `- [ ]` tasks in `tasks.md`
   - At least one task should be marked complete

   **PASS if**: At least 1 task is complete with source files created. **FAIL if**: No implementation has started yet.

   ### Check 4: AI Output Reviewed Against Spec

   For the first completed module:
   - Read the implementation files for that task
   - Compare against the requirements and scenarios referenced in the task
   - Verify:
     - All referenced requirement scenarios are covered in code
     - Error handling matches spec (401, 404, 409, 400 status codes as defined)
     - Response format matches the contract (FluentResponse.ApiWrapper envelope)
     - Business rules from the spec are enforced in code
   - Document any discrepancies found

   **PASS if**: Implementation matches spec with no discrepancies. **FAIL if**: Gaps or mismatches found.

   ### Check 5: Mid-Point Report Completed

   Verify existence of a mid-point report:
   - Look for `midpoint-report.md` in the change directory (`openspec/changes/<name>/`)
   - If not found, check the repo root or project root
   - The report should contain:
     - Progress summary (tasks completed / total)
     - What was implemented
     - Any deviations from spec
     - Remaining work

   **PASS if**: Mid-point report exists with progress summary. **FAIL if**: No report found.

   ### Check 6: No Spec-Code Drift

   Verify implementation matches the specification:
   - Pick 1-2 completed tasks and compare their deliverables against the spec
   - For backend tasks: check that API endpoints match the Contracts section (paths, methods, request shapes, response shapes, status codes)
   - For frontend tasks: check that components match the Angular Architecture and route configuration
   - Check that business rules from the spec are enforced in the code
   - Look for any code that implements behavior not specified (scope creep)

   **PASS if**: Completed code matches spec with no unexplained drift. **FAIL if**: Code deviates from spec without documented exception.

4. **Report results**

   Display results in this format:

   ```
   ## Gate 2 Review: <change-name>

   | Criterion | Result | Details |
   |---|---|---|
   | Behavior complete (>=5 scenarios) | ✅ / ❌ | <counts per capability> |
   | Constraints complete | ✅ / ❌ | <constraints documented / gaps> |
   | First AI-generated module | ✅ / ❌ | <completed tasks / source files> |
   | AI output reviewed against spec | ✅ / ❌ | <discrepancies found or none> |
   | Mid-point report completed | ✅ / ❌ | <report found or missing> |
   | No spec-code drift | ✅ / ❌ | <drift detected or clean> |

   **Verdict**: PASS / FAIL
   ```

5. **Fix failures or report summary**

   If any check failed:
   - For each failure, explain what is missing and why
   - Suggest specific improvements
   - Use the **AskUserQuestion tool** to ask: "Should I fix the failing checks now?"

   If the user agrees, fix each failure and re-run checks.

6. **If all pass**

   ```
   ## Gate 2 — PASS ✅

   All 6 criteria verified.
   ```
