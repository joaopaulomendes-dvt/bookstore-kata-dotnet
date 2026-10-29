# QA Acceptance Checklist

## Book API compatibility

- [ ] `GET /api/books/health` returns 200 only when MongoDB is available.
- [ ] List, legacy single lookup, create, and update routes preserve their documented request/response behavior.
- [ ] Book IDs are returned as 24-character ObjectId strings; invalid IDs return Problem Details with 400.
- [ ] Category, Author ID, and legacy Author-name filters return the expected records.

## Author profiles and lifecycle

- [ ] Valid Author metadata is persisted and returned; invalid names, URLs, and emails return 400.
- [ ] Normalized duplicate names return 409.
- [ ] Default Author list/get omit inactive records; `includeInactive=true` returns them.
- [ ] Delete returns 204, is idempotent, and does not delete or deactivate associated Books.

## Book association and migration

- [ ] New Books accept active `authorId`; unknown/inactive Author references return 404.
- [ ] Legacy Author names remain accepted and resolve to an Author profile.
- [ ] The backfill links legacy Books, retains the legacy author string, handles blank/malformed values, and is safe to rerun.
- [ ] Backfill does not alter Book identity, active state, or unrelated catalog fields.

## Quality and operations

- [ ] `dotnet test` passes with Docker available.
- [ ] Domain/Application line coverage meets the enforced 85% threshold.
- [ ] OpenAPI JSON matches the checked-in OpenAPI contract; Scalar is not exposed in Production.
- [ ] Errors use `application/problem+json`, include a trace identifier, and do not reveal stack traces or credentials.
- [ ] API and MongoDB start through Docker Compose on port 8080 and 27017 respectively.
