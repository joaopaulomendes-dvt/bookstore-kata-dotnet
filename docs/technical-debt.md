# Technical Debt Register

This register records known limitations that remain after the .NET 10 migration and Author feature delivery. Items are documented rather than silently treated as completed work.

| ID | Area | Current limitation | Impact and follow-up |
|---|---|---|---|
| TD-001 | Authentication | The API has no authentication or authorization. | Do not expose it directly to untrusted networks. Define an approved authentication and authorization design before public deployment. |
| TD-002 | Environments | No staging environment or staging OpenAPI URL is configured. | Validate deployment configuration and publish a staging API document when an environment is provisioned. |
| TD-003 | Book compatibility | Books still persist and return the legacy `author` string, and the legacy exact-name filter remains supported. | Keep these paths through an agreed client compatibility window; plan removal only after consumers have migrated to `authorId`. |
| TD-004 | Backfill review | Names that normalize identically are intentionally linked to one Author; the backfill does not distinguish people with identical normalized names or emit a collision-specific report. | Review representative production data before running the backfill. Resolve same-name people manually if they represent different Authors. |
| TD-005 | Migration operations | Production execution of the one-shot backfill has not been documented as a completed operational run. | Back up the database, rehearse against a representative copy, and record the production run and verification when scheduled. |
| TD-006 | OpenAPI metadata | The checked-in contract describes some constraints more precisely than runtime-generated OpenAPI. Runtime path/query ObjectId schemas omit the hexadecimal pattern, and runtime Author email metadata omits the email format. | Treat the checked-in OpenAPI YAML as the detailed design-first contract. Align runtime metadata or add an automated schema-equivalence check before clients rely solely on `/openapi/v1.json`. |
| TD-007 | Manual QA | The acceptance checklist contains manual scenarios that have not been recorded as completed. | A QA reviewer should execute and check [qa-checklist.md](./qa-checklist.md) before release sign-off. |
