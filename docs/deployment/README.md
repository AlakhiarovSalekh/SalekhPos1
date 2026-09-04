# Deployment

Production deployment topology:

```text
Internet
  -> DNS
  -> HTTPS / WAF / Reverse Proxy (Caddy or equivalent)
  -> SalekhPos.Api (ASP.NET Core, container)
  -> SalekhPos.Worker (background, container)
  -> Web static assets (CDN or reverse-proxy served)
  -> Private network
     -> PostgreSQL 16
     -> Redis 7
     -> Object storage
```

## Environments

- **Development** — local, fast feedback, isolated data.
- **Staging** — production-like, used for smoke tests and pre-release verification.
- **Production** — restricted access, encrypted backups, monitored.

## CI/CD

The pipeline runs:

```text
Checkout
 -> Restore
 -> Build
 -> Unit tests
 -> Integration tests
 -> Lint / format
 -> Security scans
 -> Artifact build
 -> Container scan
 -> Staging deploy
 -> Smoke tests
 -> Approval
 -> Production deploy
 -> Health checks
 -> Smoke verification
```

## Containers

- Versioned images, not `latest` as deployment identity.
- Multi-stage builds, non-root users, minimal base images.
- No embedded secrets; secrets are injected at runtime.
- Image scanning in CI.

## Backups

- Automated, encrypted, retention policy, **verified by restore**.
- Point-in-time recovery where supported.
- Recovery procedures documented in `docs/operations/`.

## Rollout

- Database migrations: forward-compatible, with impact analysis.
- Application: blue/green or rolling where possible.
- Health checks gate traffic; smoke tests verify post-deploy.
