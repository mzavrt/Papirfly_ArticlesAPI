# Papirfly Articles API

A RESTful API for managing articles built with .NET 10 and ASP.NET Core. This project demonstrates modern .NET development practices including dependency injection, repository patterns, validation, and concurrent operations.

## Features

- **Article Management**: Create, retrieve, and search articles via RESTful endpoints
- **In-Memory Storage**: Lightweight in-memory repository for article data (thread-safe)
- **Advanced Search**: Search articles by name (partial match, case-insensitive) or category (exact match)
- **Concurrent Operations**: Bulk insert multiple articles concurrently with atomic validation
- **Data Validation**: Comprehensive validation for article requests with detailed error messages
- **Currency Support**: Built-in ISO 4217 currency code validation
- **ETag Support**: Optimistic concurrency control using ETags for articles
- **OpenAPI/Swagger**: Auto-generated API documentation with Swagger UI
- **Strict JSON Handling**: Numbers must be numbers, not strings—helps catch client errors early

## Technology Stack

- **.NET 10** - Latest .NET runtime
- **ASP.NET Core** - Web framework
- **Swashbuckle/Swagger** - API documentation
- **xUnit** - Unit testing framework
- **NullableReference Types** - Enabled for better null safety
- **Implicit Usings** - Cleaner code with automatic using statements

## Project Structure

```
Papirfly_ArticlesAPI/
├── Controllers/
│   └── ArticlesController.cs          # REST API endpoints
├── Services/
│   ├── ArticleService.cs              # Business logic
│   ├── ArticleValidator.cs            # Validation rules
│   └── Iso4217.cs                     # Currency validation
├── Repositories/
│   ├── IArticleRepository.cs          # Repository interface
│   └── InMemoryArticleRepository.cs   # In-memory implementation
├── Models/
│   ├── Article.cs                     # Core domain model
│   ├── ArticleRequest.cs              # Request DTO
│   └── ArticleResponse.cs             # Response DTO
├── Program.cs                         # Application startup & DI configuration
├── appsettings.json                   # Configuration
└── Papirfly_ArticlesAPI.http          # HTTP request examples

Papirfly_ArticlesAPI.Tests/
└── ArticleServiceTests.cs             # Unit tests
```

## API Endpoints

### Create Article
```
POST /api/articles
Content-Type: application/json

{
  "name": "Product Name",
  "description": "Product Description",
  "category": "Electronics",
  "price": 99.99,
  "currency": "USD"
}

Response: 201 OK
{
  "articleId": 1,
  "name": "Product Name",
  "description": "Product Description",
  "category": "Electronics",
  "price": 99.99,
  "currency": "USD",
  "version": 1
}
```

### Get Article by ID
```
GET /api/articles/{id}

Response: 200 OK (or 404 Not Found)
```

### Search Articles
```
GET /api/articles?name=searchTerm&category=Electronics

Query Parameters:
- name: Partial string match (case-insensitive)
- category: Exact category match

Response: 200 OK with array of matching articles
```

### Create Multiple Articles (Concurrent)
```
POST /api/articles-concurrent
Content-Type: application/json

[
  { "name": "Article 1", ... },
  { "name": "Article 2", ... }
]

Response: 200 OK or 400 (if any article is invalid, none are inserted)
```

## Data Model

### Article
- **ArticleId** (int): Unique identifier
- **Name** (string): Article name (required)
- **Description** (string): Article description
- **Category** (string, nullable): Article category
- **Price** (decimal): Article price (must be a number)
- **Currency** (string, nullable): ISO 4217 currency code (e.g., USD, EUR)
- **Version** (int): Optimistic concurrency token

## Validation Rules

The `ArticleValidator` enforces:
- Name and Description are required and not empty
- Name length must be between 1-100 characters
- Description length must be between 1-500 characters
- Price must be positive (> 0)
- Currency must be a valid ISO 4217 code (if provided)

## Getting Started

### Prerequisites
- .NET 10 SDK or later
- Visual Studio 2026 (or any .NET 10 compatible IDE)

### Running the Application

1. Clone the repository:
```bash
git clone https://github.com/mzavrt/Papirfly_ArticlesAPI.git
cd Papirfly_ArticlesAPI
```

2. Build the project:
```bash
dotnet build
```

3. Run the application:
```bash
dotnet run
```

4. Open the API in your browser:
```
https://localhost:5001/swagger/index.html
```

The application will start on `https://localhost:5001` with Swagger UI at `/swagger`.

### Running Tests

```bash
dotnet test
```

## Dependency Injection

The application uses ASP.NET Core's built-in DI container:

- **IArticleRepository** (Singleton) → `InMemoryArticleRepository`
  - Provides thread-safe in-memory storage
  - One instance shared across all requests

- **ArticleService** (Scoped)
  - Contains business logic for article operations
  - New instance per HTTP request (not thread-safe)

## Design Patterns

- **Repository Pattern**: IArticleRepository abstracts data access
- **Validator Pattern**: Separate validation logic in ArticleValidator
- **DTO Pattern**: Separate request/response models (ArticleRequest, ArticleResponse)
- **Singleton vs Scoped**: Strategic use of DI lifetimes for thread safety

## Development

### Adding New Features

1. Add model to `Models/`
2. Add validation to `Services/ArticleValidator.cs` or create new validator
3. Add business logic to `Services/ArticleService.cs`
4. Add API endpoints to `Controllers/ArticlesController.cs`
5. Add tests to `Papirfly_ArticlesAPI.Tests/`

### Configuration

- **appsettings.json**: Production configuration
- **appsettings.Development.json**: Development-specific settings

## License

This project is available on GitHub: [mzavrt/Papirfly_ArticlesAPI](https://github.com/mzavrt/Papirfly_ArticlesAPI)

## Contributing

Feel free to fork and submit pull requests for any improvements.
