# Testing and Coverage

## Prerequisites

- .NET 10 SDK.
- Docker daemon available to Testcontainers for MongoDB integration tests.

## Run tests

Run all unit and API integration tests from the repository root:

```sh
dotnet test
```

Run only unit or integration tests:

```sh
dotnet test --filter FullyQualifiedName~Bookstore.Tests.Unit
dotnet test --filter FullyQualifiedName~Bookstore.Tests.Integration
```

The MongoDB collection fixture starts one disposable MongoDB 7.0 container. Each API test receives a unique database; the fixture serializes tests that share the container and disposes it when the collection completes.

## Coverage gate

The test project collects Cobertura line coverage and enforces 85% coverage across the Domain and Application assemblies (the repository architecture target, exceeding the 70% business-logic minimum):

```sh
dotnet test
```

Review `tests/Bookstore.Tests/coverage.cobertura.xml` or the test output for coverage results. Domain and application business code must be tested rather than excluded to satisfy the gate.

## Validation scenarios

- Domain: Author name normalization, required/max-length name, website/email validation, and deactivation state.
- Application: category mapping, ID validation, Author resolution, legacy name compatibility, timestamp handling, and not-found cases.
- API + MongoDB: Book create/list/get/update, Author create/list/get/soft-delete, unique index behavior, Problem Details, reference filtering, and idempotent backfill.
- OpenAPI: verify the document at `/openapi/v1.json` and compare it with the checked-in contract.
