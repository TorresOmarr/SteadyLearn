# 📑 Documentation Index & Quick Reference

## 🎯 Start Here

**New to the project?** Read these in order:

1. **[AGENTS.md](./AGENTS.md)** - 5-10 min read
   - What we're building and why
   - Scope and sprints
   - Key decisions
   
2. **[SETUP.md](./SETUP.md)** - 10 min to setup
   - Get your environment running
   - Verify everything works
   
3. **[ARCHITECTURE.md](./ARCHITECTURE.md)** - 10 min read
   - How the code is organized
   - Patterns we use

---

## 📚 Documentation by Role

### 👨‍💻 Backend Developer

Essential docs:
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Code structure
- [DOMAIN_MODEL.md](./DOMAIN_MODEL.md) - Entity definitions
- [API_DESIGN.md](./API_DESIGN.md) - Endpoint contracts
- [DATABASE.md](./DATABASE.md) - EF Core + migrations
- [TESTING.md](./TESTING.md) - How to test
- [AUTH_IMPLEMENTATION.md](./AUTH_IMPLEMENTATION.md) - Auth details
- [I18N_STRATEGY.md](./I18N_STRATEGY.md) - Multilenguaje

### 🎨 Frontend Developer

Essential docs:
- [FRONTEND.md](./FRONTEND.md) - React structure
- [API_DESIGN.md](./API_DESIGN.md) - API endpoints
- [I18N_STRATEGY.md](./I18N_STRATEGY.md) - Frontend i18n
- [AUTH_IMPLEMENTATION.md](./AUTH_IMPLEMENTATION.md) - Auth flows
- [SETUP.md](./SETUP.md) - Local setup

### 🧪 QA / Test Engineer

Essential docs:
- [TESTING.md](./TESTING.md) - Test strategy
- [API_DESIGN.md](./API_DESIGN.md) - API contracts
- [AGENTS.md](./AGENTS.md) - Scope & features

### 🔧 DevOps / Infrastructure

Essential docs:
- [SETUP.md](./SETUP.md) - Docker setup
- [DATABASE.md](./DATABASE.md) - PostgreSQL
- [AGENTS.md](./AGENTS.md) - Tech stack

---

## 🔍 Documentation by Feature

### 🔐 Authentication (Sprint 1)
- [AUTH_IMPLEMENTATION.md](./AUTH_IMPLEMENTATION.md) - Full details
- [API_DESIGN.md](./API_DESIGN.md) - Auth endpoints
- [DOMAIN_MODEL.md](./DOMAIN_MODEL.md) - User entity
- [TESTING.md](./TESTING.md) - Auth tests

### 📚 Course Management (Sprint 2)
- [API_DESIGN.md](./API_DESIGN.md) - Course endpoints
- [DOMAIN_MODEL.md](./DOMAIN_MODEL.md) - Course hierarchy
- [FRONTEND.md](./FRONTEND.md) - Course Builder UI
- [DATABASE.md](./DATABASE.md) - Schema

### 🌍 Multilenguaje (EN/ES)
- [I18N_STRATEGY.md](./I18N_STRATEGY.md) - Core strategy
- [DOMAIN_MODEL.md](./DOMAIN_MODEL.md) - Translation tables
- [DATABASE.md](./DATABASE.md) - Schema configuration
- [FRONTEND.md](./FRONTEND.md) - i18n implementation

---

## 📋 Document Summaries

| Document | Purpose | Read Time | Key Topics |
|----------|---------|-----------|-----------|
| **AGENTS.md** | Project blueprint & decisions | 10 min | Scope, sprints, architecture, decisions |
| **ARCHITECTURE.md** | Backend code structure | 10 min | Vertical slices, patterns, folder structure |
| **API_DESIGN.md** | REST endpoint contracts | 15 min | All endpoints, responses, error codes |
| **DOMAIN_MODEL.md** | Entity definitions | 15 min | Entities, relationships, constraints, rules |
| **DATABASE.md** | EF Core + migrations | 15 min | DbContext, configs, migrations, queries |
| **TESTING.md** | Testing strategy | 15 min | Unit tests, integration tests, examples |
| **AUTH_IMPLEMENTATION.md** | Auth system details | 15 min | JWT, tokens, flows, seeding |
| **I18N_STRATEGY.md** | Multilenguaje support | 15 min | Translation tables, middleware, i18n |
| **FRONTEND.md** | React architecture | 15 min | Zustand, hooks, services, components |
| **SETUP.md** | Getting started | 10 min | Docker, .NET, Node.js, verification |

---

## 🎯 Quick Decisions Reference

### Architecture
- **Pattern**: Vertical Slice Architecture (modular monolith)
- **Approach**: Domain-Driven Design + CQRS-lite
- **State Management**: Zustand (frontend)
- **ORM**: Entity Framework Core 8

### Database
- **Engine**: PostgreSQL 15
- **Approach**: Code-First migrations
- **Soft Delete**: Yes (via IsDeleted flag)
- **Audit Trail**: CreatedBy, UpdatedBy, CreatedAtUtc, UpdatedAtUtc

