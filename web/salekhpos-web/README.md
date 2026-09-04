# SalekhPos Web (React + TypeScript + Vite)

The web client for owners, managers, and administrators.

- **Stack:** React 18, TypeScript, Vite 5, React Query, React Router, i18next, SignalR client, Zod, React Hook Form.
- **i18n:** Georgian (`ka`), English (`en`), Azerbaijani (`az`). Default currency display: GEL.
- **Architecture:** feature-folder layout under `src/features/*`, design-system primitives under `src/components/ui`, services under `src/services/*`.
- **Authorization:** server-side is authoritative. Frontend permission checks are UX only.

## Develop

```bash
cd web/salekhpos-web
npm install
npm run dev
```

The Vite dev server runs on `:5173` and proxies `/api/*` and `/hubs/*` to `http://localhost:8080`.

## Build

```bash
npm run build
```

Output goes to `dist/`. Production builds are served through the Caddy reverse proxy in the Docker dev environment.

## Test

```bash
npm run lint
npm run typecheck
npm run test
npm run test:e2e
```

## Configuration

| Env | Default | Purpose |
|---|---|---|
| `VITE_API_BASE_URL` | `http://localhost:8080` | Public base URL of the API. |
| `VITE_HUB_BASE_URL` | `http://localhost:8080` | Base URL of the SignalR hubs. |
| `VITE_DEFAULT_LOCALE` | `en` | Initial locale. Must be one of `ka`, `en`, `az`. |
| `VITE_ENABLE_DEVTOOLS` | `false` | Optional dev-only flag. |

Secrets are NEVER exposed to the web client.
