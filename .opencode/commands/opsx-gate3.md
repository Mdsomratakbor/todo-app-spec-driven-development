---
description: Run Gate 3 review on a change — validates Validation Targets, Test Coverage, Non-regression, Security, Contributions, Logging, Secrets, and Spec Sync (Experimental)
---

Run Gate 3 review on an OpenSpec change.

**Input**: Optionally specify a change name (e.g., `/opsx-gate3 todo-app`). If omitted, infer from context or auto-select.

**Steps**

1. **Select the change**

   If a name is provided, use it. Otherwise infer from context or list available changes if ambiguous.

   Announce: "Gate 3 review for change: <name>"

2. **Load artifacts and project**

   ```bash
   openspec status --change "<name>" --json
   ```

   Read all artifacts:
   - All spec files (`specs/**/*.md`)
   - `design.md`
   - `tasks.md`
   - `metrics.md`
   - Mid-point report (if exists)
   - Source code: glob for `*.cs`, `*.ts`, `*.html`, `*.json`, `*.yaml`, `*.yml`, `*.config`
   - Test files: glob for `*Test*`, `*Spec*`, `*test*`, `*.spec.*`

3. **Run Gate 3 checks**

   Run each check and record pass/fail with evidence.

   ### Check 1: Validation Targets Complete

   Verify each spec file has a `## Validation Targets` section (or equivalent):
   - Each requirement ID maps to a pass condition (e.g., "REQ-TODO-001 is met when all 5 scenarios pass")
   - Check that every requirement defined in the spec has a corresponding validation target entry
   - Validation targets should be quantitative (scenario counts) not qualitative ("works well")

   **PASS if**: Every spec has a Validation Targets section covering all its requirements. **FAIL if**: Any requirement lacks a validation target.

   ### Check 2: Every Scenario Mapped to Tests

   Verify traceability from spec scenarios to test code:
   - List all scenarios from all spec files (count and collect their descriptions)
   - Search for test files in the project:
     - Backend: look for `*Tests*.cs`, files in `*Tests/` directories, or test projects
     - Frontend: look for `*.spec.ts` files, files in `__tests__/` directories
   - For each scenario, check if there is a corresponding test:
     - Match by requirement ID (e.g., `REQ-AUTH-001` appearing in a test file)
     - Match by keywords from the scenario description (e.g., "duplicate username", "empty title")
   - Count total scenarios and total found tests. Report coverage ratio.
   - Flag any scenario with no matching test.

   **PASS if**: >= 80% of scenarios have a corresponding test. **FAIL if**: Less than 80% coverage or critical security scenarios untested.

   ### Check 3: Non-Regression Plan Completed

   Verify existence of a non-regression test plan:
   - Look for `regression-plan.md`, `non-regression.md`, `test-plan.md`, or similar in:
     - `openspec/changes/<name>/`
     - Project root
     - `docs/` directory
   - The plan should document:
     - Which existing features/functions could be affected by this change
     - Which regression tests should be run before release
     - Manual test checklist or automated test suite reference

   **PASS if**: Non-regression plan exists with affected areas and test checklist. **FAIL if**: No plan found.

   ### Check 4: Security Scan Passes

   Verify security posture of the codebase:
   - Search for hardcoded secrets or credentials:
     - Run `git grep -i "password\s*=" -- '*.cs' '*.ts' '*.json' '*.yaml' '*.yml' '*.config' '*.env*'`
     - Look for connection strings with embedded passwords in source code
     - Check for hardcoded API keys, tokens, or JWT secrets
   - Check if security-related dependencies are present:
     - Backend: `Microsoft.AspNetCore.Authentication.JwtBearer`, ASP.NET Core Identity
     - Frontend: AuthInterceptor handling 401 responses
   - Verify authentication/authorization is applied to protected endpoints
   - Check if HTTPS/SSL is configured or noted as a requirement
   - Check for SQL injection protection (EF Core parameterized queries by default)

   **PASS if**: No secrets found in code, auth is implemented for protected endpoints, security dependencies are present. **FAIL if**: Hardcoded secrets found or protected endpoints lack auth.

   ### Check 5: AI vs Human Contribution Documented

   Verify contribution documentation exists:
   - Look for `contributions.md`, `ai-contributions.md`, `humans.md`, or similar in:
     - `openspec/changes/<name>/`
     - Project root
     - `.opencode/`
   - The document should clearly state:
     - Which parts of the code were AI-generated
     - Which parts were human-written or human-reviewed
     - Any modifications made to AI output after generation

   **PASS if**: Contribution documentation exists with clear AI/human boundaries. **FAIL if**: No documentation found.

   ### Check 6: Structured Logging Implemented

   Verify structured logging is in place:
   - Backend:
     - Search for `ILogger<T>` injections in controllers or services
     - Search for structured log messages (e.g., `LogInformation("User {UserId} created todo {TodoId}", ...)`)
     - Verify logging is configured in `Program.cs` or `appsettings.json`
   - Frontend:
     - Search for `console.log`, `console.error`, or a logging service
     - Verify error logging in services (catchError blocks, auth interceptor)

   **PASS if**: Backend has ILogger injections with structured templates. Frontend has error logging in services. **FAIL if**: No logging implementation found.

   ### Check 7: No Secrets in Repository

   Verify no secrets are committed:
   - Check `.gitignore` for excluded files: `appsettings.*.local`, `*.env`, `secrets*`, `user-secrets*`, `*.key`
   - Search for potential secrets in committed files:
     - Run `git grep -n "ConnectionString\|connectionstring\|connString" -- '*.cs' '*.json' '*.config'`
     - Check `appsettings.json` for placeholder vs real values
     - Run `git grep -n "Secret\|secret\|JwtKey\|SigningKey\|ApiKey" -- '*.cs' '*.json' '*.csproj'`
   - Check if user secrets or environment variables are used for sensitive config
   - Verify no `.env` files are committed

   **PASS if**: No secrets in committed files. Connection strings use placeholders or environment variables. User secrets configured for development. `.gitignore` excludes sensitive files. **FAIL if**: Real secrets found in committed files or no `.gitignore` for sensitive files.

   ### Check 8: Spec Synchronized with Implementation

   Verify spec matches current implementation:
   - Compare API endpoints defined in spec Contracts sections against actual controller code:
     - For each endpoint in spec, check that a corresponding controller action exists
     - Check route templates match (e.g., `[HttpGet("{id}")]` vs spec's `GET /api/v1/todos/{id}`)
   - Compare response shapes: do the actual return types match the JSON schemas in Contracts?
   - Compare field names and constraints: do model properties match spec definitions?
   - Check that business rules from spec are enforced in service code
   - Flag any implementation that exists without a spec requirement (scope creep)
   - Flag any spec requirement that has no implementation

   **PASS if**: All implemented endpoints match spec contracts. No unexplained scope creep. All spec requirements have corresponding implementation. **FAIL if**: Endpoints or behavior differ from spec without documentation.

4. **Report results**

   Display results in this format:

   ```
   ## Gate 3 Review: <change-name>

   | Criterion | Result | Details |
   |---|---|---|
   | Validation Targets complete | ✅ / ❌ | <requirements covered / gaps> |
   | Every scenario mapped to tests | ✅ / ❌ | <coverage % / untested scenarios> |
   | Non-regression plan completed | ✅ / ❌ | <plan found or missing> |
   | Security scan passes | ✅ / ❌ | <findings or clean> |
   | AI vs Human contribution documented | ✅ / ❌ | <file found or missing> |
   | Structured logging implemented | ✅ / ❌ | <ILogger found or not> |
   | No secrets in repository | ✅ / ❌ | <findings or clean> |
   | Spec synchronized with implementation | ✅ / ❌ | <drift detected or clean> |

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
   ## Gate 3 — PASS ✅

   All 8 criteria verified. Change is validated and hardened.
   ```
