# Implementation Plan: Search Books by Author Name

**Branch**: `feature/author-book-search` | **Date**: 2026-10-30 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/008-author-book-search/spec.md`

## Summary

Add a case-insensitive literal-substring Author-name criterion to the existing Book list
operation. Preserve the current Author ID and exact legacy `author` filters. Validate the
new search term at the application boundary and apply it in MongoDB alongside the existing
criteria.

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`)
**Primary Dependencies**: ASP.NET Core Minimal APIs, MongoDB.Driver, FluentValidation
**Storage**: MongoDB `books` collection; existing `author` display-name and `authorId` fields
**Testing**: xUnit; WebApplicationFactory and MongoDB Testcontainers for API integration tests
**Target Platform**: .NET 10 service hosts supported by the repository
**Project Type**: Multi-project HTTP API
**Performance Goals**: No new latency SLA is specified. The search must execute as a database filter and not load/filter the full result set in application memory.
**Constraints**: Search is literal, case-insensitive substring matching with a 255-character maximum; current book listing is unpaged, and substring matching may scan the books collection.
**Scale/Scope**: Add one optional query filter to Book listing; no new entities, collections, indexes, endpoints, or migrations.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|---|---|---|
| Supported platform and modular architecture | Pass | Keep the existing .NET 10 API, Application service, and Infrastructure repository boundaries. |
| Explicit domain rules and simple boundaries | Pass | No aggregate or domain invariant changes; the application service validates the query input and delegates persistence filtering. |
| Documented and stable HTTP contracts | Pass | Add the optional parameter to the existing list operation and update the checked-in OpenAPI contract. |
| Secure errors and observable requests | Pass | Use the existing Problem Details middleware for invalid query values; do not expose database details. |
| Automated verification and coverage | Pass | Add application-level validation tests and Docker-backed API integration tests. |
| Simplicity / no unnecessary infrastructure | Pass | No new project, dependency, collection, migration, or index is required. |

No domain events are needed: listing is a read operation and introduces no business state change.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
src/
├── Bookstore.Api/Endpoints/BookEndpoints.cs
├── Bookstore.Application/Books/BookService.cs
├── Bookstore.Application/Books/IBookRepository.cs
└── Bookstore.Infrastructure/Books/MongoBookRepository.cs
tests/
└── Bookstore.Tests/
    ├── Unit/BookUseCasesTests.cs
    └── Integration/BookEndpointsTests.cs
specs/007-author-management/contracts/openapi.yaml
docs/functional.md
docs/technical.md
docs/testing.md
```

**Structure Decision**: Extend existing Book list request flow across the API,
Application, and Infrastructure layers. Keep the existing FEAT-007 OpenAPI contract as
the single checked-in API contract and update the current functional, technical, and
testing documentation. Feature-specific rationale and run instructions live in this
directory.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
