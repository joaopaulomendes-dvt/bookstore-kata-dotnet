# Implementation Plan: Book Author Management

**Delivery branch**: `main` | **Spec**: [spec.md](./spec.md)
**Input**: [FEAT-007.md](../../FEAT-007.md), [PROMPT.md](../../PROMPT.md), and [COMPANY_GUIDELINES.md](../../COMPANY_GUIDELINES.md)

## Summary

Document the existing Java 11/Jersey/MongoDB Book API, migrate its behavior to a .NET 10 Minimal API, and add Author profiles, soft deletion, Book references, and author-based catalog filtering. Keep MongoDB and use BSON ObjectIds for persistence with string IDs in public DTOs. Add an idempotent legacy data backfill. The solution follows a lightweight DDD separation and the company’s API, error, logging, testing, coverage, and documentation requirements.

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`)
**Primary Dependencies**: ASP.NET Core Minimal APIs, `MongoDB.Driver`, `Microsoft.AspNetCore.OpenApi`, Scalar UI (Development only), xUnit, `Microsoft.AspNetCore.Mvc.Testing`, Testcontainers for MongoDB, coverlet
**Storage**: MongoDB 7.0; retain the existing `bookstore` database and books collection; add an authors collection and required indexes
**Testing**: `dotnet test`; xUnit unit tests and `WebApplicationFactory<Program>` integration tests using MongoDB Testcontainers
**Target Platform**: .NET 10 container/Linux and local development
**Project Type**: HTTP web service
**Performance Goals**: Use indexed MongoDB queries for Book/Author lookup and filtering; no unsubstantiated throughput target
**Constraints**: Preserve existing Book behaviors and MongoDB documents; use RFC 7807 errors, structured JSON logs with trace IDs, cancellation-aware async handlers, non-production API UI, minimum 70% business logic coverage and 85% Domain/Application coverage
**Scale/Scope**: Single bookstore API, two primary domain entities, existing Book endpoints plus Author resource endpoints

## Constitution Check

The checked-in constitution contains only unfilled placeholders and cannot provide valid project gates. Use `COMPANY_GUIDELINES.md`, the .NET architecture instructions, and explicit prompt acceptance criteria:

- **Stack**: .NET 10 Minimal APIs only; use `dotnet` CLI.
- **Architecture**: domain/application/infrastructure responsibilities, DI, SOLID, and small scope; do not introduce event infrastructure without a consumer.
- **API**: plural resource routes, complete OpenAPI, Problem Details, async handlers with cancellation.
- **Observability**: JSON logs and trace correlation; do not log sensitive data.
- **Quality**: unit and integration tests; at least 85% Domain/Application coverage and 70% business logic coverage.
- **Documentation**: README, functional, technical, OpenAPI, testing, and QA deliverables.

**Gate result**: PASS subject to replacing the placeholder constitution with a valid one as a separate governance task only if required; this feature does not amend it.

## Domain and Architecture

- **Entities**: `Author` has stable ObjectId identity and profile/lifecycle data. `Book` retains catalog data and optionally references `AuthorId`.
- **Relationship**: One Author may be associated with many Books. A Book has at most one Author in this version.
- **Aggregate/consistency boundary**: Author lifecycle is independent from Book lifecycle. Soft-deleting an Author does not mutate Books. Book association validates that a target Author exists and is active.
- **Ubiquitous language**: Author, Book, active/inactive Author, soft delete, legacy author name, author reference.
- **Events**: No domain events are required because no external consumers are present. Revisit only if a real integration needs decoupled notifications.
- **Identity**: Persist ObjectId in MongoDB; represent it as a string in API contracts. Use a normalized name field with a unique index for duplicate detection/migration matching, not as a foreign key.

## Implementation Structure

```text
src/
├── Bookstore.Api/                # Minimal API groups, record DTOs, middleware, OpenAPI
├── Bookstore.Application/        # Use cases, validation, repository abstractions
├── Bookstore.Domain/             # Book/Author entities and business invariants
└── Bookstore.Infrastructure/     # MongoDB repositories, indexes, legacy backfill
tests/
└── Bookstore.Tests/              # xUnit unit/integration tests and WebApplicationFactory
Directory.Packages.props          # Central package versions for the multi-project solution
Bookstore.sln
Dockerfile
docker-compose.yml
docs/
├── functional.md
├── technical.md
└── testing.md
```

Keep the solution lightweight: one test project can organize unit and integration tests into separate folders. The .NET service and its integration suite are verified; the superseded Java project has been removed. The user-supplied source briefs are tracked without changing their content.

## API Design

- Root group `/api`.
- Preserve `/api/books`, `/api/books/health`, `/api/books/single`, `POST /api/books`, and `PUT /api/books` as documented compatibility routes.
- Add conventional `GET /api/books/{id}` and Book filtering by `authorId`; retain legacy author-name filtering.
- Add `POST /api/authors`, `GET /api/authors`, `GET /api/authors/{id}`, and `DELETE /api/authors/{id}`.
- Author listing/retrieval excludes inactive records by default. `includeInactive=true` explicitly permits listing or retrieving inactive Authors.
- Return RFC 7807 Problem Details for validation, malformed IDs, conflicts, not-found, and server failures.
- Describe unauthenticated access in the OpenAPI document because no authentication currently exists.

## Data Migration

Use an idempotent migration/backfill:

1. Add optional `authorId` while retaining `author` on Book documents.
2. Normalize legacy author names for matching; create/reuse one Author record per normalized name and record its generated ObjectId.
3. Update each Book with `authorId` and a migration marker only when appropriate; do not change unrelated fields or Book active state.
4. Reruns must reuse Authors and preserve already valid links. Count skipped records. Names that normalize identically are intentionally matched; the migration cannot determine whether they refer to distinct people and does not produce a collision-specific report.
5. New requests accept `authorId` or the legacy `author` name. Legacy names resolve to an active Author or create a minimal Author to maintain client compatibility.
6. Keep the legacy field during this delivery; removal requires a later approved compatibility window.

## Test Strategy

- Unit tests use `MethodName_Condition_ExpectedResult()` naming and cover domain rules, validation, normalization, and migration idempotence.
- API integration tests use `WebApplicationFactory<Program>` and an ephemeral MongoDB Testcontainer; test index creation, persistence, route status codes, JSON shapes, filtering, and Problem Details.
- Preserve regression tests for existing Book operations.
- Configure coverage collection and a build-enforced threshold of at least 85% for Domain/Application; ensure overall business logic coverage meets the company’s 70% minimum.
- Include a documented coverage command and explain exclusions narrowly; do not exclude untested business code to achieve the threshold.

## Documentation and Delivery

- Produce `research.md`, `data-model.md`, `contracts/openapi.yaml`, `quickstart.md`, and `tasks.md` in this feature folder.
- Update root README and add functional, technical, technical-debt, testing, and QA documentation.
- Update Dockerfile and compose configuration for .NET 10 while preserving MongoDB initialization/data volumes.
- The feature work was delivered directly on `main`; the documentation work on `docs/legacy-api-and-plan` was fully merged. Do not claim that a separate `007-author-management` branch was used. User-provided feature briefs are part of the deliverable and must be tracked.

## Evaluator Remediation Plan

The feature implementation is already integrated on `main`; the historical `007-author-management` branch was not created. Do not imply that work was performed on that branch. The evaluator follow-up is limited to repository artifacts and documentation:

1. Track the user-provided `FEAT-007.md` and `PROMPT.md` so links in this plan and the feature spec resolve in a clean checkout.
2. Remove the superseded Java implementation (`pom.xml`, `src/main/java/`, and `src/test/java/`) because the .NET replacement has passed its full test suite and the selected stack is .NET 10 only.
3. Add a technical-debt register for the known remaining operational and migration limitations, link it from functional/technical documentation, and correct T014's deliverable reference.
4. Remove copied harness setup material from the product README and replace the unassigned-maintainer placeholder with the repository's support route and known repository owner.
5. Clarify that Testcontainers integration tests require Docker socket access, and document the existing coverlet configuration and its 85% Domain/Application line-coverage gate. Do not weaken the gate or claim that a Docker-unavailable run indicates a product test failure.
6. Correct feature metadata and delivery notes to reflect the actual `main` and merged `docs/legacy-api-and-plan` branches; do not create a retroactive feature branch.
7. Re-run the .NET solution tests with Docker access, inspect coverage and links, and verify that no Java project files remain.

## Risks and Mitigations

| Risk | Mitigation |
|---|---|
| Normalized names conflate different people with the same name | Treat normalization as matching only, document the limitation, require production data review, and keep ObjectId as permanent identity |
| Old clients send `author` strings | Continue accepting legacy input and dual-read/write author text while establishing `authorId` |
| Legacy documents have inconsistent or blank author values | Make backfill idempotent, skip blanks explicitly, and report unmapped records |
| OpenAPI UI discloses information in production | Enable interactive UI only in Development |
| MongoDB unique-index creation encounters existing duplicates | Normalize and inspect data before index creation; fail visibly with actionable diagnostics |
| Coverage requirements encourage brittle test-only code | Measure Domain/Application separately and test observable behavior rather than implementation details |

## Complexity Tracking

No constitution violations are identified. The separate Domain/Application/Infrastructure projects are justified by the company’s architecture instructions and persistence boundary; event infrastructure, EF Core, and additional runtime services remain out of scope.

## Follow-up Evaluator Remediation Plan

**Purpose**: Address the latest evaluator feedback with source-backed documentation,
reproducible verification evidence, explicit dependency compliance, and an accurate
delivery record. This is follow-up work; it does not change the historical delivery
branch or claim that previous work was reviewed through a PR.

### Scope and decisions

- Improve `docs/functional.md` with a business narrative for the catalog, Author lifecycle,
  Book/Author association, compatibility behavior, and relevant business decisions.
  Treat the pre-removal Java sources in Git history as evidence. Label any behavior that
  cannot be confirmed from source or tests as unknown rather than presenting it as fact.
- Expand `docs/technical.md` with Mermaid architecture and sequence diagrams for Book
  creation/Author resolution, Author soft deletion, and the backfill. Diagrams must match
  the current code and must not imply integrations that are not implemented.
- Execute the scenarios in `docs/qa-checklist.md` where the available environment permits.
  Record dated evidence and distinguish automated test results from manual acceptance.
  Leave every unexecuted item unchecked.
- Re-run and explain the coverage measurements. The repository's Coverlet configuration
  includes Domain and Application and enforces 85% line coverage. The evaluator's 75.5%
  result has a different or unknown denominator; do not claim equivalence unless its exact
  assemblies, exclusions, and instrumentation are reproduced. If evaluator configuration
  is unavailable, state that explicitly and report local scopes separately.
- Resolve logging and validation guideline compliance before modifying dependencies.
  `COMPANY_GUIDELINES.md` lists Serilog and FluentValidation as standard dependencies, and
  the current constitution requires them for new work unless an approved exception is
  documented. Default to adopting them for this service if no exception is approved.
  Preserve existing response shapes, validation rules, JSON output, and trace correlation.
  Package versions remain centrally managed in `Directory.Packages.props`.
- Add a changelog entry describing only changes actually delivered in this follow-up.
  Do not fabricate a historical branch, PR, QA sign-off, or past changelog.
- Ask the repository owner to confirm the original constitution ratification date. Until
  confirmed, retain its TODO. Remove the temporary Sync Impact Report comment only as part
  of an approved constitution update and before that file is committed.

### Design impact

No domain entities, persistence schemas, API routes, or OpenAPI operations are intended to
change. Reuse the current `data-model.md` and OpenAPI contract. Update the quickstart only
if verification commands or prerequisites change. Any dependency implementation must
preserve the existing public API contract and be covered by the current unit/integration
tests.

### Constitution Check

- **Architecture and API**: documentation changes describe existing .NET 10 Minimal API
  behavior; no route or persistence redesign is proposed.
- **Secure errors and observability**: dependency work must preserve Problem Details,
  JSON logs, and trace ID behavior.
- **Automated verification**: automated coverage and manually executed QA scenarios must
  be reported separately; no threshold may be reduced or met by excluding business code.
- **Documentation and governance**: requirements and historical delivery facts must be
  traceable; unknown ratification date and unexecuted QA must remain explicit.
- **Gate result**: PASS for planning. Dependency adoption remains a gated implementation
  decision; an exception requires repository-owner approval and written rationale.

### Acceptance criteria

1. Functional documentation includes a readable business narrative and an explicit
   separation of confirmed legacy behavior from unknowns.
2. Technical documentation contains accurate, readable diagrams for the actual runtime
   and requested flows.
3. Each checked QA item has dated evidence; unexecuted manual/operational checks remain
   unchecked and are listed as pending.
4. Coverage documentation identifies the local assembly scope, threshold, command,
   result, and evaluator comparison limitation. The 75.5% evaluator number is not
   represented as reproduced without matching instrumentation.
5. Serilog/FluentValidation are adopted and verified, or an approved exception is linked
   and justified. Existing API validation/error behavior remains compatible.
6. Changelog, README, test guide, technical debt, OpenAPI, and feature docs remain
   mutually consistent; no historical branch/PR/QA claim is invented.
7. Final .NET 10 build/test and configured coverage gates pass with Docker access. If Docker
   is unavailable, the integration verification is reported as blocked, not passed.

### Risks and mitigations

| Risk | Mitigation |
|---|---|
| Git history does not prove a business rule described in existing docs | Cite only source/test-confirmed behavior; label uncertainty and avoid invented legacy semantics |
| Manual QA cannot be performed in the available environment | Leave affected checks open and record the missing prerequisite and evidence owner |
| Evaluator's coverage denominator is unavailable | Request evaluator instrumentation details; report reproducible local scopes separately |
| Serilog/FluentValidation change logging or validation behavior | Add dependencies through central package management, preserve contracts, and run focused plus full tests |
| Existing uncommitted changes are mixed with follow-up work | Inspect and preserve them before any future branch/commit; do not stash, discard, commit, or push them implicitly |
| Constitution ratification date remains unknown | Keep the explicit TODO and report the governance item as pending owner confirmation |

### Delivery strategy

This plan does not create a branch, commit, or push. For implementation, use a new
remediation branch only after the existing dirty worktree has been reviewed and safely
separated. If a PR is opened for the follow-up, describe it as new remediation work; do
not imply it changes the historical delivery record.
