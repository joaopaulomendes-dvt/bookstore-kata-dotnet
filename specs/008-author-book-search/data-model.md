# Data Model: Search Books by Author Name

This feature does not add or modify persisted entities.

## Book

Existing attributes used by search:

- `author`: Author display name stored on the Book; present for linked Books and may be
  the only Author representation on legacy records.
- `authorId`: Optional stable reference to the Author profile.
- `categoryId`: Existing catalog category filter.

## Search criteria

The list operation accepts an optional `authorName` text fragment. Surrounding
whitespace is ignored; blank input has no filter. Terms up to 255 characters are
matched literally and case-insensitively anywhere in `Book.author`.

Search criteria compose using AND semantics. Author profile status does not suppress
Books, and no search result changes persisted state.
