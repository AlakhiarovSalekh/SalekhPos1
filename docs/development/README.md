# Development

This folder tracks the current implementation state of the SalekhPos platform.

- [`IMPLEMENTATION_STATUS.md`](IMPLEMENTATION_STATUS.md) — current phase, completed work, pending work, known issues, migrations, blockers, last verified commit, next phase.
- [`LOCAL_DEV.md`](LOCAL_DEV.md) — how to run the stack locally.

## Phase gate

A phase is complete only when:

```text
Implementation
 -> Tests
 -> Build
 -> Security checks
 -> Documentation
 -> Verification
 -> Logical commit
 -> Push
```

## Phases

0. Foundation
1. Infrastructure
2. Identity / Auth
3. Multi-tenancy
4. Product catalog
5. Inventory
6. Purchasing
7. Core POS sales
8. Desktop POS
9. Sync engine
10. Returns / refunds
11. Cash / expenses
12. Reporting / analytics
13. Realtime
14. Mobile
15. Super Admin
16. Integrations
17. Hardware certification
18. Security hardening
19. Performance
20. Production hardening
21. Disaster recovery
22. Final E2E
23. Release candidate
24. Final quality gate
25. Final audit
