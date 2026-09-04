# Pull request template — SalekhPos

## Summary

What does this change do, and why? Reference the phase / ADR it implements.

## Phase / ADR

- Phase: _(e.g. Phase 2 — Identity / Authentication)_
- ADR: _(e.g. ADR-006 — Multi-tenancy)_

## Change type

- [ ] feat
- [ ] fix
- [ ] security
- [ ] refactor
- [ ] perf
- [ ] test
- [ ] docs
- [ ] build / ci
- [ ] chore

## Quality gates

- [ ] Build passes locally.
- [ ] Unit tests pass.
- [ ] Integration tests pass (where applicable).
- [ ] Lint / format pass.
- [ ] Security scan passes (no new HIGH/CRITICAL vulnerabilities).
- [ ] Documentation updated where the change affects behaviour.
- [ ] No secrets committed.
- [ ] No business rules added to controllers / clients.
- [ ] No client-side authorization treated as authoritative.
- [ ] No last-write-wins for inventory or financial state.
- [ ] Migrations reviewed (no destructive change without impact analysis).

## Test evidence

Commands run and their results:

```
$ dotnet build ...
$ dotnet test ...
$ npm run test ...
```

## Operational impact

- [ ] Migration required.
- [ ] Configuration change required.
- [ ] Outage / restart required.
- [ ] Backward compatible.

## Reviewer notes

Anything reviewers should pay attention to (tenant isolation, concurrency, financial correctness, security).
