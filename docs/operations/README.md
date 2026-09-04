# Operations

## Observability

- Structured logs with `requestId`, `userId`, `tenantId`, `storeId`, `deviceId`, `endpoint`, `duration`, `statusCode`.
- Metrics for API latency, sale completion time, queue depth, inventory movement rate, sync success rate, hardware health, payment/fiscal UNKNOWN rate.
- Distributed tracing where justified.
- Correlation IDs propagate from edge to background jobs.

## Health

- `/health/live` — liveness; the process is up.
- `/health/ready` — readiness; dependencies (PostgreSQL, Redis) reachable.

## Alerts

- Sale completion p95 over threshold.
- Inventory movement rate anomalous.
- Sync queue depth or conflict rate anomalous.
- Payment UNKNOWN rate over threshold.
- Fiscal UNKNOWN rate over threshold.
- Authentication failures, MFA challenges, token reuse detections, suspicious device activity.

## Backups and recovery

- Automated encrypted backups.
- Retention policy.
- **Restore verification** — a backup that has never been restored/tested is not assumed reliable.
- RPO / RTO targets are derived from business requirements, not invented.
- Recovery procedures are documented and rehearsed.

## Deploy

- Forward-compatible migrations.
- Health-check gated rollout.
- Smoke tests after deploy.
- Rollback plan recorded for each release.
