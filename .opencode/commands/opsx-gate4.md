---
description: Run Gate 4 review on a change — validates Deployment Drill, Regression Tests, Traceability, Validation Targets, Root-Cause Analysis, Security Verification, and Retrospective (Experimental)
---

Run Gate 4 review on an OpenSpec change.

**Input**: Optionally specify a change name (e.g., `/opsx-gate4 todo-app`). If omitted, infer from context or auto-select.

**Steps**

1. **Select the change**

   If a name is provided, use it. Otherwise infer from context or list available changes if ambiguous.

   Announce: "Gate 4 review for change: <name>"

2. **Load artifacts and project state**

   ```bash
   openspec status --change "<name>" --json
   ```

   Read all artifacts and deliverables:
   - All spec files (`specs/**/*.md`)
   - `design.md`
   - `tasks.md`
   - `metrics.md`
   - Mid-point report, retrospective report, contribution report
   - Source code: glob for `*.cs`, `*.ts`, `*.html`, `*.json`, `*.yaml`, `*.yml`
   - Test files: glob for `*Test*`, `*Spec*`, `*.spec.*`
   - Deployment/config files: glob for `Dockerfile`, `docker-compose*`, `*.ps1`, `*.sh`, `*.yaml`, `*.yml`
   - Any CI/CD configuration files (`.github/`, `azure-pipelines.yml`, `.gitlab-ci.yml`)

