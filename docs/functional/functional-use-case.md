# Use Case: Create a Book

## Document Information

- **Business capability:** Book inventory and catalogue.
- **Evidence basis:** [`BookController.java`](../../src/main/java/com/bookstore/controller/BookController.java), [`CreateBookDTO.java`](../../src/main/java/com/bookstore/controller/CreateBookDTO.java), [`BookService.java`](../../src/main/java/com/bookstore/service/BookService.java), [`Category.java`](../../src/main/java/com/bookstore/model/Category.java), and [`BookControllerTest.java`](../../src/test/java/com/bookstore/controller/BookControllerTest.java).
- **Why selected:** This is the only business operation with a controller test, and the implementation includes the full create-to-database insert path.
- **Business owner, intended users, and approval status:** Need confirmation.
- **Evidence boundary:** The controller test uses a mocked service. It verifies request delegation and the successful response, but does not verify MongoDB persistence or production behavior.

## Business Purpose

Add a new book to the bookstore catalogue so it can be maintained and retrieved as part of inventory. Whether the record represents a physical copy, a title-level catalogue entry, or another inventory unit is Need confirmation.

## Actions and Triggers

- An external client submits a create-book request to `POST /api/books` with a JSON body. The API is served under `/api` on port `8080` by the application configuration.
- The system checks the title, prepares a new book record, and asks the bookstore database to save it.
- The specific user, upstream system, and business event that initiates the request are Need confirmation. No UI action was found in the inspected project.
- The application is configured to use the `bookstore` database and `books` collection on MongoDB at `mongodb://mongodb:27017`. The service uses this hardcoded address; the `MONGODB_URI` value in Docker Compose is not read by the service.

## Inputs

The JSON request maps to the fields below. Only the title has an explicit controller check; requiredness beyond that check is Need confirmation.

| Input | Implemented behavior |
|---|---|
| `title` | Must not be `null` for the create path to run. An empty string is not rejected by the code. |
| `author` | Passed through and stored; mandatory status is Need confirmation. |
| `categoryId` | Despite the field name, the service expects a category name such as `Programming`, not a numeric ID. It is optional in the code. |
| `quantity` | Passed through as an integer; if omitted, Java's primitive default is `0`. Negative values are not explicitly rejected. |
| `description`, `language`, `publisher` | Passed through and stored; mandatory status is Need confirmation. |
| `active` | Defaults to `true` when omitted by the request object. |
| `inactiveDate`, `publisherDate` | Passed through and stored when supplied. Accepted date format and business meaning are Need confirmation. |

## Main Flow

1. The client submits the JSON request to `POST /api/books`.
2. If `title` is `null`, the controller returns HTTP `404` without calling the service. Otherwise, it passes the request fields to the create operation.
3. The service creates a new identifier and prepares a book record. A supplied category name is mapped to the corresponding static category ID.
4. The service inserts the record into the MongoDB `bookstore.books` collection.
5. After a successful insert, the controller returns HTTP `201 Created` with the created book representation.

If a non-null category value does not exactly match a configured category name, category lookup returns no result and the service dereferences it. This produces an unhandled runtime exception; the resulting HTTP error response is Need confirmation because no application-specific mapping was found.

## Business Rules

- The title check rejects only `null`; blank titles are accepted by the implemented condition. Whether blank titles should be allowed is Need confirmation.
- Recognized category names are `Fiction`, `Non-Fiction`, `Science`, `Biography`, `Children`, and `Programming`. Matching is exact and case-sensitive. If category is omitted, the record retains category ID `0`; handling of unknown category names is an unhandled exception, not a validation response.
- The system generates the new identifier. The implementation does not check for duplicate titles or enforce a non-negative quantity.
- The active flag defaults to `true`; other required-field, date, and inventory rules are Need confirmation.
- The tested successful example uses category `Programming`, which maps to category ID `6`.

## State Changes

- On successful creation, one document is inserted into the `books` collection with the generated identifier, title, author, category ID, quantity, description, language, active flag, inactive date, publisher, and publisher date.
- The category collection is not updated. Category IDs used by the application come from its static category list.
- The controller does not call the create service when title is `null`. Duplicate handling, rollback, and recovery when MongoDB is unavailable are Need confirmation; no explicit behavior for those cases is implemented here.

## Examples

**Successful request**

```json
{
	"title": "Test Driven Development",
	"author": "Kent Beck",
	"categoryId": "Programming",
	"quantity": 3,
	"description": "A book about TDD",
	"language": "English",
	"active": true,
	"publisher": "Addison-Wesley"
}
```

**Observed outcome:** With a successful database insert, the API returns HTTP `201 Created` and the created book, including a generated identifier. The controller test verifies the `201` response, returned entity, and service arguments using a mock; it does not verify database insertion.

**Title omitted or null:** The controller returns HTTP `404` and does not invoke the create service. Whether this status is intended is Need confirmation.
