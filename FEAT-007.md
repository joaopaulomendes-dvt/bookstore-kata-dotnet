Feature Request: Book Author Management System
Document Type: Product Requirement
Priority: High
Status: Approved for Development

Title
Add Author Management Capabilities to Support Book Authors as First-Class Entities

Description
Currently, the bookstore system stores author information as a simple string field (author) in the Book model. This approach has limitations:

Authors can only have a name
No author profile information (biography, website, contact info)
Difficult to search books by author across the catalog
No way to manage author metadata or relationships
Duplicate author names across books are not de-duplicated
This feature request proposes to create endpoints for Authors to manage authors as well. The following operations should be supported: Create, Soft-delete, List and Get.

Objectives
Create a dedicated Author model with comprehensive metadata
Establish a one-to-many relationship between Authors and Books
Provide RESTful endpoints for Author management
Enable filtering books by author with improved query capabilities
Proposed Author Schema
{
  _id: ObjectId,
  id: String (unique identifier),
  name: String (required, max 255),
  birthDate: Date (optional),
  nationality: String (optional, country of origin),
  email: String (optional, contact email),
  active: Boolean (default: true),
  createdAt: Date (auto, creation timestamp),
  updatedAt: Date (auto, last update timestamp)
}