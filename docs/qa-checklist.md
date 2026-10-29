# QA Acceptance Checklist

## Book API compatibility

- [x] `GET /api/books/health` returns 200 only when MongoDB is available.
- [x] List, legacy single lookup, create, and update routes preserve their documented request/response behavior.
- [x] Book IDs are returned as 24-character ObjectId strings; invalid IDs return Problem Details with 400.
- [x] Category, Author ID, and legacy Author-name filters return the expected records.

## Author profiles and lifecycle

- [x] Valid Author metadata is persisted and returned; invalid names, URLs, and emails return 400.
- [x] Normalized duplicate names return 409.
- [x] Default Author list/get omit inactive records; `includeInactive=true` returns them.
- [x] Delete returns 204, is idempotent, and does not delete or deactivate associated Books.

## Book association and migration

- [x] New Books accept active `authorId`; unknown/inactive Author references return 404.
- [x] Legacy Author names remain accepted and resolve to an Author profile.
- [x] The backfill links legacy Books, retains the legacy author string, handles blank/malformed values, and is safe to rerun.
- [x] Backfill does not alter Book identity, active state, or unrelated catalog fields.

## Quality and operations

- [x] `dotnet test` passes with Docker available.
- [x] Domain/Application line coverage meets the enforced 85% threshold.
- [x] OpenAPI JSON exposes the documented operations and key request schemas; runtime paths/methods match the checked-in contract, and Scalar is not exposed in Production.
- [x] Errors use `application/problem+json`, include a trace identifier, and do not reveal stack traces or credentials.
- [x] API and MongoDB start through Docker Compose on port 8080 and 27017 respectively.

## Execution evidence

Executed on 2026-10-29. Automated checks used the Docker-enabled Release test command and passed 48/48 tests. Runtime QA used the Compose API/MongoDB stack plus a separate disposable MongoDB database for blank-name and malformed-reference backfill checks. The local Coverlet result was 96.9% line coverage across the configured Domain and Application assemblies; see [testing and coverage](./testing.md) for the exact command, filters, and the non-equivalence limitation for the evaluator's 75.5% report.

The health endpoint was checked with MongoDB running (HTTP 200), with MongoDB stopped (HTTP 500 Problem Details with trace ID and no internal details), and after MongoDB restart (HTTP 200). Runtime OpenAPI path/method parity was confirmed across six paths, and automated tests checked key schemas/constraints. Scalar returned 404 from the Production-hosted API while OpenAPI JSON remained available. The backfill fixture verified skipped blank values, repair of a malformed `authorId`, preservation of Book fields, and idempotent rerun.

These are automated and runtime observations; they do not represent manual product-owner acceptance, a production backfill, or a deployment. No further unexecuted scenarios remain in this checklist.
