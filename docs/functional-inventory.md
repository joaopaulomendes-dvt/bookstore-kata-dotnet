# Functional Inventory

This inventory describes behavior evidenced by the repository's application and configuration files. It does not claim validation against a running deployment.

## Book Inventory and Catalogue

- **Business purpose:** Maintain book records and quantities, browse the catalogue, and retrieve a book by title or ID. Catalogue listing can be filtered by category.
- **Trigger system:** External HTTP clients; no specific consuming system is identified in the repository.
- **Entry points:**
  - `GET /api/books?category={category}` lists books. An exact match to one of the static category names filters results; a missing, empty, unrecognized, or differently cased value returns the full list. Filtering happens in memory after all books are read from MongoDB.
  - `GET /api/books/single?id={id}&name={name}&author={author}` retrieves a book by title or ID. Title matching is exact and case-sensitive. Author filtering is not implemented and throws an exception when requested. If both title and ID are provided, title is checked first and a matching ID takes precedence.
  - `POST /api/books` creates a book. The request DTO carries title, author, category name, quantity, description, language, active status, inactive date, publisher, and publisher date.
  - `PUT /api/books` updates quantity and active status for the supplied ID. Deactivation sets the inactive date to the current date.
  - No delete endpoint is present.
- **OpenAPI operations:** No OpenAPI document or generation dependency was found. The HTTP operations above are derived from JAX-RS annotations, not from a published OpenAPI contract.
- **Main implementation and tests:** Routes and request handling: [`BookController.java`](../src/main/java/com/bookstore/controller/BookController.java). MongoDB persistence and category filtering: [`BookService.java`](../src/main/java/com/bookstore/service/BookService.java). Data model and request DTOs: [`Book.java`](../src/main/java/com/bookstore/model/Book.java), [`CreateBookDTO.java`](../src/main/java/com/bookstore/controller/CreateBookDTO.java), [`UpdateBookDTO.java`](../src/main/java/com/bookstore/controller/UpdateBookDTO.java). Static category names and IDs: [`Category.java`](../src/main/java/com/bookstore/model/Category.java). The only discovered test is a successful create-controller test: [`BookControllerTest.java`](../src/test/java/com/bookstore/controller/BookControllerTest.java); no tests were found for the catalogue-list or single-book lookup GET operations.
- **Relevant configuration:** The server listens on `0.0.0.0:8080`, with the API base path `/api`, in [`BookstoreApp.java`](../src/main/java/com/bookstore/BookstoreApp.java). The service hardcodes MongoDB URI `mongodb://mongodb:27017`, database `bookstore`, and collection `books`. Docker Compose sets `MONGODB_URI`, but the service does not read it. [`01-init-bookstore.js`](../mongo-init/01-init-bookstore.js) seeds a `categories` collection; application category matching instead uses the static list in `Category.java`.
- **Observed behavior caveats:** Category filtering uses static category IDs and has no invalid-category error; listing is read-only. ID matching uses Java string reference comparison (`==`), so an ID lookup may not find a matching book. A title or ID miss, or a request with neither parameter, returns HTTP 200 with a null entity; if both parameters are given and the ID misses, a title match is retained. A null stored title can cause the title comparison to throw. Supplying `author` throws a runtime exception. The controller returns 201 for the update operation, and does not verify whether MongoDB matched an existing ID.

## API Availability

- **Business purpose:** Expose a lightweight health response for availability checks.
- **Trigger system:** An external HTTP health checker or client; no specific checker is configured in the repository.
- **Entry point:** `GET /api/books/health` returns HTTP 200.
- **OpenAPI operations:** No formal OpenAPI specification was found; the operation is inferred from JAX-RS annotations.
- **Main implementation and tests:** The route is in [`BookController.java`](../src/main/java/com/bookstore/controller/BookController.java). No health-specific test was found.
- **Relevant configuration:** Uses the same server host, port, and API base path configured in [`BookstoreApp.java`](../src/main/java/com/bookstore/BookstoreApp.java).

## UI and Background Processing

No UI assets or user-interface actions, scheduled jobs, or other background workers were found in the inspected source and configuration. The repository exposes an HTTP API only.

## Inspection Limits

Inspected the README, Maven configuration, Java application/controller/service/model/DTO/test files, Dockerfile, Docker Compose configuration, and MongoDB initialization script. The application was not run; MongoDB behavior, external callers, production deployment settings, and runtime monitoring were not verified. The test suite was not run.