3. **Run Gate 4 checks**

   ### Check 1: Deployment Drill Completed

   Verify a deployment drill has been performed:
   - Look for deployment artifacts:
     - `Dockerfile` or `docker-compose.yml` in project root
     - Deployment scripts: `deploy.ps1`, `deploy.sh`, `*.ps1`, `*.sh`
     - CI/CD configuration: `.github/workflows/*.yml`, `azure-pipelines.yml`, `.gitlab-ci.yml`
   - Look for deployment documentation:
     - `deployment.md`, `deploy.md`, `ops-guide.md`, `runbook.md`
     - In project root, `openspec/changes/<name>/`, or `docs/`
   - The deployment drill evidence should document:
     - Environment where drill was run (local, staging)
     - Steps executed (build, migrate DB, start services, verify health)
     - Outcome (passed / failed with notes)
   - Check for startup configuration: `Program.cs`, `appsettings.json`, connection strings, environment variable mappings

   **PASS if**: Deployment documentation or scripts exist AND a deployment drill was successfully executed (documented outcome). **FAIL if**: No deployment artifacts or no drill evidence.

   ### Check 2: Regression Tests Pass

   Verify regression tests exist and pass:
   - Search for test projects and test files:
     - Backend: `*Tests*.csproj`, files in `*Tests/` directories
     - Frontend: `*.spec.ts` files
   - If test project exists, attempt to run tests:
     ```bash
     dotnet test <test-project-path> --no-restore 2>&1
     ```
     Or for frontend:
     ```bash
     cd <frontend-dir> && npx ng test --watch=false --browsers=ChromeHeadless 2>&1
     ```
   - If tests can't be run (missing dependencies, no test project), check for:
     - Test plan document describing manual regression tests
     - Evidence of manual regression test execution (checklist with pass/fail)
   - Count: total tests, passed, failed, skipped

   **PASS if**: All regression tests pass (or manual test checklist is fully green). **FAIL if**: Any test fails, or no regression testing evidence exists.

   ### Check 3: Spec → Code → Test Traceability Demonstrated

   Verify full traceability chain:
   - Create a traceability matrix by reading:
     - Spec files: extract all requirement IDs (e.g., `REQ-AUTH-001`, `REQ-TODO-001`)
     - Tasks file: extract which tasks reference which requirements
     - Source code: search for requirement IDs in comments or test names
     - Test files: search for requirement IDs or matching scenario descriptions
   - For each requirement, verify:
     - ✅ Spec has the requirement defined with scenarios
     - ✅ Code implements the requirement (file exists, endpoints match)
     - ✅ Tests cover the requirement (test file or test case found)
   - Report: "X/Y requirements have full spec → code → test traceability"

   **PASS if**: >= 90% of requirements have full spec → code → test traceability. **FAIL if**: Less than 90%, or any critical requirement lacks traceability.

   ### Check 4: All Validation Targets Achieved

   Verify every validation target from the specs has been met:
   - Read Validation Targets sections from all spec files
   - For each validation target (e.g., "REQ-TODO-001 is met when all 5 scenarios pass"):
     - Verify the implementation covers all scenarios for that requirement
     - Check that tests exist for each scenario
     - Determine if the requirement is fully implemented
   - Look for a `validation-report.md` or `test-results.md` in the change directory
   - Report: "X/Y validation targets achieved"

   **PASS if**: All validation targets are achieved. **FAIL if**: Any validation target is unmet or untested.

   ### Check 5: Root-Cause Analysis Completed Successfully

   Verify RCA has been performed for any issues encountered:
   - Look for RCA documentation:
     - `rca.md`, `root-cause.md`, `postmortem.md`, `incident-report.md`
     - In `openspec/changes/<name>/` or project root
   - If issues were encountered during development (bugs, regressions, build failures):
     - RCA should document: What happened, Root cause, Fix applied, How to prevent
   - If no issues were encountered at all:
     - A brief statement like "No issues requiring RCA during development" is acceptable

   **PASS if**: RCA exists for all issues encountered, or a statement confirms no issues needed RCA. **FAIL if**: Known issues lack RCA documentation.

   ### Check 6: Final Security Verification Passed

   Perform final security verification:
   - Re-check for secrets in repository (re-run from Gate 3):
     - `git grep -i "password\s*=" -- '*.cs' '*.ts' '*.json' '*.yaml' '*.yml' '*.config'`
     - `git grep -n "ConnectionString\|connectionstring" -- '*.cs' '*.json' '*.config'`
     - `git grep -n "Secret\|secret\|JwtKey\|SigningKey\|ApiKey" -- '*.cs' '*.json' '*.csproj'`
   - Verify auth is applied to all protected endpoints:
     - Check controllers for `[Authorize]` attribute usage
     - Verify AuthInterceptor is configured in frontend
   - Check input validation:
     - Backend: `[Required]`, `[StringLength]`, `[MinLength]` data annotations on DTOs
     - Frontend: form validators (required, minlength, maxlength)
   - Check error handling doesn't leak sensitive info:
     - 404 for another user's data (no information leakage)
     - 401 without revealing which credential was wrong
   - If a security scan tool is available (e.g., `dotnet list package --vulnerable`), run it:
     ```bash
     dotnet list <project>.csproj package --vulnerable 2>&1
     ```

   **PASS if**: No secrets in repo, auth on all protected endpoints, validation in place, no info leakage. **FAIL if**: Any security finding remains.

   ### Check 7: Retrospective Completed

   Verify a retrospective has been conducted:
   - Look for retrospective documentation:
     - `retrospective.md`, `retro.md`, `lessons-learned.md`
     - In `openspec/changes/<name>/` or project root
   - The retrospective should cover:
     - What went well
     - What could be improved
     - Action items for future changes
     - Lessons learned about the Spec-Driven Development process itself

   **PASS if**: Retrospective document exists with the 4 required sections. **FAIL if**: No retrospective found.

4. **Report results**

   ```
   ## Gate 4 Review: <change-name>

   | Criterion | Result | Details |
   |---|---|---|
   | Deployment drill completed | ✅ / ❌ | <drill evidence or gap> |
   | Regression tests pass | ✅ / ❌ | <pass rate / gap> |
   | Spec → Code → Test traceability | ✅ / ❌ | <coverage % / gaps> |
   | All validation targets achieved | ✅ / ❌ | <targets met / gaps> |
   | Root-cause analysis completed | ✅ / ❌ | <RCA found or gap> |
   | Final security verification passed | ✅ / ❌ | <findings or clean> |
   | Retrospective completed | ✅ / ❌ | <retro found or gap> |

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
   ## Gate 4 — PASS ✅

   All 7 criteria verified. Change is production-ready.
   ```
