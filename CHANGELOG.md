# Changelog

Notable user-facing changes are recorded here. Entries describe changes actually delivered;
they do not imply a historical branch, pull request, deployment, or QA sign-off.

## Unreleased

### Changed

- Added Serilog JSON logging with request trace enrichment.
- Added FluentValidation at the HTTP request boundary while retaining domain validation
  and RFC 7807 error responses.
- Added case-insensitive partial Author-name search to the Book catalog while preserving
  stable Author ID filtering and legacy exact-name filtering.
- Expanded functional documentation with source-confirmed legacy behavior and business
  context.
- Added Mermaid diagrams for runtime architecture, Author resolution, Author soft-delete,
  and the Author backfill.

### Verification

- Docker-enabled .NET 10 Release suite: 59/59 tests passed; configured Domain/Application
  coverage measured 96.95% line coverage combined (Domain 98.19%, Application 95.79%),
  above the 85% threshold.
- Runtime QA verified Book/Author flows, unavailable-database health/error behavior,
  backfill handling and rerun safety, key OpenAPI contract behavior, and Production-only
  Scalar exclusion. Detailed scope and limitations are recorded in `docs/testing.md`.
- The evaluator's separate 75.5% whole-repository coverage result remains unreproduced
  because its denominator and instrumentation configuration were not available.
