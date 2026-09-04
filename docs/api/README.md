# API

- **Base version:** `/api/v1/`.
- **Realtime:** SignalR hubs at `/hubs/v1/`.
- **Auth:** Bearer access token in `Authorization`. Refresh-token rotation and revocation in place.
- **Errors:** `application/problem+json` (RFC 7807). No stack traces in production.
- **Idempotency:** `Idempotency-Key` header on financial endpoints.
- **Versioning:** URL-segment versioning (`/api/v1/...`).
- **Rate limiting:** progressive throttling; per-IP and per-user buckets.
- **Pagination:** cursor-based / keyset where large.
- **DTOs only:** entities are not exposed.
- **Statuses:** `200, 201, 204, 400, 401, 403, 404, 409, 422, 429, 500`.

The full OpenAPI document is generated from the controllers in `backend/SalekhPos.Api/` and published as part of the deployment artifact.
