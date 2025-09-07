# FunnyActivities API

A comprehensive .NET 8 Web API for managing materials, products, and categories in the FunnyActivities system. Built with CQRS pattern, MediatR, and role-based authorization.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [API Documentation](#api-documentation)
  - [Authentication](#authentication)
  - [Category Management](#category-management)
  - [Materials Management](#materials-management)
  - [Products Management](#products-management)
- [Configuration](#configuration)
- [Error Handling](#error-handling)
- [Development](#development)

## Overview

FunnyActivities is a modern .NET 8 Web API that provides comprehensive management capabilities for materials, products, and categories. The system implements CQRS (Command Query Responsibility Segregation) pattern with MediatR for clean architecture and maintainable code.

### Key Features

- **CQRS Architecture**: Separate command and query handlers for optimal performance
- **Role-Based Authorization**: Granular permissions with JWT authentication
- **Comprehensive API Documentation**: Swagger/OpenAPI integration
- **Domain-Driven Design**: Clean separation of concerns with domain events
- **Centralized Authentication**: Middleware-based user context management
- **File Upload Support**: Multi-part form data handling for images and documents

## Architecture

### CQRS Pattern Implementation

The system follows CQRS principles with clear separation between commands (write operations) and queries (read operations):

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Controllers   │───▶│    MediatR      │───▶│  Command/Query  │
│                 │    │   Mediator      │    │   Handlers      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                        │
                                                        ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Domain        │───▶│   Repository    │───▶│   Database      │
│   Entities      │    │   Interface     │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

#### Commands (Write Operations)
- `CreateCategoryCommand` - Creates new categories
- `UpdateCategoryCommand` - Updates existing categories
- `DeleteCategoryCommand` - Deletes categories

#### Queries (Read Operations)
- `GetCategoriesQuery` - Retrieves paginated category list
- `GetCategoryQuery` - Retrieves single category
- `GetCategoryWithProductsQuery` - Retrieves category with associated products
- `SearchCategoriesQuery` - Searches categories by name

#### Domain Events
- `CategoryCreatedEvent` - Fired when category is created
- `CategoryUpdatedEvent` - Fired when category is updated
- `CategoryDeletedEvent` - Fired when category is deleted

### Project Structure

```
FunnyActivities/
├── FunnyActivities.WebAPI/           # API Controllers and Startup
├── FunnyActivities.Application/       # Application Layer (CQRS)
│   ├── Commands/                      # Write Operations
│   ├── Queries/                       # Read Operations
│   ├── Handlers/                      # Command/Query Handlers
│   ├── DTOs/                          # Data Transfer Objects
│   └── Validators/                    # Request Validation
├── FunnyActivities.Domain/            # Domain Layer
│   ├── Entities/                      # Domain Entities
│   ├── Events/                        # Domain Events
│   └── Exceptions/                    # Domain Exceptions
├── FunnyActivities.Infrastructure/    # Infrastructure Layer
└── FunnyActivities.CrossCuttingConcerns/ # Shared Concerns
    ├── Authentication/                # Auth Middleware
    ├── Authorization/                 # Policies
    ├── APIDocumentation/              # Swagger Config
    └── Logging/                       # Logging Configuration
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server or compatible database
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd FunnyActivities
```

2. Restore packages:
```bash
dotnet restore
```

3. Update database connection string in `appsettings.json`

4. Run database migrations:
```bash
dotnet ef database update
```

5. Run the application:
```bash
dotnet run --project FunnyActivities.WebAPI
```

The API will be available at `https://localhost:5001` with Swagger UI at `https://localhost:5001/swagger`.

## API Documentation

### Authentication

All API endpoints require JWT authentication. Include the token in the Authorization header:

```
Authorization: Bearer {your-jwt-token}
```

#### User Roles
- **Admin**: Full access to all operations
- **Viewer**: Read-only access to categories and materials

### Category Management

The Category Management module provides comprehensive CRUD operations for product categories with advanced filtering and search capabilities.

#### Authorization Policies

| Policy | Description | Required Role |
|--------|-------------|---------------|
| `CanViewCategory` | View categories | Admin, Viewer |
| `CanCreateCategory` | Create new categories | Admin |
| `CanUpdateCategory` | Update existing categories | Admin |
| `CanDeleteCategory` | Delete categories | Admin |

#### Endpoints

##### 1. Get Categories (Paginated)

**GET** `/api/categories`

Retrieves a paginated list of categories with optional filtering and sorting.

**Query Parameters:**
- `pageNumber` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 10, max: 100)
- `searchTerm` (string, optional): Filter by category name
- `sortBy` (string, optional): Sort field (name, createdAt, updatedAt)
- `sortOrder` (string, optional): Sort order (asc, desc)

**Example Request:**
```http
GET /api/categories?pageNumber=1&pageSize=10&searchTerm=electronics&sortBy=name&sortOrder=asc
Authorization: Bearer {token}
```

**Success Response (200):**
```json
{
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "name": "Electronics",
        "description": "Electronic devices and accessories",
        "createdAt": "2024-01-15T10:30:00Z",
        "updatedAt": "2024-01-15T10:30:00Z",
        "productCount": 25
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "message": "Categories retrieved successfully",
  "statusCode": 200
}
```

##### 2. Get Category by ID

**GET** `/api/categories/{id}`

Retrieves a specific category by its unique identifier.

**Path Parameters:**
- `id` (Guid): Category unique identifier

**Example Request:**
```http
GET /api/categories/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer {token}
```

**Success Response (200):**
```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Electronics",
    "description": "Electronic devices and accessories",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "productCount": 25
  },
  "message": "Category retrieved successfully",
  "statusCode": 200
}
```

##### 3. Get Category with Products

**GET** `/api/categories/{id}/with-products`

Retrieves a category along with all its associated products.

**Path Parameters:**
- `id` (Guid): Category unique identifier

**Example Request:**
```http
GET /api/categories/550e8400-e29b-41d4-a716-446655440000/with-products
Authorization: Bearer {token}
```

**Success Response (200):**
```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Electronics",
    "description": "Electronic devices and accessories",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "products": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440001",
        "name": "iPhone 15",
        "description": "Latest iPhone model",
        "price": 999.99,
        "categoryId": "550e8400-e29b-41d4-a716-446655440000"
      }
    ],
    "totalProducts": 1
  },
  "message": "Category with products retrieved successfully",
  "statusCode": 200
}
```

##### 4. Search Categories

**GET** `/api/categories/search`

Searches for categories based on the provided search term.

**Query Parameters:**
- `searchTerm` (string, required): Search term for category names
- `pageNumber` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 10, max: 100)

**Example Request:**
```http
GET /api/categories/search?searchTerm=phone&pageNumber=1&pageSize=5
Authorization: Bearer {token}
```

**Success Response (200):**
```json
{
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "name": "Mobile Phones",
        "description": "Smartphones and accessories",
        "createdAt": "2024-01-15T10:30:00Z",
        "updatedAt": "2024-01-15T10:30:00Z",
        "productCount": 15
      }
    ],
    "pageNumber": 1,
    "pageSize": 5,
    "totalCount": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "message": "Categories search completed successfully",
  "statusCode": 200
}
```

##### 5. Create Category

**POST** `/api/categories`

Creates a new category.

**Request Body:**
```json
{
  "name": "Home Appliances",
  "description": "Kitchen and household appliances"
}
```

**Example Request:**
```http
POST /api/categories
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Home Appliances",
  "description": "Kitchen and household appliances"
}
```

**Success Response (201):**
```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "name": "Home Appliances",
    "description": "Kitchen and household appliances",
    "createdAt": "2024-01-15T11:00:00Z",
    "updatedAt": "2024-01-15T11:00:00Z",
    "productCount": 0
  },
  "message": "Category created successfully",
  "statusCode": 201
}
```

##### 6. Update Category

**PUT** `/api/categories/{id}`

Updates an existing category.

**Path Parameters:**
- `id` (Guid): Category unique identifier

**Request Body:**
```json
{
  "name": "Home & Kitchen Appliances",
  "description": "Updated description for home and kitchen appliances"
}
```

**Example Request:**
```http
PUT /api/categories/550e8400-e29b-41d4-a716-446655440002
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Home & Kitchen Appliances",
  "description": "Updated description for home and kitchen appliances"
}
```

**Success Response (200):**
```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "name": "Home & Kitchen Appliances",
    "description": "Updated description for home and kitchen appliances",
    "createdAt": "2024-01-15T11:00:00Z",
    "updatedAt": "2024-01-15T11:15:00Z",
    "productCount": 0
  },
  "message": "Category updated successfully",
  "statusCode": 200
}
```

##### 7. Delete Category

**DELETE** `/api/categories/{id}`

Deletes a category.

**Path Parameters:**
- `id` (Guid): Category unique identifier

**Example Request:**
```http
DELETE /api/categories/550e8400-e29b-41d4-a716-446655440002
Authorization: Bearer {token}
```

**Success Response (204):**
```json
{
  "data": null,
  "message": "Category deleted successfully",
  "statusCode": 204
}
```

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=FunnyActivities;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "FunnyActivities",
    "Audience": "FunnyActivitiesUsers",
    "ExpiryInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Authorization Policies Configuration

Policies are configured in `Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewCategory", policy =>
        policy.RequireRole("Admin", "Viewer"));

    options.AddPolicy("CanCreateCategory", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("CanUpdateCategory", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("CanDeleteCategory", policy =>
        policy.RequireRole("Admin"));
});
```

## Error Handling

### Standard Error Response Format

All API errors follow a consistent format:

```json
{
  "error": "ErrorType",
  "message": "Human-readable error message",
  "statusCode": 400
}
```

### Category Management Error Codes

| Status Code | Error Type | Description | Example Scenario |
|-------------|------------|-------------|------------------|
| 400 | ValidationError | Invalid request data | Missing required fields, invalid format |
| 400 | CategoryNameAlreadyExists | Category name already exists | Creating category with duplicate name |
| 401 | Unauthorized | Authentication required | Missing or invalid JWT token |
| 403 | Forbidden | Insufficient permissions | User lacks required role |
| 404 | NotFound | Category not found | Invalid category ID |
| 500 | InternalError | Server error | Database connection issues |

### Specific Error Examples

#### Validation Error (400)
```json
{
  "error": "ValidationError",
  "message": "Category name is required.",
  "statusCode": 400
}
```

#### Category Not Found (404)
```json
{
  "error": "NotFound",
  "message": "Category with ID '550e8400-e29b-41d4-a716-446655440000' was not found.",
  "statusCode": 404
}
```

#### Duplicate Category Name (400)
```json
{
  "error": "ValidationError",
  "message": "A category with the name 'Electronics' already exists.",
  "statusCode": 400
}
```

#### Unauthorized (401)
```json
{
  "error": "Unauthorized",
  "message": "Invalid user identity",
  "statusCode": 401
}
```

#### Forbidden (403)
```json
{
  "error": "Forbidden",
  "message": "User does not have required permissions",
  "statusCode": 403
}
```

## Development

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test FunnyActivities.Application.UnitTests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Code Generation

The project uses MediatR for CQRS implementation. Commands and queries are automatically registered through dependency injection.

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Generate SQL script
dotnet ef migrations script
```

### API Documentation Access

Once the application is running, access Swagger UI at:
- Development: `https://localhost:5001/swagger`
- Production: `https://yourdomain.com/swagger`

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.