### Authentication
- **Type**: JWT (Access + Refresh tokens)
- **Access Token**: 15 min lifetime
- **Refresh Token**: 7 days, HttpOnly cookie, rotated
- **Password**: bcrypt hashing
- **Email Verification**: 24-hour tokens

### Multilenguaje
- **Strategy**: Separate translation tables
- **Languages**: English (EN) primary, Spanish (ES) secondary
- **API**: Accept-Language header for responses
- **Frontend**: JSON translation files per language

### Testing
- **Framework**: xUnit
- **Assertions**: FluentAssertions
- **Database**: InMemory for unit tests
- **Coverage Target**: 80%+ on critical paths

### API Response Format
```json
{
  "success": true,
  "data": { /* payload */ },
  "code": "SUCCESS_CODE"
}

// Error response
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable message"
  }
}
```

---

## 🔗 Document Dependencies

```
AGENTS.md (Master document)
  ├── ARCHITECTURE.md
  ├── API_DESIGN.md
  ├── DOMAIN_MODEL.md
  ├── TESTING.md
  ├── AUTH_IMPLEMENTATION.md
  ├── I18N_STRATEGY.md
  ├── DATABASE.md
  ├── FRONTEND.md
  └── SETUP.md

Related by feature:
  Auth
    ├── AUTH_IMPLEMENTATION.md
    ├── API_DESIGN.md (auth endpoints)
    ├── DOMAIN_MODEL.md (User entity)
    └── TESTING.md (auth tests)

  Course Management
    ├── API_DESIGN.md (course endpoints)
    ├── DOMAIN_MODEL.md (course hierarchy)
    ├── FRONTEND.md (course builder)
    └── DATABASE.md (course schema)

  Multilenguaje
    ├── I18N_STRATEGY.md
    ├── DATABASE.md (translation tables)
    ├── DOMAIN_MODEL.md (entities)
    └── FRONTEND.md (i18n hooks)
```

---

## 🚀 Implementation Checklist

### Pre-Development
- [ ] Read AGENTS.md to understand vision
- [ ] Review ARCHITECTURE.md for structure
- [ ] Check SETUP.md and set up local environment
- [ ] Verify Docker, .NET 8, Node.js installed
- [ ] Run migrations and seed admin user

### Sprint 1: Authentication
- [ ] Create User entity (DOMAIN_MODEL.md)
- [ ] Create database migrations (DATABASE.md)
- [ ] Implement Register handler (ARCHITECTURE.md)
- [ ] Implement Login handler
- [ ] Implement Email Verification
- [ ] Implement Token Refresh
- [ ] Implement Password Reset
- [ ] Create API endpoints (API_DESIGN.md)
- [ ] Write unit tests (TESTING.md)
- [ ] Create frontend components (FRONTEND.md)
- [ ] Create auth store (Zustand)
- [ ] Implement i18n (I18N_STRATEGY.md)
- [ ] Test complete auth flow

### Sprint 2: Course Management
- [ ] Create Course entities (DOMAIN_MODEL.md)
- [ ] Create migration for courses
- [ ] Implement CRUD handlers
- [ ] Create API endpoints
- [ ] Add publish/unpublish logic
- [ ] Implement ordering system
- [ ] Add multilenguaje support (I18N_STRATEGY.md)
- [ ] Write unit tests
- [ ] Create frontend components
- [ ] Create course store
- [ ] Build Course Builder UI (tree + accordion)
- [ ] Test complete course management flow

---

## 💡 Guiding Principles

### Code Quality
```
✓ Boring code > clever code
✓ Testable code > untested code
✓ Simple architecture > complex architecture
✓ Another senior dev should thank me, not curse me
```

### When Uncertain
1. Make the safest, simplest assumption
2. Document it in comments
3. Ask for review from another senior dev
4. Code should be easy to delete or extend

### Domain Rules
- Always document why (not just what)
- Never ignore business logic
- Test domain rules exhaustively
- Hard to change = probably a domain rule

---

## 📞 Getting Help

### Technical Questions
1. Check relevant documentation first
2. Search for related patterns in docs
3. Look at examples in TESTING.md or FRONTEND.md
4. Ask in code review

### Configuration Issues
- Check SETUP.md first
- Verify Docker is running
- Check connection strings
- Review appsettings.Development.json

### Architecture Questions
- Read AGENTS.md first (decisions are documented)
- Review ARCHITECTURE.md (patterns explained)
- Check related feature docs

---

## 🎓 Learning Path

**Week 1: Foundation**
1. Read AGENTS.md
2. Read ARCHITECTURE.md
3. Read DOMAIN_MODEL.md
4. Do SETUP.md (hands-on)
5. Review API_DESIGN.md

**Week 2: Auth Implementation**
1. Read AUTH_IMPLEMENTATION.md
2. Review relevant API endpoints
3. Review TESTING.md for test examples
4. Implement auth handlers
5. Write tests
6. Build UI components

**Week 3: Deeper Dives**
1. Review DATABASE.md
2. Review I18N_STRATEGY.md
3. Review FRONTEND.md
4. Review TESTING.md patterns

**Week 4: Course Management**
1. Review course-related endpoints
2. Review domain model for courses
3. Implement course features
4. Build Course Builder UI

---

*This index serves as a roadmap for SteadyLearn development. Start with AGENTS.md!*
