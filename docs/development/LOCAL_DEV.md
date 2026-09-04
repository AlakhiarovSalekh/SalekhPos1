# Local development

## Toolchain

| Tool | Version | Install |
|---|---|---|
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Node.js | 20 LTS or 24 | https://nodejs.org/ |
| Docker Desktop | 4.x | https://www.docker.com/products/docker-desktop/ |
| Git | 2.40+ | https://git-scm.com/ |

The dev environment expects a `.env` file at the repo root (copy from `.env.example`). Secrets must not be committed.

## Run the full stack with Docker

```bash
cp .env.example .env
docker compose -f infrastructure/docker/docker-compose.yml up
```

This brings up:

- PostgreSQL 16 (port 5432, internal to compose)
- Redis 7 (port 6379, internal to compose)
- Caddy reverse proxy (port 8080 -> 8443 with self-signed dev cert)
- `backend` (ASP.NET Core API, internal port 8080)
- `worker` (.NET background service)
- `web` (Vite dev server, port 5173)

## Backend only

```bash
cd backend
dotnet restore
dotnet build
dotnet test
# Run API
dotnet run --project SalekhPos.Api
```

The API is at `http://localhost:8080`. Health: `/health/live`, `/health/ready`.

## Web only

```bash
cd web/salekhpos-web
npm install
npm run dev
```

Vite dev server is at `http://localhost:5173`.

## Migrations

```bash
cd backend
dotnet ef migrations add <Name> --project SalekhPos.Infrastructure --startup-project SalekhPos.Api
dotnet ef database update --project SalekhPos.Infrastructure --startup-project SalekhPos.Api
```

## Tests

- Backend: `dotnet test` (unit, integration, security, performance, fixtures).
- Web: `npm run test` for unit/component; `npm run e2e` for E2E.
