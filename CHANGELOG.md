# Changelog

All notable changes to SalekhPos are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Phase 0 / early Phase 1 — repository foundation
- `added` Repository foundation: README, CONTRIBUTING, CHANGELOG, LICENSE, `.gitignore`, `.editorconfig`.
- `added` Documentation skeleton under `docs/` (architecture, api, database, security, deployment, testing, integrations, decisions, operations, development).
- `added` Initial ADRs: modular monolith, PostgreSQL, offline-first POS, .NET MAUI mobile, SignalR, multi-tenancy, inventory ledger, idempotency.
- `added` `docs/development/IMPLEMENTATION_STATUS.md` with current phase, completed work, pending work, known issues, and toolchain gaps.
- `added` Backend .NET 8 solution scaffold: `SalekhPos.Api`, `SalekhPos.Application`, `SalekhPos.Domain`, `SalekhPos.Infrastructure`, `SalekhPos.Tests`.
- `added` Health endpoints (`/health/live`, `/health/ready`), ProblemDetails, request ID middleware in `SalekhPos.Api`.
- `added` Web scaffold: React 18 + TypeScript + Vite with the feature-folder layout defined in the architecture; i18n for `ka`, `en`, `az`; BusinessShell placeholder.
- `added` Docker dev environment: `docker-compose.yml` with PostgreSQL 16, Redis 7, Caddy reverse proxy, backend, web, and worker services; multi-stage non-root Dockerfiles.
- `added` CI workflow (`.github/workflows/ci.yml`): restore, build, unit tests, lint, security scan, container build (no deploy).

### Notes
- No production release yet. Version `0.0.0` until Phase 24 final quality gate.
- `.NET 8 SDK` is **not installed** on the development machine that produced this commit; the source compiles in `dotnet new`-generated structure, but the user must install the SDK to build locally.
