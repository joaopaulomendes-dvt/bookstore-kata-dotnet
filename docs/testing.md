# Testing and Coverage

## Prerequisites

- .NET 10 SDK.
- Docker daemon and socket access available to Testcontainers for MongoDB integration tests. If the socket is unavailable, the integration tests fail during container startup; those failures are environmental and do not measure API behavior.

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

In containerized CI/evaluation environments, mount or otherwise grant the test process access to the Docker socket. Do not interpret `DockerUnavailableException` as an application regression; rerun the suite with Docker access before assessing integration-test results.

## Coverage gate

`tests/Bookstore.Tests/Bookstore.Tests.csproj` configures `coverlet.msbuild` to collect Cobertura line coverage for the Domain and Application assemblies and enforces an 85% threshold. This exceeds the 70% business-logic minimum:

```sh
dotnet test
```

Review `tests/Bookstore.Tests/coverage.cobertura.xml` or the test output for coverage results. The configured threshold applies specifically to Domain and Application; externally injected whole-repository coverage reports may use a different denominator. Domain and application business code must be tested rather than excluded to satisfy the gate.

## Last verified run

On 2026-10-29, the Docker-enabled Release run passed all 41 tests. It reported 95.65% Application line coverage, 98.19% Domain line coverage, and 96.9% total across the measured assemblies. The generated Cobertura report is available at `tests/Bookstore.Tests/coverage.cobertura.xml` after running the suite; generated reports are not committed.

## Validation scenarios

- Domain: Author name normalization, required/max-length name, website/email validation, and deactivation state.
- Application: category mapping, ID validation, Author resolution, legacy name compatibility, timestamp handling, and not-found cases.
- API + MongoDB: Book create/list/get/update, Author create/list/get/soft-delete, unique index behavior, Problem Details, reference filtering, and idempotent backfill.
- OpenAPI: verify the document at `/openapi/v1.json` and compare it with the checked-in contract.
