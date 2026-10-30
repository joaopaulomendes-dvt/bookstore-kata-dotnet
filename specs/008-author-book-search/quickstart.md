# Quickstart: Search Books by Author Name

## Prerequisites

- .NET 10 SDK and Docker with socket access for Testcontainers.
- Repository dependencies restored by `dotnet test`.

## Exercise the API

Create a Book with an Author name, then list books using a case-insensitive fragment:

```http
GET /api/books?authorName=butler
```

The response is the standard Book collection and contains books whose stored Author
display name includes `butler`. The existing exact filter remains available:

```http
GET /api/books?author=Octavia%20Butler
```

## Verify

Run the complete test suite from the repository root:

```sh
dotnet test Bookstore.sln --configuration Release
```

The suite includes unit validation tests and API integration tests backed by a disposable
MongoDB Testcontainer. Verify that partial matching is case-insensitive, query input is
literal, supplied filters combine, empty results return an empty array, and existing
Author ID and exact-name filters remain unchanged.

## Latest verification

On 2026-10-30, the Docker-enabled Release run passed all 59 tests (0 failed, 0 skipped).
Coverlet measured 95.79% line coverage for `Bookstore.Application`, 98.19% for
`Bookstore.Domain`, and 96.95% combined, above the configured 85% threshold. An initial
run reported 0% for Domain and failed the threshold; two subsequent full-suite runs
with the same configuration returned the successful values, so the initial result was
not reproducible. The evaluator's separate 75.5% result remains a different denominator
and is not represented by these local measurements; see
[`docs/testing.md`](../../docs/testing.md).
