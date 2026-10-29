# Feature Specification: Book Author Management

**Feature Branch**: `007-author-management`
**Status**: Approved
**Input**: `FEAT-007.md` and `PROMPT.md`

## Summary

Document the existing bookstore API, replace its Java 11/Jersey implementation with the company-standard .NET 10 ASP.NET Core Minimal API while preserving book behavior, and add first-class Author management and Book-to-Author relationships.

## User Scenarios & Testing

### User Story 1 - Modernized, documented bookstore API (Priority: P1)

As an API consumer and maintainer, I want the existing book service documented and migrated to the supported .NET stack without losing its current capabilities, so that the service remains usable while becoming supportable and testable.

**Why this priority**: Author management builds on a dependable, documented API and the company requires .NET 10.

**Independent Test**: Start the .NET API with MongoDB and exercise the existing health, list, get, create, and update book operations. Verify the generated OpenAPI document describes the operations and that API integration tests pass.

**Acceptance Scenarios**:
1. **Given** a running API and MongoDB, **When** a client invokes the existing book operations, **Then** book data and status behavior remain compatible with the documented legacy contract.
2. **Given** the .NET service is running in Development, **When** a client requests the OpenAPI document or API reference UI, **Then** the current contract and interactive reference are available.
3. **Given** a production environment, **When** a client requests interactive API documentation, **Then** the UI is not exposed.

### User Story 2 - Author profiles (Priority: P1)

As a bookstore catalog manager, I want to create, list, and retrieve Author profiles so that author metadata is managed once and can be associated with books.

**Why this priority**: Author profiles are the core capability requested by FEAT-007.

**Independent Test**: Create an Author, retrieve it by ID, and list active Authors. Verify required name validation, optional profile fields, generated identity, timestamps, and duplicate-name handling.

**Acceptance Scenarios**:
1. **Given** a valid profile with a unique name, **When** a client creates an Author, **Then** the API stores it as active and returns its stable ID and timestamps.
2. **Given** a profile with no name or a name longer than 255 characters, **When** a client creates an Author, **Then** the API returns a validation Problem Details response.
3. **Given** an existing active Author, **When** a client lists or retrieves Authors, **Then** the matching profile is returned.
4. **Given** an existing Author with the same normalized name, **When** a client creates another one, **Then** the API reports a conflict rather than creating a duplicate.

### User Story 3 - Author soft deletion (Priority: P1)

As a catalog manager, I want to soft-delete an Author and explicitly include inactive profiles when needed, so that the author is no longer active without losing historical relationships.

**Why this priority**: The requested lifecycle includes soft deletion, and it must not destroy book references.

**Independent Test**: Soft-delete an Author and verify it is absent from default list/get operations, available with `includeInactive=true`, and linked books remain active and unchanged.

**Acceptance Scenarios**:
1. **Given** an active Author, **When** a client deletes the Author resource, **Then** the Author is marked inactive and its update timestamp changes.
2. **Given** an inactive Author, **When** a client lists or retrieves Authors without `includeInactive=true`, **Then** it is not returned.
3. **Given** an inactive Author, **When** a client lists or retrieves Authors with `includeInactive=true`, **Then** it is returned with `active=false`.
4. **Given** books linked to an Author, **When** the Author is soft-deleted, **Then** those books remain active and retain their Author IDs.

### User Story 4 - Book association and author filtering (Priority: P1)

As a catalog consumer, I want Books to reference an Author and be searchable by that relationship, so that author details are not duplicated as the durable relationship.

**Why this priority**: The primary data-model limitation identified by FEAT-007 is the lack of stable, deduplicated author relationships.

**Independent Test**: Create or migrate an Author and linked Books, then filter the catalog by Author ID. Verify references remain stable across author-name changes and the migration is safe to rerun.

**Acceptance Scenarios**:
1. **Given** a valid active Author ID, **When** a client creates a Book using that ID, **Then** the Book stores the Author reference and preserves the display name for legacy consumers.
2. **Given** legacy Books with author names, **When** the migration runs, **Then** Books are linked to Authors created or matched using normalized names, and the original author text is retained during transition.
3. **Given** a completed migration, **When** the migration runs again, **Then** it does not create duplicate Authors or change existing links.
4. **Given** a catalog with linked Books, **When** a client filters by Author ID, **Then** only Books linked to that Author are returned.
5. **Given** an inactive Author, **When** a client filters Books by the Author ID, **Then** associated Books remain discoverable and unchanged.

