# Feature Specification: Search Books by Author Name

**Feature Branch**: `feature/author-book-search`

**Created**: 2026-10-30

**Status**: Implemented; awaiting review

**Input**: User request: “Implement book search by author, with its own specification and tests. Update the documentation too.”

## User Scenarios & Testing

### User Story 1 - Find catalog books by an author name (Priority: P1)

As a bookstore catalog consumer, I want to find books using all or part of an author's name, so that I can discover the author's books without knowing a Book ID or Author ID.

**Why this priority**: Author-based catalog discovery is the primary user value requested by this feature.

**Independent Test**: Add multiple books with different author names, search using a case-insensitive fragment of one name, and verify that only books whose author display name contains that fragment are returned.

**Acceptance Scenarios**:

1. **Given** books by “Octavia Butler” and “Terry Pratchett”, **When** a consumer searches for `butler`, **Then** the results contain the Octavia Butler books and exclude the Terry Pratchett books.
2. **Given** a book whose author name contains a literal special character such as `C++`, **When** a consumer searches for that literal name fragment, **Then** the search treats the input as text and returns the matching book.
3. **Given** a valid author-name search with a category filter, **When** the catalog is searched, **Then** only books matching both filters are returned.
4. **Given** no book matches the supplied author-name fragment, **When** the catalog is searched, **Then** the response is an empty collection.

### User Story 2 - Preserve existing author filters (Priority: P1)

As an existing API consumer, I want current Author ID and legacy exact-name filters to retain their behavior, so that adding name search does not break existing catalog integrations.

**Why this priority**: Search is additive and must not regress stable-reference or compatibility clients.

**Independent Test**: Run the existing Author ID and exact legacy-name filter scenarios alongside the new partial-name scenario and compare their results and error behavior.

**Acceptance Scenarios**:

1. **Given** books linked to different Author IDs, **When** a consumer filters by `authorId`, **Then** only books linked to that ID are returned.
2. **Given** books with legacy author strings, **When** a consumer uses the existing exact `author` filter, **Then** exact-match behavior is unchanged.
3. **Given** a consumer supplies category, Author ID, exact legacy name, and/or partial author-name criteria, **When** multiple criteria are supplied, **Then** the results satisfy every supplied criterion.

## Edge Cases

- An omitted or whitespace-only author-name search does not add a filter.
- Search terms longer than 255 characters are rejected as invalid input.
- Search values containing regular-expression metacharacters are treated literally.
- Matching is case-insensitive and includes a match anywhere within the stored Author display name.
- Inactive Author profiles do not hide already-associated Books from the catalog.
- Database failures continue to use the API's standard Problem Details response.

## Requirements

### Functional Requirements

- **FR-001**: The catalog MUST allow a consumer to search Books by a non-empty fragment of the Author display name.
- **FR-002**: Author-name matching MUST ignore letter case and MUST match the fragment anywhere within the name.
- **FR-003**: Search input MUST be interpreted as literal text; metacharacters MUST NOT change the meaning of the search.
- **FR-004**: A supplied author-name search MUST be limited to 255 characters; longer values MUST receive the standard invalid-request response.
- **FR-005**: An omitted or whitespace-only author-name search MUST behave as if no author-name filter was supplied.
- **FR-006**: Author-name search MUST compose with the existing category, Author ID, and exact legacy `author` filters by returning only Books satisfying all supplied filters.
- **FR-007**: Existing Author ID and exact legacy-name filtering behavior MUST remain compatible.
- **FR-008**: Search MUST include Books associated with inactive Authors and legacy Books that have only a stored Author display name.
- **FR-009**: API documentation MUST describe the new search input, matching rules, maximum length, and its interaction with existing filters.
- **FR-010**: Automated tests MUST cover matching, non-matching, special characters, filter composition, input boundaries, and compatibility filters.

### Key Entities

- **Book**: A catalog entry containing a display name for its Author and, when available, a stable Author identifier.
- **Author**: A managed catalog profile associated with Books; profile activation does not determine whether its existing Books are searchable.
- **Author-name search**: A literal text fragment used to find Books by the stored Author display name.

## Success Criteria

### Measurable Outcomes

- **SC-001**: A consumer can retrieve matching Books using a case-insensitive fragment from an Author name, and unrelated Authors' Books are excluded.
- **SC-002**: All existing Author ID and exact legacy-name filter tests continue to pass without changed results.
- **SC-003**: Search returns an empty collection, rather than an error, when no Books match.
- **SC-004**: All new search acceptance scenarios are covered by automated unit or API integration tests.
- **SC-005**: Search documentation and the checked-in API contract state the same request parameter and matching behavior.

## Assumptions

- The existing list-books operation remains the entry point; author-name search is an additive query filter rather than a separate resource.
- The new query parameter is named `authorName`; the existing `author` query parameter remains an exact legacy filter.
- A name fragment is a literal, case-insensitive substring, and all supplied catalog filters are combined with AND semantics.
- Search operates on the Author display name already stored with each Book. It does not add Author profile editing, pagination, ranking, or fuzzy spelling correction.
