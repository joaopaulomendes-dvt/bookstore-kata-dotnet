# Research: Search Books by Author Name

## Existing behavior

**Decision**: Add a separate `authorName` query criterion for partial-name search and
leave the existing `author` query criterion as an exact legacy match.

**Evidence**:

- `BookEndpoints.ListAsync` currently binds `category`, `authorId`, and `author`.
- `BookService.ListAsync` validates category and ObjectId input, then delegates all list
  criteria to `IBookRepository`.
- `MongoBookRepository.ListAsync` adds an exact equality filter for the stored `author`
  field when the legacy `author` query is supplied.
- The list route is already documented in the checked-in OpenAPI contract at
  `specs/007-author-management/contracts/openapi.yaml`.

## Search semantics

**Decision**: Match the new term as a case-insensitive, literal substring of the stored
Book Author display-name field; trim surrounding whitespace, treat an empty term as
omitted, and reject terms longer than 255 characters.

**Rationale**: The Book document already carries the display name for both linked and
legacy Books. This supports partial name discovery without changing the data model,
backfill, or compatibility semantics. Escaping search input prevents regex characters
from becoming query operators.

**Alternatives considered**:

- Change the legacy `author` criterion from exact to partial matching: rejected because
  it changes established behavior for existing clients.
- Resolve partial Author names through the Author repository and search by IDs: rejected
  because it would add a second collection query and would not cover unlinked legacy
  Books as directly.
- Add a separate search endpoint, pagination, or a new database index: out of scope for
  this additive filter and not required by the current list API.

## Filter composition and lifecycle

**Decision**: Combine `authorName` with category, Author ID, and legacy exact `author`
criteria using AND semantics. Do not filter by Author activation state.

**Rationale**: Existing list filters are composed as MongoDB predicates. Books remain
catalog records even after their Author profile is deactivated, and legacy Books may
have no stable `authorId`.

## Validation and tests

**Decision**: Validate the 255-character maximum in the Application service and cover
both direct use-case validation and actual MongoDB behavior through the existing xUnit
and Testcontainers test infrastructure.

**Rationale**: The Application layer already validates category and Author ID filters;
the same boundary is the narrowest place to reject invalid query input before database
access.
