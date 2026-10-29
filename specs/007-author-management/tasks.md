# Tasks: Book Author Management

**Input**: Design documents from `specs/007-author-management/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/openapi.yaml`, `quickstart.md`

**Tests**: Required by `PROMPT.md`, `FEAT-007.md`, and repository guidelines. Tests follow `MethodName_Condition_ExpectedResult()` naming.

## Phase 1: Setup

**Purpose**: Establish the .NET 10 solution and buildable project layout.

- [x] T001 Create the `Bookstore.sln`, `Directory.Build.props`, and `Directory.Packages.props` with `net10.0`, nullable references, implicit usings, and centralized package versions
- [x] T002 [P] Create `src/Bookstore.Domain/Bookstore.Domain.csproj` and `src/Bookstore.Domain/` folder structure
- [x] T003 [P] Create `src/Bookstore.Application/Bookstore.Application.csproj` and reference the Domain project
- [x] T004 [P] Create `src/Bookstore.Infrastructure/Bookstore.Infrastructure.csproj` and reference Application and Domain
- [x] T005 Create `src/Bookstore.Api/Bookstore.Api.csproj`, reference Application and Infrastructure, and configure the public `Program` entry point
- [x] T006 Create `tests/Bookstore.Tests/Bookstore.Tests.csproj` with xUnit, WebApplicationFactory, Testcontainers.MongoDb, and coverage dependencies

## Phase 2: Foundational

**Purpose**: Provide shared persistence, API error handling, logging, and test infrastructure before feature stories.

- [x] T007 [P] Add strongly typed MongoDB settings, connection registration, and `IMongoDatabase` setup in `src/Bookstore.Infrastructure/Configuration/MongoDbRegistration.cs`
- [x] T008 [P] Add RFC 7807 exception handling and trace-ID response/log correlation in `src/Bookstore.Api/Infrastructure/ProblemDetailsMiddleware.cs`
- [x] T009 Configure JSON console logging, request trace enrichment, OpenAPI generation, and Development-only Scalar UI in `src/Bookstore.Api/Program.cs`
- [x] T010 Add an xUnit MongoDB Testcontainer fixture and `WebApplicationFactory<Program>` configuration override in `tests/Bookstore.Tests/Infrastructure/BookstoreApiFactory.cs` and `MongoDbFixture.cs`
- [x] T011 Add service configuration validation and safe connection-string handling in `src/Bookstore.Api/appsettings.json` and `src/Bookstore.Api/Program.cs`

## Phase 3: User Story 1 - Modernized, documented bookstore API (Priority: P1)

**Goal**: Preserve the legacy Book API while moving it to the .NET 10 Minimal API and documenting its current behavior.

**Independent Test**: Start the API and MongoDB test container, exercise health/list/get/create/update book flows, and verify status codes and JSON match the recorded legacy contract.

### Tests for User Story 1

- [x] T012 [P] [US1] Add unit tests for category mapping, Book persistence mapping, and legacy ID/title lookup in `tests/Bookstore.Tests/Unit/BookUseCasesTests.cs`
- [x] T013 [P] [US1] Add API integration tests for health, list, get, create, and update routes in `tests/Bookstore.Tests/Integration/BookEndpointsTests.cs`

### Implementation for User Story 1

- [x] T014 [US1] Record existing Book endpoint behavior and technical debt in `docs/functional.md` and `docs/technical.md`
- [x] T015 [US1] Implement the Book domain entity and category rules in `src/Bookstore.Domain/Books/Book.cs` and `src/Bookstore.Domain/Books/Category.cs`
- [x] T016 [US1] Implement Book repository abstractions and MongoDB mappings in `src/Bookstore.Application/Books/IBookRepository.cs` and `src/Bookstore.Infrastructure/Books/MongoBookRepository.cs`
- [x] T017 [US1] Implement asynchronous Book use cases in `src/Bookstore.Application/Books/BookService.cs`
- [x] T018 [US1] Map legacy and conventional Book routes with request/response record DTOs in `src/Bookstore.Api/Endpoints/BookEndpoints.cs`
- [x] T019 [US1] Wire Book services, route groups, health route, and API metadata in `src/Bookstore.Api/Program.cs`

## Phase 4: User Story 2 - Author profiles (Priority: P1)

**Goal**: Create, list, and retrieve validated Author profiles with stable IDs and unique normalized names.

**Independent Test**: POST a valid Author, retrieve it by ID, and list active Authors; invalid and duplicate names return Problem Details.

### Tests for User Story 2

- [x] T020 [P] [US2] Add unit tests for Author name normalization, field validation, defaults, and duplicate handling in `tests/Bookstore.Tests/Unit/AuthorDomainTests.cs`
- [x] T021 [P] [US2] Add API integration tests for Author create/list/get and validation responses in `tests/Bookstore.Tests/Integration/AuthorEndpointsTests.cs`

### Implementation for User Story 2

- [x] T022 [US2] Implement the Author entity, normalized-name rules, and validation in `src/Bookstore.Domain/Authors/Author.cs`
- [x] T023 [US2] Add Author repository contracts and MongoDB persistence with the unique normalized-name index in `src/Bookstore.Application/Authors/IAuthorRepository.cs` and `src/Bookstore.Infrastructure/Authors/MongoAuthorRepository.cs`
- [x] T024 [US2] Implement Author create/list/get use cases in `src/Bookstore.Application/Authors/AuthorService.cs`
- [x] T025 [US2] Add Author request/response record DTOs and POST/GET routes in `src/Bookstore.Api/Endpoints/AuthorEndpoints.cs`
- [x] T026 [US2] Register Author services and document Author schemas and success/error responses in `src/Bookstore.Api/Program.cs` and `specs/007-author-management/contracts/openapi.yaml`

