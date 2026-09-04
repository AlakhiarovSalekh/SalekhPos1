# Contributing to SalekhPos

SalekhPos is a commercial-grade retail platform. Contributions must respect the architectural, security, and quality rules in `docs/architecture/ARCHITECTURE.md` and the project's ADRs under `docs/decisions/`.

## Required reading before contributing

1. `docs/architecture/ARCHITECTURE.md` — overall system architecture and boundaries.
2. `docs/architecture/OVERVIEW.md` — module responsibilities and dependency rules.
3. `docs/development/IMPLEMENTATION_STATUS.md` — current phase, in-progress work, known issues.
4. ADRs in `docs/decisions/` — accepted architectural decisions.
5. `docs/security/SECURITY.md` — security principles and threat model.

## Workflow

1. Branch from `main` using one of:
   - `feature/<short-scope>`
   - `fix/<short-scope>`
   - `security/<short-scope>`
   - `perf/<short-scope>`
2. Use commit prefixes: `feat:`, `fix:`, `refactor:`, `perf:`, `security:`, `test:`, `docs:`, `build:`, `ci:`, `chore:`.
3. Keep changes scoped. One logical change per commit.
4. Before pushing:
   - `git status` — confirm no stray edits.
   - `git diff` — review what is about to be committed.
   - Run the build, unit tests, and any relevant integration tests.
   - Run linters and the security scan.
5. Open a pull request with:
   - A clear summary of the change.
   - The phase / ADR it implements.
   - Test evidence (commands run, results).
   - Any migration, configuration, or operational impact.

## Quality gates

A change is mergeable only when:

- Build passes.
- Unit and integration tests pass.
- Linters and formatters pass.
- No new security warnings from `dotnet`/`npm audit`.
- Documentation affected by the change has been updated in the same commit.
- Architecture boundaries are preserved (no new direct dependencies from Domain to Infrastructure, no business rules in controllers, no client-side authorization, no hard-coded secrets).

## Architectural boundaries

- `SalekhPos.Domain` must not depend on `Infrastructure`, `Application`, EF Core, ASP.NET Core, or any external SDK.
- `SalekhPos.Application` orchestrates use cases and may depend on Domain and abstractions only.
- `SalekhPos.Infrastructure` implements abstractions defined in `Application` and persists Domain aggregates.
- `SalekhPos.Api` is HTTP-only: routing, DTO binding, authentication pipeline integration, Problem Details, rate limiting, health endpoints. **No business rules.**
- Web, Desktop, and Mobile clients are untrusted. Authorization, prices, taxes, totals, stock, tenant, and permission decisions live on the server.

## Don't

- Do not delete tests to make CI pass.
- Do not weaken authentication, authorization, or tenant isolation.
- Do not introduce hard-coded secrets, tokens, or connection strings.
- Do not put financial / inventory / tenant logic on the client.
- Do not invent API shapes for external providers — verify against authoritative documentation first.
- Do not change the technology stack (ASP.NET Core, PostgreSQL, React, .NET MAUI, SignalR) without an ADR and impact analysis.

## Reporting security issues

See `docs/security/SECURITY.md`. Do not open public issues for suspected vulnerabilities.
