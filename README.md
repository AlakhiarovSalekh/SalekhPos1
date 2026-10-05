# SalekhPos — Earlier Platform Implementation

[![.NET](https://img.shields.io/badge/.NET-Backend-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-Web-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Database-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![License](https://img.shields.io/github/license/AlakhiarovSalekh/SalekhPos1)](LICENSE)
[![Stars](https://img.shields.io/github/stars/AlakhiarovSalekh/SalekhPos1?style=social)](https://github.com/AlakhiarovSalekh/SalekhPos1/stargazers)

An earlier implementation track for **SalekhPos**, a multi-tenant retail point-of-sale and business-management platform.

For the newer active public repository and current architecture, see **[AlakhiarovSalekh/SalekhPos](https://github.com/AlakhiarovSalekh/SalekhPos)**.

## Platform Scope

| Component | Stack | Role |
|---|---|---|
| Web | React, TypeScript, Vite | Business/management UI |
| Desktop POS | .NET | Cashier terminal and local-first desktop work |
| Mobile | .NET MAUI | Owner/manager mobile experience |
| Backend | ASP.NET Core, EF Core, PostgreSQL, SignalR | API, realtime communication, integrations |
| Workers | .NET background services | Reconciliation, reports, webhooks, fiscal work |
| Database | PostgreSQL | Central business data |
| Cache | Redis | Coordination and short-lived state |

## Repository Layout

```text
backend/
web/
desktop/
mobile/
infrastructure/
scripts/
tests/
docs/
```

## Status

This repository represents an early implementation stage and is useful for reviewing the project's earlier architecture, scaffolding, and design direction. It should not be treated as the current production-ready SalekhPos implementation.

## Development

```bash
# Backend
cd backend
dotnet build
dotnet test

# Web
cd web/salekhpos-web
npm install
npm run dev
```

Infrastructure and environment requirements are documented inside the repository.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## Current Project

➡️ **[SalekhPos](https://github.com/AlakhiarovSalekh/SalekhPos)** — current public repository.

## Author

**Salekh Alakhiarov** · [GitHub](https://github.com/AlakhiarovSalekh)

## License

See [LICENSE](LICENSE).
