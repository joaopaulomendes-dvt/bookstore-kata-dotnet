# Research: Book Author Management

## MongoDB identity and API IDs

**Decision**: Store Author and Book identifiers as BSON `ObjectId` values. Expose the hexadecimal representation as a string in request/response DTOs and route parameters. Validate route IDs with `ObjectId.TryParse` before querying.

**Rationale**: MongoDB's C# driver uses ObjectId as the conventional BSON identity. Keeping the BSON type internally avoids a redundant persisted ID while string DTO values keep MongoDB types out of the public API contract.

**Alternatives considered**: Persist an additional string `id` beside `_id`; introduce custom identity serialization. Both add complexity without a need in the current API.

Sources:
- [MongoDB C# driver: Get started](https://www.mongodb.com/docs/drivers/csharp/current/get-started/)
- [MongoDB C# driver: Serialization](https://www.mongodb.com/docs/drivers/csharp/current/serialization/)

## Backfilling legacy author names

**Decision**: Add optional `authorId` to Book documents, keep the legacy `author` field during transition, and run an idempotent backfill. Normalize names for matching only, upsert an Author for each normalized value, and link Books by generated ObjectId. Do not use the author name as a permanent relationship key. Report blank values and ambiguous collisions rather than silently deleting or merging unrelated data.

**Rationale**: A staged additive migration permits old and new document shapes to coexist and makes retry safe. Stable ObjectIds remain valid if a name changes.

**Alternatives considered**: Replace string author with an ID in one migration (risky for existing clients); store author name permanently as a foreign key (fragile across renames and normalization changes).

Source:
- [MongoDB: Building with Patterns — Schema Versioning](https://www.mongodb.com/company/blog/building-with-patterns-the-schema-versioning-pattern)

## OpenAPI and reference UI

**Decision**: Use .NET 10 built-in `Microsoft.AspNetCore.OpenApi` document generation. Expose the JSON document and configure Scalar or Swagger UI only for Development.

**Rationale**: Built-in OpenAPI supports Minimal APIs without adding a separate document-generation stack. Microsoft advises limiting interactive OpenAPI UIs to development to reduce production information disclosure.

**Alternatives considered**: Swashbuckle remains viable if a required plugin is found, but should not duplicate the built-in generator without a requirement.

Sources:
- [Microsoft Learn: OpenAPI documents in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/using-openapi-documents?view=aspnetcore-10.0)
- [Microsoft Learn: OpenAPI overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview?view=aspnetcore-10.0)

## API integration testing

**Decision**: Use `WebApplicationFactory<Program>` for the API host and the official `Testcontainers.MongoDb` module for disposable MongoDB integration tests. Inject the container connection string through test configuration. Use xUnit fixtures for lifecycle management.

**Rationale**: This exercises routing, serialization, validation, MongoDB persistence, and indexes using the actual web host and a real MongoDB server.

**Alternatives considered**: Mock repositories for all tests (insufficient coverage of BSON and index behavior); shared developer MongoDB (non-hermetic and stateful).

Sources:
- [Microsoft Learn: Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0)
- [Testcontainers for .NET: MongoDB module](https://dotnet.testcontainers.org/modules/mongodb/)

## Quality and architecture

**Decision**: Meet the stricter documented coverage rule: at least 85% line coverage for Domain/Application, which exceeds the company-wide minimum 70% business-logic coverage. Use lightweight Domain/Application/Infrastructure/API responsibilities without adding an event bus.

**Rationale**: The repository's architecture instructions require DDD/SOLID review, unit and integration tests, and 85% coverage for Domain/Application. The prompt requires at least 70% business logic.

**Sources**:
- [Company guidelines](../../COMPANY_GUIDELINES.md)
- [Repository .NET architecture instructions](../../.github/instructions/dotnet-architecture-good-practices.instructions.md)

## Limitations

The Microsoft integration-test and Testcontainers documentation describe the host-factory and MongoDB-container patterns separately; the test setup composes those documented patterns. The backfill should be validated against a copy of representative legacy data before production use.
