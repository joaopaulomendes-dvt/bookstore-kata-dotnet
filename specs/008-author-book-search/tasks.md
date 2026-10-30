# Tasks: Search Books by Author Name

**Input**: Design documents from `specs/008-author-book-search/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `quickstart.md`

**Tests**: Required by the feature specification and repository constitution. Tests follow `MethodName_Condition_ExpectedResult()`.

## Phase 1: Setup

**Purpose**: Reuse the existing .NET 10 solution, centralized package management, and MongoDB test infrastructure.

- [X] T001 Confirm the existing solution and Docker-backed test fixture are usable via `Bookstore.sln` and `tests/Bookstore.Tests/Bookstore.Tests.csproj`; do not add packages or projects.

## Phase 2: Foundational

**Purpose**: No new shared infrastructure is needed. The feature extends the existing Book list query through its established API, Application, and Infrastructure layers.

## Phase 3: User Story 1 - Find catalog books by an author name (Priority: P1)

**Goal**: Consumers can find Books by a literal, case-insensitive substring of the stored Author display name.

**Independent Test**: Insert Books by different Authors, search using a mixed-case name fragment and a literal metacharacter, and verify matching and non-matching results through the list endpoint.

### Tests for User Story 1

- [X] T002 [P] [US1] Add `ListAsync_AuthorNameSearch_RejectsTermsLongerThan255Characters` and whitespace search tests in `tests/Bookstore.Tests/Unit/BookUseCasesTests.cs`.
- [X] T003 [US1] Add API integration tests for case-insensitive substring matching, literal special characters, and empty results in `tests/Bookstore.Tests/Integration/BookEndpointsTests.cs`.

### Implementation for User Story 1

- [X] T004 [US1] Bind `authorName` and preserve existing query parameters in `src/Bookstore.Api/Endpoints/BookEndpoints.cs`; trim blank input and reject terms over 255 characters in `src/Bookstore.Application/Books/BookService.cs`.
- [X] T005 [US1] Extend the list contract and add an escaped, case-insensitive substring predicate in `src/Bookstore.Application/Books/IBookRepository.cs` and `src/Bookstore.Infrastructure/Books/MongoBookRepository.cs`; propagate cancellation.
- [X] T006 [US1] Document the optional `authorName` parameter, matching rules, maximum length, and 400 response in `specs/007-author-management/contracts/openapi.yaml`.

## Phase 4: User Story 2 - Preserve existing author filters (Priority: P1)

**Goal**: Existing stable Author ID and exact legacy `author` filters continue to work and combine with the new criterion without changing their semantics.

**Independent Test**: Run integration scenarios for `authorId`, exact `author`, and combinations of all supplied criteria; confirm the response contains only records satisfying every criterion.

### Tests for User Story 2

- [X] T007 [US2] Add regression integration tests for stable `authorId` filtering, exact legacy `author` matching, and AND composition with `authorName` in `tests/Bookstore.Tests/Integration/BookEndpointsTests.cs`.

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Keep product documentation, feature artifacts, and validation evidence consistent.

- [X] T008 [P] Update `docs/functional.md`, `docs/technical.md`, and `docs/testing.md` with the new search behavior, compatibility semantics, tests, and performance limitation.
- [X] T009 Run the full Docker-backed test suite and verify OpenAPI/docs consistency and Markdown links; record verified outcomes in `specs/008-author-book-search/quickstart.md`.

## Dependencies & Execution Order

### Phase Dependencies

- **Setup**: T001 verifies existing prerequisites; no project scaffolding is required.
- **Foundational**: No new shared foundation is required.
- **User Story 1**: T002–T003 establish failing/acceptance coverage; T004–T006 implement and document the new criterion.
- **User Story 2**: T007 depends on the new query flow so it can verify compatibility and composition.
- **Polish**: T008 can proceed independently after search behavior is established; T009 is the final verification gate.

### User Story Dependencies

- **US1 (P1)**: Independent implementation of the requested search behavior.
- **US2 (P1)**: Depends on US1's additive query parameter to verify compatibility and combined-filter behavior.

### Parallel Opportunities

- T002 can be authored independently from T003 because they edit separate test files.
- T008 uses documentation files and can proceed independently of implementation once behavior is finalized.
- Tasks touching `BookEndpointsTests.cs` are intentionally sequential to avoid conflicting edits.

## Implementation Strategy

Deliver US1 as the MVP: first add tests, then implement the query through the existing layers, update the API contract, and verify the full suite. Complete US2 regression tests before final documentation and verification. No new packages, database schema, indexes, or domain events are planned.

## Notes

- `[P]` tasks operate on separate files and have no dependency on incomplete tasks.
- Search is an escaped substring query against the stored Author display name; it can scan the unpaged books collection. No performance guarantee beyond database-side filtering is introduced.
- Tests require Docker socket access for Testcontainers MongoDB integration coverage.
