# Functional Documentation

## What the service does

The Bookstore API supports a bookstore catalog where staff and consuming applications
can create books, find books, and update inventory availability. The Author feature
extends that catalog by giving each author a managed profile instead of relying only on
the name copied into each book. This makes an author a reusable catalog identity and lets
clients find related books by a stable identifier.

The API currently has no authentication or authorization. Until an approved access-control
design exists, deployments must be protected by a trusted network or gateway and must not
be exposed directly to untrusted users.

## Catalog and inventory

A book is the catalog record used by clients to describe a title and its inventory state.
The existing book operations are retained for compatibility: clients can list books,
retrieve one by identifier or through the older lookup route, create a book, and update
quantity and active state. Deactivating a book records an inactive date. This API does not
provide a book deletion operation.

Books may be listed by category, Author ID, or the legacy exact Author name. Category
names are case-insensitive in the current API. An unsupported category is rejected with
400 instead of silently returning the whole catalog. When no category is supplied, all
categories are included.

For new associations, clients should send `authorId`. The stable reference points to an
Author profile; the existing `author` display string is retained in stored data and
responses for compatibility with older consumers. An explicit Author ID must identify an
active Author. If a client sends only the legacy author name, the API resolves it against
an active profile by normalized name or creates a minimal active profile when no matching
profile exists. If both values are sent, the explicit ID takes precedence.

## Author profiles and lifecycle

An Author profile represents a person credited with one or more books. The name is
required, trimmed, and limited to 255 characters. The API rejects duplicate profiles
whose names match after trimming, collapsing repeated whitespace, and case normalization.
This normalization supports matching; it is not the permanent identity of a person.

Biography, website, birth date, nationality, and email are optional profile details.
Websites must be absolute HTTP or HTTPS URLs, and email values must be valid addresses.
The service assigns an ID and creation/update timestamps when a profile is created.

Authors can be listed and retrieved while active. Deletion is a soft-delete: the record
remains stored with `active=false`, and the update timestamp changes. By default, inactive
profiles are omitted from list and get operations. A client can request
`includeInactive=true` when it needs to inspect them. Deactivation is idempotent and does
not delete, deactivate, or rewrite associated books. Existing books remain discoverable
through their stored Author ID, including after the Author is inactive.

There is no merge or reactivation workflow. If two real people share a normalized name,
the current duplicate rule cannot determine that they are different people; source data
may require human review.

## Legacy behavior and compatibility

The Java implementation is no longer in the working tree. The following legacy details
are confirmed by the controller and service source preserved in Git history at the parent
of commit `8ec79aa`:

- The Java controller exposed a `/books` resource with health, list, single lookup, create,
  and update operations. The exact deployed URL prefix beyond that resource path is not
  confirmed here.
- Its health operation returned 200 without checking MongoDB.
- Listing accepted a category. Category matching was case-sensitive; an unknown category
  fell back to the full book list.
- The single lookup accepted ID, title, or author. Author filtering was explicitly
  unimplemented and threw a runtime exception.
- Creating a book proceeded when title was non-null; a missing title returned 404.
  Updating proceeded when an ID was supplied; a missing ID returned 404.
- The service populated missing persisted quantity and category values as 0 and active as
  false when reading legacy MongoDB documents.
- Deactivation during update set the inactive date to the current date.
- Title lookup used Java string value equality, but ID lookup used reference equality.
  That is a confirmed implementation defect; whether any consumer depended on it is
  unknown.

The .NET API intentionally improves some of these behaviors: its health route checks
MongoDB, invalid categories return 400, category matching is case-insensitive, author
filtering is implemented, identifiers use ObjectId value semantics, and invalid input is
reported as Problem Details. The available source does not establish how production
clients handled the legacy ID defect, how null response entities were serialized in every
deployment, or the exact external route prefix. These points are not treated as confirmed
business requirements.

## Operations reference

| Operation | Route | Current behavior |
|---|---|---|
| Check API and MongoDB availability | `GET /api/books/health` | Returns 200 when MongoDB responds to a ping. |
| List books | `GET /api/books` | Returns books, optionally filtered by category, Author ObjectId, or legacy exact Author name. |
| Retrieve a book | `GET /api/books/{id}` | Returns a matching book or 404; IDs must be 24-character hexadecimal ObjectIds. |
| Legacy single-book lookup | `GET /api/books/single?id=…`, `?name=…`, or `?author=…` | Kept for compatibility; new clients should use the ID route. |
| Create a book | `POST /api/books` | Requires a nonblank title; accepts an Author ID or legacy Author name. |
| Update inventory state | `PUT /api/books` | Updates quantity and active state; deactivation records the current UTC time. |
| Create Author profile | `POST /api/authors` | Creates an active Author and returns 201. |
| List Author profiles | `GET /api/authors` | Returns active Authors sorted by normalized name. |
| Retrieve Author profile | `GET /api/authors/{id}` | Returns an active Author or 404. |
| Soft-delete Author profile | `DELETE /api/authors/{id}` | Marks the Author inactive and returns 204; repeating deletion is safe. |

Supported categories are Fiction, Non-Fiction, Science, Biography, Children, and
Programming. If category is omitted, the API lists all categories.

## Errors and observability

Validation errors return 400, missing resources return 404, normalized duplicate Author
names return 409, and unexpected failures return a generic 500 response. Errors use
`application/problem+json`; `instance` and `traceId` identify the request. The API also
returns the trace identifier in `X-Trace-Id`. Internal exceptions are logged without
exposing stack traces to clients.

## Known limitations

- The API has no built-in authentication or authorization.
- There is no paging, bulk update, Book delete route, or workflow for merging duplicate
  people.
- The legacy `author` string and exact-name filter remain during the compatibility
  transition. Their removal requires an agreed client migration window.
- A name is a matching aid, not identity; same-name people may need human review.
- See the [technical-debt register](./technical-debt.md) for operational limitations and
  follow-up actions.