## Edge Cases

- Malformed ObjectId strings return a client error without an unhandled database exception.
- Unknown Author IDs return not found; inactive Authors are unavailable by default unless explicitly included.
- Creating an Author with a case/whitespace-normalized duplicate name returns conflict.
- Books with a missing, blank, or legacy author value remain readable; migration reports and leaves records with no usable author name unlinked.
- Migration failure can be retried without duplicate Authors or destructive Book updates.
- Database errors return RFC 7807 Problem Details without stack traces or credentials.
- Book and Author persistence operations honor request cancellation.

## Requirements

### Functional Requirements

- **FR-001**: The service MUST target `net10.0` and implement HTTP endpoints with ASP.NET Core Minimal APIs.
- **FR-002**: The service MUST preserve and document the legacy health, list, get, create, and update Book operations.
- **FR-003**: The service MUST expose an OpenAPI 3.0-or-newer document containing request/response schemas and all supported status/error responses.
- **FR-004**: The service MUST expose an interactive OpenAPI UI only in non-production development environments.
- **FR-005**: The service MUST implement Author create, list, get, and soft-delete operations using plural REST resource paths.
- **FR-006**: An Author MUST have a generated stable identifier, a required name of at most 255 characters, optional biography, website, birth date, nationality, and email, an active flag defaulting to true, and creation/update timestamps.
- **FR-007**: Author names MUST be unique after trimming and case normalization; duplicate creation MUST return a conflict response.
- **FR-008**: Normal Author list and get requests MUST return active Authors only; `includeInactive=true` MUST include soft-deleted Authors.
- **FR-009**: Soft deletion MUST only deactivate the Author and MUST NOT deactivate or remove linked Books.
- **FR-010**: Books MUST store an Author ID reference when an Author is associated; the API MUST expose IDs as strings while MongoDB stores ObjectIds.
- **FR-011**: The service MUST provide an idempotent backfill for legacy author-name data, deduplicating by normalized name and retaining the legacy author string during transition.
- **FR-012**: The Book catalog MUST support filtering by Author ID; existing author-name filtering must continue to work where the legacy contract provides it.
- **FR-013**: Request and response DTOs MUST use C# record types; endpoint handlers MUST be asynchronous and accept cancellation tokens.
- **FR-014**: All API errors MUST use RFC 7807 Problem Details; raw exception details MUST NOT be returned to clients.
- **FR-015**: The service MUST emit structured JSON logs with a request trace ID and propagate the trace ID to downstream requests where applicable.
- **FR-016**: Unit and API/database integration tests MUST cover core behavior; Domain and Application code MUST reach at least 85% line coverage and business logic MUST meet the 70% minimum.
- **FR-017**: README, functional/technical documentation, OpenAPI, test guide, implementation plan, and QA checklist MUST be updated consistently.

### Key Entities

- **Author**: A uniquely named, soft-deletable author profile with optional metadata and timestamps.
- **Book**: Existing catalog item with preserved fields and behavior, plus an optional stable Author reference and transitional legacy author text.

## Success Criteria

- **SC-001**: The service builds and runs on .NET 10 using the .NET CLI.
- **SC-002**: Automated integration tests pass for all documented legacy Book operations and all Author/Book scenarios above.
- **SC-003**: OpenAPI documents all routes, payloads, and supported errors; API UI is unavailable in production.
- **SC-004**: Coverage is at least 85% for Domain/Application code and at least 70% for business logic.
- **SC-005**: Re-running the author backfill does not create duplicate Authors or modify established Book links.
- **SC-006**: README, functional/technical documentation, OpenAPI, tests, and QA checklist match the delivered behavior.

## Assumptions

- MongoDB remains the persistence platform; the refactor does not introduce a relational database or EF Core.
- Email is the requested contact field; phone and postal address are out of scope until specifically requested.
- Normalized author name is a migration and duplicate-detection key only. Persisted Book relationships use generated ObjectIds.
- The API currently has no authentication requirement; the refactor documents this rather than inventing an authentication scheme.
- A Book request using a legacy author name resolves or creates a minimal Author record when needed, preserving existing clients while establishing the relationship.
