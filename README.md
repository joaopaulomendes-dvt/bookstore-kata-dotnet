# Bookstore API

A .NET 10 Minimal API for bookstore inventory and Author profiles, backed by MongoDB. The service preserves the legacy Book API while adding stable Author identities, profile metadata, soft deletion, and Book-to-Author references.

## Prerequisites

- .NET 10 SDK
- Docker with Docker Compose (for local MongoDB and integration tests)
- `dotnet` CLI

## Getting started

Start the API and MongoDB:

```sh
docker compose up --build
```

The API listens on `http://localhost:8080`; MongoDB listens on `localhost:27017`. To run the API outside Compose, configure MongoDB and start the project:

```sh
export MongoDb__ConnectionString=mongodb://localhost:27017
export MongoDb__DatabaseName=bookstore
dotnet run --project src/Bookstore.Api
```

The connection string may alternatively be supplied as `MONGODB_URI`. The API fails at startup if neither setting is present. Do not commit credentials or production connection strings.

## API documentation

- OpenAPI JSON: `http://localhost:8080/openapi/v1.json`
- Scalar UI in Development: `http://localhost:8080/scalar/v1`
- The design-first contract is in [specs/007-author-management/contracts/openapi.yaml](./specs/007-author-management/contracts/openapi.yaml).
- No staging environment or staging OpenAPI URL is configured for this repository.

## Testing

Run unit and MongoDB-backed API integration tests:

```sh
dotnet test
```

Docker must be available to Testcontainers, and the Docker socket must be accessible to the test process. Without Docker access, MongoDB-backed integration tests fail before exercising the API; this is an environment prerequisite, not an application test failure.

Coverlet is configured in `tests/Bookstore.Tests/Bookstore.Tests.csproj` to collect Domain and Application line coverage and enforce an 85% threshold:

```sh
dotnet test --no-restore
```

See [docs/testing.md](./docs/testing.md) for focused runs and report locations.

## Architecture and design notes

- `Bookstore.Domain` contains Book and Author invariants.
- `Bookstore.Application` coordinates use cases through repository interfaces.
- `Bookstore.Infrastructure` implements MongoDB repositories, indexes, and the Author backfill.
- `Bookstore.Api` hosts Minimal API route groups, OpenAPI, trace correlation, and RFC 7807 errors.
- MongoDB ObjectIds remain the persisted identity; API DTOs expose their hexadecimal string representation.
- `Book.authorId` is the stable reference. The legacy `author` string remains for compatibility.
- Author deletion is soft; linked Books remain unchanged.

See [docs/functional.md](./docs/functional.md), [docs/technical.md](./docs/technical.md), [docs/technical-debt.md](./docs/technical-debt.md), and [specs/007-author-management/plan.md](./specs/007-author-management/plan.md).

## Maintainers

Repository owner: [luismatiasmendes-dvt](https://github.com/luismatiasmendes-dvt). For support or to report an issue, use the [repository issue tracker](https://github.com/luismatiasmendes-dvt/bookstore-kata-dotnet/issues).
