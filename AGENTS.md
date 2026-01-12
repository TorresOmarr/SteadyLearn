# Agent Instructions

**Scope:** Entire repository.

- Do **not** scaffold, modify, or delete Entity Framework migrations. The human runs all `dotnet ef migrations add/update/remove` commands and owns files under `api/src/Data/Migrations`.
- If a change requires a migration, stop and ask the human to generate it. Do not create migration files yourself.
- You may adjust models/configuration code, but leave migration creation/applies to the human.

**How to proceed (strict):**
1) Read `docs/INDEX.md` before the first interaction/change to know where to look.
2) Check for AGENTS in the current scope.
3) If backend feature: read `docs/backend/DOMAIN.md` (encapsulation, factories) and `docs/backend/ARCHITECTURE.md` (vertical slices).
4) If database change: read `docs/backend/DATABASE.md` + `docs/guides/MIGRATIONS.md` (remember: migrations are human-only).
5) If auth/API change: read `docs/backend/API.md` (Auth section) + `docs/project/DECISIONS.md` (JWT strategy).

**Domain rule:** setters private + static factory + behaviors on the model (see `docs/backend/DOMAIN.md`). Keep logic inside the model for maintainability.
