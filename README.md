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

## Testing

Run unit and MongoDB-backed API integration tests:

```sh
dotnet test
```

Docker must be available for the integration-test MongoDB Testcontainer. Coverage is collected for Domain and Application code and the 85% line threshold is enforced by the test project:

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

See [docs/functional.md](./docs/functional.md), [docs/technical.md](./docs/technical.md), and [specs/007-author-management/plan.md](./specs/007-author-management/plan.md).

## Maintainers

Maintainer and support-channel ownership has not yet been assigned. Contact the repository owner until a team contact is designated.

## Harness Architecture

### Tools

| Tool | Why | Install (reference) |
|------|-----|---------------------|
| **GitHub MCP** | Fetch issues (included in `.mcp.json`) | Uses Copilot GitHub auth — `https://api.githubcopilot.com/mcp/` with `X-MCP-Toolsets: issues` |
| **CodeGraph** | Code navigation (base) | `npm i -g @colbymchenry/codegraph` → `codegraph install` → in repo `codegraph init` |
| **RTK** | Compress shell/test noise | `brew install rtk-ai/tap/rtk` (or Cargo from `rtk-ai/rtk`) → `rtk gain` to verify → `rtk init -g --copilot` |
| **Caveman** | Chat prose lite only | `npx skills add JuliusBrussee/caveman -g` → use `/caveman lite` in chat |
| **Graphify** (optional) | Project/docs/schema map | Install per [Graphify](https://github.com/safishamsi/graphify), then add the MCP block below |
| **Spec Kit** (optional) | Spec-driven delivery | [github/spec-kit](https://github.com/github/spec-kit) `specify init` for Copilot |
| **grill-with-docs** (optional) | ADRs / CONTEXT alignment | From [mattpocock/skills](https://github.com/mattpocock/skills) |

### Optional Graphify MCP

Add only if Graphify is installed. Portable `.mcp.json`:

```json
"graphify": {
  "type": "stdio",
  "command": "graphify",
  "args": [".", "--mcp"]
}
```

VS Code `.vscode/mcp.json` uses the same server entry under `"servers"`.

### Harness configuration

- **GitHub MCP** loads issues before work.
- **CodeGraph** supports code navigation.
- **Graphify** maps project documentation and schemas when installed.
- **RTK** reduces noisy shell output.
- **Caveman lite** is for chat prose only, never committed specs or PR text.
- **Spec Kit** is the sole delivery framework when used.

No phase profiles. No parallel Superpowers + Spec Kit.
