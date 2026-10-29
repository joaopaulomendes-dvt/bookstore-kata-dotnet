# Quickstart: Book Author Management

## Prerequisites

- .NET 10 SDK
- Docker Desktop or another Docker-compatible runtime for MongoDB/Testcontainers
- `dotnet` CLI

## Start the local dependencies

From the repository root:

```sh
docker compose up -d mongodb
```

Configure the API with a MongoDB connection string for the `bookstore` database, then run:

```sh
dotnet run --project src/Bookstore.Api
```

The local API should listen on port 8080. In Development, open the Scalar/Swagger reference exposed by the API and fetch `/openapi/v1.json`.

## Validate author lifecycle

1. Create an Author with a unique `name` and optional `biography`, `website`, `birthDate`, `nationality`, and `email` using `POST /api/authors`.
2. Record the string `id` from the response.
3. Retrieve it with `GET /api/authors/{id}` and confirm the optional fields and timestamps.
4. List active Authors with `GET /api/authors`.
5. Soft-delete it using `DELETE /api/authors/{id}`.
6. Confirm default list/get no longer returns it, then repeat with `includeInactive=true` and confirm `active` is false.

## Validate Book linkage and migration

1. Create a Book using the Author `id` as `authorId`; confirm the returned Book keeps the reference and legacy display name.
2. Request `GET /api/books?authorId={id}` and confirm only associated Books are returned.
3. Run the legacy backfill against a disposable copy of MongoDB data, then run it a second time.
4. Confirm the second run creates no duplicate Authors and does not change Book IDs, active state, or established references.

## Run automated checks

```sh
dotnet test
```

The test suite includes unit tests and `WebApplicationFactory<Program>` integration tests backed by a disposable MongoDB Testcontainer. Verify the generated coverage report meets the Domain/Application threshold of 85% and the business-logic minimum of 70%.
