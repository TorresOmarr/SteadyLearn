# Project Overview

**SteadyLearn** is an adaptive learning platform MVP built with a focus on boring, testable, and scalable code.

## Vision

Build the foundation that will survive long-term evolution and AI/automation additions without architectural debt.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 8, PostgreSQL 15, EF Core 8 |
| Frontend | React 19, TypeScript, Vite, Zustand |
| Architecture | Vertical Slice Architecture (Modular Monolith) |
| Patterns | CQRS, MediatR, FluentValidation, Result Pattern |
| Languages | English (primary), Spanish (from Day 1) |

## Scope

### In Scope (Phase 1)

- Authentication (local email/password + JWT)
- Course creation and management (Admin only)
- Topics, Subtopics, Sessions hierarchy
- Manual content organization
- Multilingual support (EN/ES)
- RESTful API with Swagger
- Clean, testable code

### Out of Scope (Phase 1)

- AI/LLM integration
- Background jobs / Message queues
- Microservices / Multi-tenancy
- Feature flags
- Real-time collaboration
- Student features (Phase 2)
- Analytics / Metrics
- Payment systems

## Sprint Roadmap

### Sprint 1: Authentication Foundation (Complete)

**Goal**: Secure user registration and login, seed admin user

**Deliverables**:
- User entity with roles (Admin, Student)
- Email verification flow (mock email in dev)
- JWT tokens (Access + Refresh)
- Password hashing (bcrypt)
- Login/Register/Logout/RefreshToken/ResetPassword endpoints
- Admin seeding via configuration

### Sprint 2: Course Management (Upcoming)

**Goal**: Enable admins to build courses with structure

**Deliverables**:
- Course entity + translation tables
- Topic, Subtopic, Session entities
- CRUD endpoints for each
- Ordering system (explicit Order field)
- Course publish/unpublish
- Course Builder UI (React)

### Sprint 3+: Student Features (Future)

- View published courses
- Track progress
- Take tests/assessments

## User Roles

| Role | Permissions |
|------|-------------|
| Admin | Register, login, create/edit/publish courses, manage hierarchy |
| Student | View published courses, track progress (Phase 2+) |

## Quality Philosophy

> "Another senior dev should thank me, not curse me."

- Boring code > clever code
- Tested code > untested code
- Simple architecture > complex architecture
- Code should be deletable without cascading breaks

## Related Documentation

- [Architecture Decisions](./DECISIONS.md) - Why we chose what we chose
- [Backend Architecture](../backend/ARCHITECTURE.md) - Code structure
- [API Reference](../backend/API.md) - Endpoints
- [Setup Guide](../guides/SETUP.md) - Getting started
