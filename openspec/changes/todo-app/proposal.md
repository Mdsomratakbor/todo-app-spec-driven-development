## Project Vision

A simple, intuitive Todo List Web Application that demonstrates professional Spec-Driven Development practices. Built with .NET 10 Web API, Angular 20, and PostgreSQL — using Cartographer.Mapper for object mapping and FluentResponse.ApiWrapper for standardized API responses — this project serves as a learning vehicle for the full spec-driven lifecycle while producing a functional task management tool.

## Problem Statement

Developers learning new technologies often jump straight into coding without proper specification, design, or requirement analysis. This leads to unclear scope, missed requirements, and rework. There is a need for a realistic-but-simple project that demonstrates the entire Spec-Driven Development workflow — from proposal through validation — using modern web technologies.

## Goals

- Deliver a working full-stack Todo application with user authentication, task CRUD, and category organization
- Demonstrate the complete Spec-Driven Development lifecycle across all 7 phases
- Produce clean, well-architected code following .NET and Angular best practices
- Establish reusable specification and design artifacts that can be referenced for future projects
- Ensure all requirements are testable through scenario-based specifications

## Non-Goals

- Real-time collaboration or multi-user task sharing
- Mobile-native applications (responsive web only)
- Third-party OAuth providers (Google, Microsoft, GitHub)
- Role-based authorization beyond user-scoped data isolation
- Performance optimization at scale (caching, query optimization, indexing strategy)
- DevOps pipeline automation (CI/CD, containerization)

## Scope

**In Scope:**
- Backend REST API (.NET 10 Web API with Entity Framework Core)
- Frontend SPA (Angular 20 with Angular Material)
- PostgreSQL database with EF Core code-first migrations
- User registration and JWT-based authentication
- Full CRUD for todo items with title, description, due date, completion status
- Category management for organizing todos
- Search and filter functionality (by status, category, due date range, text)
- User-scoped data isolation (users see only their own data)
- Object mapping with Cartographer.Mapper between entities and DTOs
- Standardized API response format using FluentResponse.ApiWrapper

**Out of Scope:**
- Password reset or email verification flows
- Bulk operations (batch delete, batch update)
- File attachments on todos
- Audit logging or activity history
- Dark mode / theme customization

## Stakeholders

- **Learning Developers**: Primary audience — developers who want to learn Spec-Driven Development with modern .NET and Angular
- **Project Maintainer**: Responsible for reviewing specs and approving changes
- **End Users**: People who will use the Todo application to manage personal tasks

## Success Metrics

- All scenarios defined in specification documents pass validation
- Frontend-backend integration works end-to-end for all CRUD flows
- 100% of spec requirements are traceable to implementation
- Application runs successfully on a local development environment with minimal setup

## Assumptions

- Development environment has .NET 10 SDK, Node.js, Angular CLI 20, and PostgreSQL installed
- PostgreSQL is accessible via localhost with configurable connection string
- Developers are familiar with basic C#, TypeScript, and REST API concepts
- No production deployment is required — local development server is sufficient
- JWT tokens are sufficient for authentication without refresh token rotation

## Risks

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| PostgreSQL Npgsql EF Core provider incompatibility with .NET 10 | Low | High | Verify package compatibility early in setup phase; fall back to EF Core in-memory provider for development if needed |
| Angular 20 Material components may have breaking changes | Low | Medium | Pin Angular Material version; use stable release channel |
| JWT secret management in development | Low | Low | Use user secrets for development; document configuration clearly |
| Requirement creep during implementation | Medium | Medium | Follow spec-first approach; treat spec changes as formal change requests |
