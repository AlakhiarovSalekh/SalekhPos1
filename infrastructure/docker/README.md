# SalekhPos Docker — local development

This directory contains the local-development Docker images and compose file.

## Run the local stack

```bash
cp .env.example .env
docker compose -f infrastructure/docker/docker-compose.yml up
```

Services:

- `postgres` — PostgreSQL 16, internal-only.
- `redis` — Redis 7, internal-only.
- `backend` — ASP.NET Core API, internal-only.
- `worker` — .NET background service, internal-only.
- `web` — Vite-built static assets served by unprivileged nginx.
- `caddy` — Reverse proxy. Public surface.

URLs:

- Web: <http://localhost:8080>
- API: <http://localhost:8080/api/v1/>
- Health: <http://localhost:8080/health/live>, <http://localhost:8080/health/ready>
- SignalR: `ws://localhost:8080/hubs/v1/`

## Production notes

The compose file is for local development only. Production:

- Uses versioned images (no `latest` as deployment identity).
- Uses a managed reverse proxy with real certificates and a WAF.
- Runs the database, Redis, and workers on a private network.
- Uses a managed secret store; no secrets in images.
- Performs image scanning in CI before deployment.
