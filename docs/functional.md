# Functional Documentation

## Purpose

The Bookstore API manages the bookstore catalog and the Author profiles associated with it. The API currently has no authentication or authorization; deployments must be protected by the network or gateway until an authentication requirement is defined.

## Book operations

| Operation | Route | Behavior |
|---|---|---|
| Check service and MongoDB availability | `GET /api/books/health` | Returns 200 when MongoDB responds to a ping. |
| List books | `GET /api/books` | Returns all books, optionally filtered by category, Author ObjectId, or the legacy exact Author name. |
| Retrieve a book | `GET /api/books/{id}` | Returns a matching book or 404. IDs must be 24-character hexadecimal ObjectIds. |
| Legacy single-book lookup | `GET /api/books/single?id=…`, `?name=…`, or `?author=…` | Retained for compatibility; new clients should use the ID route. |
| Create a book | `POST /api/books` | Requires a nonblank title. Accepts an Author ID or legacy Author name. |
| Update inventory state | `PUT /api/books` | Updates quantity and active state for the requested ID. Deactivation records the current UTC time. |

Supported categories are Fiction, Non-Fiction, Science, Biography, Children, and Programming. Category names are case-insensitive. An unsupported category on list or create is a 400 response. If no category is supplied, all categories are listed.

New Book associations should use `authorId`. The legacy `author` string remains in persisted data and responses. A legacy author name on create is normalized to locate an active Author; when none exists, the API creates a minimal Author profile. Supplying an `authorId` takes precedence and must identify an active Author. Books linked to an inactive Author remain in the catalog and can still be filtered by that ID.

## Author operations

| Operation | Route | Behavior |
|---|---|---|
| Create profile | `POST /api/authors` | Creates an active Author and returns 201. |
| List profiles | `GET /api/authors` | Returns active Authors, sorted by normalized name. |
| Retrieve profile | `GET /api/authors/{id}` | Returns an active Author or 404. |
| Soft-delete profile | `DELETE /api/authors/{id}` | Marks the Author inactive and returns 204. Repeating deletion is safe. |

Set `includeInactive=true` on list or get to include an inactive Author. Deletion never removes a profile, changes a Book's active state, or removes a Book's `authorId`.

Author names are required, trimmed, and limited to 255 characters. Duplicate detection ignores case and repeated/leading/trailing whitespace. Website values must be absolute HTTP or HTTPS URLs. Email values must be valid email addresses. Biography, birth date, nationality, website, and email are optional.

## Errors and observability

Validation errors return 400, missing resources return 404, normalized duplicate Author names return 409, and unexpected failures return a generic 500 response. Errors use `application/problem+json`; `instance` and `traceId` identify the request. The API also returns the ID in `X-Trace-Id`. Internal exceptions are logged without exposing stack traces to clients.

## Legacy behavior and known limitations

- The original service stores Author names as strings and does not have first-class Author records.
- Its author filter was a TODO that threw an exception; the new service implements filtering.
- The legacy ID lookup compared Java strings by reference rather than value; the new lookup uses ObjectId equality.
- There is no authentication, paging, bulk update, Book delete route, or merge workflow for duplicate people.
- Normalized names are matching aids, not permanent identity. Human review may be needed when same-name people are encountered.
