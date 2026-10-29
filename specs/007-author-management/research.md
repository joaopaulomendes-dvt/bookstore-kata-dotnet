# Research: Book Author Management

## MongoDB identity and API IDs

**Decision**: Store Author and Book identifiers as BSON `ObjectId` values. Expose the hexadecimal representation as a string in request/response DTOs and route parameters. Validate route IDs with `ObjectId.TryParse` before querying.

**Rationale**: MongoDB's C# driver uses ObjectId as the conventional BSON identity. Keeping the BSON type internally avoids a redundant persisted ID while string DTO values keep MongoDB types out of the public API contract.

**Alternatives considered**: Persist an additional string `id` beside `_id`; introduce custom identity serialization. Both add complexity without a need in the current API.

Sources:
- [MongoDB C# driver: Get started](https://www.mongodb.com/docs/drivers/csharp/current/get-started/)
- [MongoDB C# driver: Serialization](https://www.mongodb.com/docs/drivers/csharp/current/serialization/)

## Backfilling legacy author names

**Decision**: Add optional `authorId` to Book documents, keep the legacy `author` field during transition, and run an idempotent backfill. Normalize names for matching only, upsert an Author for each normalized value, and link Books by generated ObjectId. Do not use the author name as a permanent relationship key. Count skipped records in the migration report. Names that normalize identically are intentionally matched to one Author; the migration cannot determine whether same-name people are distinct, so review source data if that risk applies. The migration does not delete Author profiles.

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

## Evaluator follow-up

### Recovering legacy behavior

**Decision**: Use the Java controller and service at `8ec79aa^` as evidence for confirmed
legacy behavior. The controller confirms health always returned 200 without a database
check; list accepted a category filter; single lookup accepted ID, title, or author;
create returned 201 when title was non-null; and update returned 201 when ID was supplied.
The Java service confirms that category matching was case-sensitive, an unknown category
fell back to listing all books, author filtering threw an unimplemented exception, and
missing quantity/category/active values were projected as 0/0/false. Update changed
quantity and active state and set `inactiveDate` to the current date when deactivating.
The legacy ID lookup used Java reference equality (`==`) while title matching used value
equality (`equals`).

**Rationale**: These claims are directly visible in committed source. The effective
deployed URL prefix beyond Jersey's `/books` class path, client dependence on the broken
ID comparison, null entity serialization behavior, and whether every edge case was covered
in production remain unconfirmed; they must not be presented as known facts.

**Alternatives considered**: Treating the current .NET behavior as a proxy for legacy
behavior (risks retroactively attributing new behavior to the old API); inferring behavior
from the challenge prompt alone (insufficient evidence for detailed rules).

### Coverage comparison

**Decision**: Re-run Coverlet with its configured Domain/Application include filter and
record the exact command, generated report, assembly set, and results. Separately measure
the broader application assemblies only if needed to explain the denominator. Treat the
evaluator's 75.5% as unreproduced until its instrumentation and exclusions are known.

**Rationale**: The checked-in test project configures `coverlet.msbuild` with an 85% line
threshold and `Include` restricted to `[Bookstore.Domain]*,[Bookstore.Application]*`.
That scope is not equivalent to a harness-injected whole-repository result. Reproducible
scope labels prevent misleading comparisons.

**Alternatives considered**: Comparing percentages without denominator details (invalid);
weakening or broadening the existing threshold solely to match an external result
(unjustified and potentially conceals untested business code).

### Logging and validation dependencies

**Decision**: Adopt Serilog and FluentValidation. `COMPANY_GUIDELINES.md` lists both under
standard dependencies, and the active constitution requires them for new work unless an
approved exception is documented. No exception is recorded. Keep package versions in
`Directory.Packages.props`; use FluentValidation at the HTTP request boundary while retaining
domain invariants in the Domain layer.

**Rationale**: `COMPANY_GUIDELINES.md` labels these as standard dependencies; the current
constitution sets them as requirements for new work unless an approved exception exists.
The current service does not reference either package, so compliance should be resolved
deliberately rather than assumed. Existing RFC 7807 responses, JSON output, trace IDs, and
domain validation remain compatibility requirements.

**Alternatives considered**: Treating equivalent existing functionality as automatic
compliance (does not satisfy the constitution without an approved exception); adding
packages without comparing behavior or central version management (needless risk).

### QA and delivery evidence

**Decision**: Distinguish automated test evidence from manually executed acceptance checks.
Only check off scenarios actually executed; record unexecuted scenarios and environmental
blockers. Add a changelog entry only for delivered follow-up changes. Do not invent a
historical feature branch or PR.

**Rationale**: The current QA checklist is unchecked, while prior automated runs do not
prove every operational or manual scenario. Accurate delivery metadata is required for
review and evaluation.

**Alternatives considered**: Marking every checklist line complete based on test-suite
success (overstates evidence); creating a retroactive branch/PR record (misrepresents
history).

### Constitution ratification metadata

**Decision**: Obtain the original ratification date from the repository owner. Preserve
the existing TODO and temporary Sync Impact Report until the date and amendment status are
confirmed; remove the report only under the constitution update workflow before commit.

**Rationale**: A file timestamp or first commit cannot prove when governance was ratified.

**Alternatives considered**: Guessing a date from Git history (not reliable); silently
removing the TODO or report (would conceal unresolved governance state).
