# Documentation Index

## Quick Start

1. **Read AGENTS.md** (root) for hard rules
2. **Read this INDEX** to locate docs by role/feature
3. **[Project Overview](./project/OVERVIEW.md)** - What we're building
4. **[Setup Guide](./guides/SETUP.md)** - Get your environment running
5. **[Architecture](./backend/ARCHITECTURE.md)** - How the code is organized

---

## Documentation Structure

```
docs/
├── CLAUDE.md              # AI agent reference
├── INDEX.md               # This file
├── project/
│   ├── OVERVIEW.md        # Vision, scope, sprints
│   └── DECISIONS.md       # Architecture decisions
├── backend/
│   ├── ARCHITECTURE.md    # Code structure, CQRS
│   ├── DOMAIN.md          # Entities and rules
│   ├── API.md             # Endpoints reference
│   └── DATABASE.md        # EF Core, schema
└── guides/
    ├── SETUP.md           # Local development
    ├── MIGRATIONS.md      # Database migrations
    ├── TESTING.md         # Test strategy
    └── I18N.md            # Multilingual support
```

---

## By Role

### Backend Developer
- [Architecture](./backend/ARCHITECTURE.md) - Code structure, CQRS patterns
- [Domain Model](./backend/DOMAIN.md) - Entity definitions
- [API Reference](./backend/API.md) - Endpoint contracts
- [Database](./backend/DATABASE.md) - EF Core + migrations
- [Testing Guide](./guides/TESTING.md) - How to test

### Frontend Developer
- [API Reference](./backend/API.md) - API endpoints
- [I18N Guide](./guides/I18N.md) - Frontend i18n
- [Setup Guide](./guides/SETUP.md) - Local setup

### DevOps
- [Setup Guide](./guides/SETUP.md) - Docker setup
- [Database](./backend/DATABASE.md) - PostgreSQL config

---

## By Feature

### Authentication (Sprint 1)
- [API Reference](./backend/API.md#authentication-endpoints)
- [Domain Model](./backend/DOMAIN.md#user-entity)
- [Decisions](./project/DECISIONS.md#5-jwt-strategy-access--refresh-tokens)

### Course Management (Sprint 2)
- [API Reference](./backend/API.md#course-endpoints)
- [Domain Model](./backend/DOMAIN.md#course-entity)

### Multilingual Support
- [I18N Guide](./guides/I18N.md)
- [Domain Model](./backend/DOMAIN.md) - Translation entities
- [Decisions](./project/DECISIONS.md#4-separate-translation-tables)

---

## Document Summary

| Document | Purpose | Read Time |
|----------|---------|-----------|
| [OVERVIEW](./project/OVERVIEW.md) | Vision, scope, sprints | 5 min |
| [DECISIONS](./project/DECISIONS.md) | Architecture decisions | 10 min |
| [ARCHITECTURE](./backend/ARCHITECTURE.md) | Code structure, CQRS | 15 min |
| [DOMAIN](./backend/DOMAIN.md) | Entities, relationships | 10 min |
| [API](./backend/API.md) | Endpoints, responses | 15 min |
| [DATABASE](./backend/DATABASE.md) | EF Core, schema | 10 min |
| [SETUP](./guides/SETUP.md) | Local development | 10 min |
| [MIGRATIONS](./guides/MIGRATIONS.md) | Database migrations | 5 min |
| [TESTING](./guides/TESTING.md) | Test strategy | 10 min |
| [I18N](./guides/I18N.md) | Multilingual support | 10 min |
| [CONVENTIONS](./CONVENTIONS.md) | Coding rules & patterns | 7 min |

---

## For Agents (fast path)
- Always read **AGENTS.md** first (hard rules, migrations human-only).
- Then skim this INDEX to jump where needed.
- Backend feature: `backend/DOMAIN.md` (encapsulación, factorías) + `backend/ARCHITECTURE.md` (vertical slices) + `CONVENTIONS.md` (reglas y checklist).
- Auth/API: `backend/API.md` (Auth section) + `project/DECISIONS.md` (JWT strategy) + DOMAIN (User rules).
- Database: `backend/DATABASE.md` + `guides/MIGRATIONS.md` (solo humano corre/crea migraciones).
- Refresh tokens: modelo dedicado en DB; revisa `DATABASE.md`.
- Estándares de fecha/hora: usar `DateTimeOffset` y `timestamp with time zone`.

## Quick Reference

### Tech Stack
- **Backend**: .NET 8, PostgreSQL 15, EF Core 8
- **Frontend**: React 19, TypeScript, Vite, Zustand
- **Patterns**: Vertical Slice, CQRS, Result Pattern

### Authentication
- **Access Token**: 15 min, Authorization header
- **Refresh Token**: 7 days, HttpOnly cookie, rotated (tabla dedicada)
- **Password**: bcrypt hashing

### Languages
- English (EN) - Primary
- Spanish (ES) - From Day 1

### API Response Format
```json
{
  "success": true,
  "data": { /* payload */ }
}

{
  "success": false,
  "error": { "code": "ERROR_CODE" }
}
```

---

## Guiding Principles

- Boring code > clever code
- Tested code > untested code
- Simple architecture > complex architecture
- Another senior dev should thank me, not curse me

---

*Start with [Project Overview](./project/OVERVIEW.md)!*