## Phase 5: User Story 3 - Author soft deletion (Priority: P1)

**Goal**: Deactivate Authors without deleting them or changing their Books; permit explicit inactive lookup.

**Independent Test**: Delete an Author, confirm default list/get omit it, confirm `includeInactive=true` returns it as inactive, and verify linked Books do not change.

### Tests for User Story 3

- [x] T027 [P] [US3] Add unit tests for idempotent soft deletion and active/inactive lookup behavior in `tests/Bookstore.Tests/Unit/AuthorUseCasesTests.cs`
- [x] T028 [P] [US3] Add API integration tests for DELETE and `includeInactive` list/get behavior in `tests/Bookstore.Tests/Integration/AuthorEndpointsTests.cs`

### Implementation for User Story 3

- [x] T029 [US3] Implement Author soft-delete and inactive filtering in `src/Bookstore.Application/Authors/AuthorService.cs` and `src/Bookstore.Infrastructure/Authors/MongoAuthorRepository.cs`
- [x] T030 [US3] Add DELETE and `includeInactive` route behavior with documented 204/400/404 responses in `src/Bookstore.Api/Endpoints/AuthorEndpoints.cs` and `specs/007-author-management/contracts/openapi.yaml`

## Phase 6: User Story 4 - Book association and author filtering (Priority: P1)

**Goal**: Link Books to stable Author IDs, backfill legacy names idempotently, and filter Books by Author.

**Independent Test**: Link Books to an Author, filter by its ID, run migration twice, and verify the second run makes no duplicate or destructive changes.

### Tests for User Story 4

- [x] T031 [P] [US4] Add unit tests for legacy author-name resolution, Book association, and author filtering in `tests/Bookstore.Tests/Unit/BookUseCasesTests.cs`
- [x] T032 [P] [US4] Add integration tests for authorId writes/filtering and legacy author compatibility in `tests/Bookstore.Tests/Integration/BookEndpointsTests.cs`
- [x] T033 [P] [US4] Add migration tests for existing links, blank names, normalized duplicates, and reruns in `tests/Bookstore.Tests/Integration/AuthorBackfillTests.cs`

### Implementation for User Story 4

- [x] T034 [US4] Add optional `AuthorId` MongoDB mapping and string API projection while retaining legacy `author` in `src/Bookstore.Domain/Books/Book.cs` and `src/Bookstore.Infrastructure/Books/MongoBookRepository.cs`
- [x] T035 [US4] Resolve explicit authorId and legacy author-name writes through Author use cases in `src/Bookstore.Application/Books/BookService.cs`
- [x] T036 [US4] Add an idempotent legacy author backfill with collision/blank reporting in `src/Bookstore.Infrastructure/Migrations/AuthorBackfill.cs`
- [x] T037 [US4] Add authorId filtering and legacy author-name lookup to Book route/query handling in `src/Bookstore.Api/Endpoints/BookEndpoints.cs`
- [x] T038 [US4] Update Book endpoint OpenAPI request/response schemas and filtering examples in `specs/007-author-management/contracts/openapi.yaml`

## Phase 7: Polish and cross-cutting concerns

**Purpose**: Meet delivery gates and keep documentation, runtime packaging, and mainline consistent.

- [x] T039 [P] Add coverage collection and enforce the 85% Domain/Application line threshold in `tests/Bookstore.Tests/Bookstore.Tests.csproj` and `Directory.Build.props`
- [x] T040 [P] Update `README.md` with prerequisites, setup, API docs, tests, architecture, and maintainer guidance
- [x] T041 [P] Complete functional rules and technical architecture documentation in `docs/functional.md` and `docs/technical.md`
- [x] T042 [P] Add local and CI test/coverage instructions in `docs/testing.md` and `specs/007-author-management/quickstart.md`
- [x] T043 [P] Add the QA acceptance checklist in `docs/qa-checklist.md`
- [x] T044 Update `Dockerfile`, `docker-compose.yml`, and `.gitignore` for .NET 10 while preserving MongoDB initialization and volumes
- [x] T045 Run `dotnet test`, verify coverage thresholds and OpenAPI responses, and resolve failures in the relevant source/test files
- [x] T046 Verify all documentation and OpenAPI examples match implementation and record final branch/commit integration on `main`

## Dependencies and Execution Order

- Setup tasks T001–T006 must finish before foundational tasks T007–T011.
- User Story 1 must be completed before Author work because it establishes the Book domain and MongoDB integration.
- User Story 2 precedes User Story 3 and User Story 4 because lifecycle and association depend on Author persistence.
- User Story 3 and User Story 4 can proceed independently after User Story 2, provided they do not edit the same Author/Book service files concurrently.
- Polish tasks follow the user stories; T045 and T046 are final gates.

## Parallel Execution Examples

- During setup, T002–T004 can be scaffolded independently after T001.
- During User Story 1, T012 and T013 can be authored independently before implementation; T015 and the functional documentation task can proceed in separate files.
- During User Story 2, T020 and T021 can be authored independently; domain validation and API test scaffolding can proceed separately.
- In the polish phase, T040–T043 edit separate documentation files and can proceed independently.

## Implementation Strategy

Deliver the documented .NET Book API first as the compatibility MVP, then add Author create/list/get, soft deletion, and Book association/filtering in separate tested increments. Keep the legacy author field until the staged backfill has been validated. Complete coverage and documentation gates before integrating each task branch into `main`.
