# Technical Architecture

## Runtime and dependencies

- .NET 10 Minimal API hosted by ASP.NET Core.
- MongoDB 7.0 using the official MongoDB C# driver.
- xUnit, `WebApplicationFactory<Program>`, and Testcontainers for integration testing.
- Built-in OpenAPI generation; Scalar is mapped only in Development.

## Project layout

```text
src/
├── Bookstore.Api/             # Route groups, request/response records, middleware
├── Bookstore.Application/     # Use cases and repository contracts
├── Bookstore.Domain/          # Book and Author entities and business rules
└── Bookstore.Infrastructure/  # MongoDB adapters, indexes, migration
tests/Bookstore.Tests/         # Unit and MongoDB-backed integration tests
tools/Bookstore.Migration/     # One-shot legacy Author backfill executable
```

The Author aggregate owns profile identity and lifecycle. A Book stores an optional `authorId` reference and a compatibility `author` display value. Soft deletion is intentionally independent: it does not cascade to Books. Repository interfaces keep application use cases independent of MongoDB.

## Persistence

The existing `bookstore.books` collection is retained. The API adds `bookstore.authors`, a unique ascending index on `normalizedName`, and an ascending index on `books.authorId`. `_id` and `authorId` are BSON ObjectIds; JSON contracts expose 24-character hexadecimal strings.

The Author backfill reads legacy Book documents, normalizes nonblank author names, creates or reuses Author records, and links Books without changing their legacy author value or other catalog fields. It preserves valid existing references, repairs malformed `authorId` values using a conditional update, and counts skipped documents. The migration is safe to rerun.

Run the one-shot backfill with `MONGODB_URI` or `MongoDb__ConnectionString` set:

```sh
MONGODB_URI=mongodb://localhost:27017 dotnet run --project tools/Bookstore.Migration
```

Back up the database and rehearse the backfill against a representative copy before production execution.

## Configuration and operations

- `MongoDb:ConnectionString` (environment form `MongoDb__ConnectionString`) or `MONGODB_URI`: required, never logged.
- `MongoDb:DatabaseName` (environment form `MongoDb__DatabaseName`): optional; defaults to `bookstore`.
- `ASPNETCORE_URLS`: controls listener URLs; Compose maps port 8080.

The API initializes indexes at startup and fails explicitly if MongoDB settings are missing or index initialization fails. JSON console logging includes a request scope with the trace ID. Middleware returns RFC 7807 Problem Details with a generic client response for unexpected exceptions.

## API contract

The maintained contract is [contracts/openapi.yaml](../specs/007-author-management/contracts/openapi.yaml). Runtime OpenAPI JSON is available at `/openapi/v1.json`; the Development-only Scalar UI is available at `/scalar/v1`.

## Security and privacy

There is no authentication/authorization layer in the current requirements. Do not expose the service publicly without a gateway or a separately approved authentication design. Keep production credentials in a secret store/environment injection, not in source control. Error logs do not include client-provided validation details; internal failures retain server-side exception details.
