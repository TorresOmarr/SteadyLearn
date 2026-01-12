# 📚 SteadyLearn

Adaptive learning platform to build and deliver courses with modules, topics, and sessions in EN/ES. Includes authentication with email verification, password recovery, and rotated access/refresh tokens.

## ✨ What it does
- Create courses structured into topics, subtopics, and sessions.
- Manage bilingual content (English and Spanish).
- Ready-to-use auth: register, login, email verification, refresh, and password reset.
- Error codes are ready for frontend translation.

## 🚀 Run it locally (no Docker)
**Prerequisites**
- .NET 8 SDK
- PostgreSQL 18 locally

**Quick steps**
```bash
# 1) Enter the API project
cd api

# 2) Configure the connection in appsettings.Development.json
#    "DefaultConnection": "Host=localhost;Port=5432;Database=steadylearn;Username=...;Password=..."

# 3) Create the database if it doesn't exist
psql -U postgres -c "CREATE DATABASE steadylearn;"

# 4) Restore packages and apply existing migrations (created by the human maintainer)
dotnet restore
dotnet ef database update

# 5) Run the API
dotnet run
# Swagger: http://localhost:5000/swagger
```
> Note: If you need a new migration, ask the human maintainer to generate/apply it.

## 🧭 Where to read more (quick)
- Quick index: `docs/INDEX.md`
- Rules and conventions: `AGENTS.md`, `docs/CONVENTIONS.md`
- Architecture and slices: `docs/backend/ARCHITECTURE.md`
- Domain model: `docs/backend/DOMAIN.md`

## 📊 Status
- ✅ Sprint 1 (Backend Auth) done.
- 🚧 Frontend in progress.
- Next: course/topic/subtopic/session management and progress tracking.

## 🤝 Contributing
- Follow Vertical Slice + CQRS; mutate state only through domain methods.
- Use error codes and the Result Pattern.
- New migrations are generated/applied only by the human maintainer.

---
**Built with ❤️ on .NET 8 + PostgreSQL 18**
