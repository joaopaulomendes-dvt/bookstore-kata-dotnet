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
docker run --rm \
  -v "$PWD:/src" \
  -w /src \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -e TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  dotnet test Bookstore.sln --configuration Release
```

The configured Coverlet include filter is `[Bookstore.Domain]*,[Bookstore.Application]*`; there is no explicit `Exclude` filter. The 85% threshold is for line coverage. The report is written to `tests/Bookstore.Tests/coverage.cobertura.xml`. The total displayed by Coverlet is the combined result for those two included assemblies, not whole-repository coverage.

## Last verified run

On 2026-10-29, the Docker-enabled Release command above passed all 48 tests (0 failed, 0 skipped). Coverlet reported:

| Included assembly | Line | Branch | Method |
|---|---:|---:|---:|
| `Bookstore.Application` | 95.65% | 87.93% | 100% |
| `Bookstore.Domain` | 98.19% | 90.62% | 100% |
| Combined included assemblies | 96.9% | 88.88% | 100% |

Both included assemblies exceeded the configured 85% line threshold.

The evaluator separately reported 75.5% total line coverage and 98.3% for Domain. The evaluator's full denominator, assembly inclusion/exclusion rules, and instrumentation configuration were not available in the repository or provided evidence, so that result could not be reproduced or compared as an equivalent measurement. The local 96.9% is explicitly limited to Domain and Application; it is not presented as whole-repository coverage or as a replacement for the evaluator's 75.5%.

## Validation scenarios

- Domain: Author name normalization, required/max-length name, website/email validation, and deactivation state.
- Application: category mapping, ID validation, Author resolution, legacy name compatibility, timestamp handling, and not-found cases.
- API + MongoDB: Book create/list/get/update, Author create/list/get/soft-delete, unique index behavior, Problem Details, reference filtering, and idempotent backfill.
- API validation and observability: invalid request payloads return Problem Details; JSON logs contain request trace enrichment.
- Backfill QA: blank legacy names are skipped; malformed author IDs are repaired without changing Book identity, legacy author text, active state, or unrelated fields; a second run is safe.
- OpenAPI: verify `/openapi/v1.json`, required request schemas and constraints, runtime path/method parity with the checked-in contract, and Production Scalar behavior.

## Runtime QA evidence

On 2026-10-29, the API and MongoDB were started with Docker Compose on ports 8080 and 27017. Runtime checks exercised the Book and Author lifecycle, validation and error responses, category/Author filters, legacy Author-name compatibility, and soft deletion without changing linked Books. The backfill reported one linked Book and one created Author on the first pass, and no new links or Authors on the second pass; existing valid links were counted as already linked.

A separate disposable `bookstore-backfill-qa` database verified a blank author was skipped and a malformed `authorId` was repaired. The malformed-record check confirmed the ObjectId Book identity, title, legacy author text, quantity, active state, and description remained intact. Re-running the migration left the repaired link unchanged.

With MongoDB stopped, `/api/books/health` returned HTTP 500 as `application/problem+json`, included a trace identifier, and did not expose exception, connection-string, or password details. After MongoDB restarted, the health endpoint returned HTTP 200. The Production-hosted API returned 404 for Scalar while `/openapi/v1.json` remained available. Runtime paths and HTTP methods matched the checked-in contract across six paths; automated OpenAPI tests also verified key request schemas, required fields, and constraints.

Serilog runtime output was observed as JSON; 143 of 147 sampled log records included the request trace identifier. These checks are automated/runtime evidence, not manual product-owner acceptance or production operational sign-off. Production migration, live deployment, and other operational checks were not performed.
