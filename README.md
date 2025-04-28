# webdev_week5

# Data Persistence Options for .NET Web Applications

## Current Project Context
Our BookAPI project is currently using an in-memory collection (`List<Book>`) for data storage. This is fine for development but lacks persistence between application restarts. Let's explore how to implement proper data persistence options, focusing on Entity Framework integration with your existing code.

## Object Relational Mappers (ORM)
# Entity Framework Overview and ORMs Across Languages

## What is Entity Framework?

Entity Framework (EF) is Microsoft's object-relational mapping (ORM) framework for .NET applications. It enables developers to work with databases using .NET objects, eliminating most of the data-access code they would typically need to write.

### Key Features of Entity Framework

1. **Object-Relational Mapping**: Maps database tables to .NET classes and database records to objects
2. **LINQ Support**: Allows writing queries using C# instead of SQL
3. **Change Tracking**: Keeps track of changes made to entities during their lifetime
4. **Database Schema Management**: Supports code-first and database-first approaches with migrations
5. **Multiple Database Providers**: Works with SQL Server, SQLite, PostgreSQL, MySQL, etc.
6. **Navigation Properties**: Allows moving between related entities using object references
7. **Lazy/Eager Loading**: Provides flexible data loading strategies for related entities
8. **Transaction Support**: Ensures data consistency across operations

## Popular ORMs in Different Languages

### Java
1. **Hibernate**
   - Most popular Java ORM
   - Robust query capabilities with HQL (Hibernate Query Language)
   - Extensive caching mechanisms
   - Session management for unit-of-work pattern

2. **JPA (Java Persistence API)**
   - Standard specification for ORM in Java
   - Implemented by various providers (Hibernate, EclipseLink)
   - Annotation-based configuration

3. **MyBatis**
   - SQL-focused approach with XML mappings
   - More control over SQL with less automatic behavior
   - Good for legacy applications

### Python
1. **SQLAlchemy**
   - Comprehensive ORM with both high-level and low-level APIs
   - Supports many database engines
   - Flexible transaction management

2. **Django ORM**
   - Built into Django web framework
   - Simple, intuitive API
   - Auto-generates admin interfaces

3. **Peewee**
   - Lightweight, expressive ORM
   - Simple syntax, small codebase
   - Good for smaller projects

### JavaScript/TypeScript
1. **Sequelize**
   - Promise-based Node.js ORM
   - Supports PostgreSQL, MySQL, SQLite and others
   - Migrations, associations, and validation

2. **TypeORM**
   - ORM for TypeScript and JavaScript
   - Supports Active Record and Data Mapper patterns
   - Strongly typed with decorators

3. **Mongoose**
   - MongoDB-specific ODM (Object Document Mapper)
   - Schema-based solution
   - Validation, query building, hooks

### PHP
1. **Doctrine**
   - Inspired by Hibernate
   - Data mapper pattern
   - Support for complex queries with DQL

2. **Eloquent**
   - Laravel's ORM
   - Active record implementation
   - Simple, expressive syntax

3. **Propel**
   - Active record ORM
   - Schema-driven code generation

### Ruby
1. **Active Record**
   - Part of Ruby on Rails
   - Convention over configuration
   - Seamless integration with Rails

2. **Sequel**
   - Standalone ORM for Ruby
   - Flexible query interface
   - Support for advanced database features

### Go
1. **GORM**
   - Full-featured ORM with callbacks
   - Auto migrations
   - Associations and hooks

2. **SQLBoiler**
   - Code generation approach
   - Type-safe database queries
   - Performance-focused

## Comparative Analysis

| Feature | Entity Framework | Hibernate | SQLAlchemy | TypeORM | Active Record |
|---------|------------------|-----------|------------|---------|---------------|
| Language | .NET (C#/VB) | Java | Python | TypeScript/JS | Ruby |
| Mapping Style | Code-First/DB-First | Both | Both | Both | Convention-Based |
| Query Language | LINQ | HQL/JPQL | SQL Expression | QueryBuilder | Ruby DSL |
| Schema Evolution | Migrations | Schema Tool | Alembic | Migrations | Migrations |
| Performance | Good | Good | Excellent | Good | Moderate |
| Learning Curve | Moderate | Steep | Steep | Moderate | Gentle |

Entity Framework sits comfortably among the major ORMs with comparable features to Hibernate and SQLAlchemy while integrating seamlessly with the .NET ecosystem and leveraging LINQ for type-safe queries.

## Entity Framework
# Step-by-Step Guide to Implementing Entity Framework Core in Your BookAPI Project

## Step 1: Install Required NuGet Packages

Open a terminal in your project directory and run:

```bash
# Install EF Core and SQLite provider
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design

# For migrations support
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

## Step 2: Create a DbContext Class

1. Create a `Data` folder in your project if it doesn't exist
2. Add a new file called `BookDbContext.cs`:

```csharp
using BookAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookAPI.Data;

public class BookDbContext : DbContext
{
    public BookDbContext(DbContextOptions<BookDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Book> Books { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entity properties
        modelBuilder.Entity<Book>()
            .Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        modelBuilder.Entity<Book>()
            .Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(100);
            
        // Seed data
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "To Kill a Mockingbird", Author = "Harper Lee", Year = 1960, Genre = "Fiction", IsAvailable = true },
            new Book { Id = 2, Title = "1984", Author = "George Orwell", Year = 1949, Genre = "Dystopian", IsAvailable = true },
            new Book { Id = 3, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Year = 1925, Genre = "Classic", IsAvailable = true }
        );
    }
}
```

## Step 3: Add Connection String to appsettings.json

Open `appsettings.json` and add your connection string:

```json
{
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=books.db"
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

## Step 4: Register DbContext in Program.cs

Modify your `Program.cs` file to register the `DbContext`:

```csharp
using BookAPI.Models;
using BookAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Book API", Version = "v1" });
});

// Add EF Core with SQLite
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

// Rest of your existing configuration...
```

## Step 5: Initialize the Database

Add this code after `var app = builder.Build();` to ensure the database is created:

```csharp
// Ensure database is created with seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BookDbContext>();
    dbContext.Database.EnsureCreated();
}
```

## Step 6: Update Your Endpoints to Use EF Core

Replace your in-memory list endpoints with EF Core database operations:

```csharp
// GET - Get all books
app.MapGet("/api/books", async (BookDbContext db) =>
    await db.Books.ToListAsync())
.WithName("GetAllBooks");

// GET - Get a specific book by ID
app.MapGet("/api/books/{id}", async (int id, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    return book == null ? Results.NotFound() : Results.Ok(book);
})
.WithName("GetBookById");

// POST - Add a new book
app.MapPost("/api/books", async (Book book, BookDbContext db) =>
{
    db.Books.Add(book);
    await db.SaveChangesAsync();
    return Results.Created($"/api/books/{book.Id}", book);
})
.WithName("AddBook");

// PUT - Update a book
app.MapPut("/api/books/{id}", async (int id, Book updatedBook, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book == null) return Results.NotFound();
    
    book.Title = updatedBook.Title;
    book.Author = updatedBook.Author;
    book.Year = updatedBook.Year;
    book.Genre = updatedBook.Genre;
    book.IsAvailable = updatedBook.IsAvailable;
    
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("UpdateBook");

// PATCH - Update book availability
app.MapPatch("/api/books/{id}/availability", async (int id, bool isAvailable, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book == null) return Results.NotFound();
    
    book.IsAvailable = isAvailable;
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("UpdateBookAvailability");

// DELETE - Delete a book
app.MapDelete("/api/books/{id}", async (int id, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book == null) return Results.NotFound();
    
    db.Books.Remove(book);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteBook");
```

## Step 7: Set Up EF Core Migrations

To enable proper schema updates, set up migrations:

1. Install the EF Core CLI tools globally if you haven't already:

```bash
dotnet tool install --global dotnet-ef
```

2. Create your initial migration:

```bash
dotnet ef migrations add InitialCreate
```

3. Apply the migration to create your database:

```bash
dotnet ef database update
```

## Step 8: Run Your Application

```bash
dotnet run
```

Your application should now be using Entity Framework Core with SQLite for data persistence!

## Updating Your Data Model

When you need to update your data model (like adding fields or changing constraints), follow these steps:

### Step 1: Modify Your Model Class

For example, let's add a `PublisherId` property to the `Book` class:

```csharp
using System;

namespace BookAPI.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Genre { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public int? PublisherId { get; set; } // New property
}
```

### Step 2: Create a Migration

Run this command to create a migration for your changes:

```bash
dotnet ef migrations add AddPublisherId
```

This creates migration files in a `Migrations` folder in your project.

### Step 3: Apply the Migration

Apply the migration to update the database schema:

```bash
dotnet ef database update
```

### Step 4: Update Your API Endpoints (If Needed)

If your endpoints need to handle the new field, update them accordingly:

```csharp
// PUT - Update a book
app.MapPut("/api/books/{id}", async (int id, Book updatedBook, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book == null) return Results.NotFound();
    
    book.Title = updatedBook.Title;
    book.Author = updatedBook.Author;
    book.Year = updatedBook.Year;
    book.Genre = updatedBook.Genre;
    book.IsAvailable = updatedBook.IsAvailable;
    book.PublisherId = updatedBook.PublisherId; // Handle the new field
    
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("UpdateBook");
```

## Adding Related Entities (Relationships)

If you want to add a related entity (like `Publisher`):

### Step 1: Create the New Entity Class

```csharp
namespace BookAPI.Models;

public class Publisher
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
```

### Step 2: Update the Book Class with Navigation Property

```csharp
namespace BookAPI.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Genre { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    
    // Foreign key
    public int? PublisherId { get; set; }
    
    // Navigation property
    public Publisher? Publisher { get; set; }
}
```

### Step 3: Update DbContext to Include the New Entity

```csharp
using BookAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookAPI.Data;

public class BookDbContext : DbContext
{
    public BookDbContext(DbContextOptions<BookDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Book> Books { get; set; }
    public DbSet<Publisher> Publishers { get; set; } // Add this line
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entity properties for Book (existing code)
        
        // Configure relationship
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Publisher)
            .WithMany(p => p.Books)
            .HasForeignKey(b => b.PublisherId);
        
        // Seed data for Publisher
        modelBuilder.Entity<Publisher>().HasData(
            new Publisher { Id = 1, Name = "Penguin Books", Location = "London" },
            new Publisher { Id = 2, Name = "HarperCollins", Location = "New York" }
        );
        
        // Seed data for Book (update with PublisherId)
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "To Kill a Mockingbird", Author = "Harper Lee", Year = 1960, Genre = "Fiction", IsAvailable = true, PublisherId = 1 },
            new Book { Id = 2, Title = "1984", Author = "George Orwell", Year = 1949, Genre = "Dystopian", IsAvailable = true, PublisherId = 1 },
            new Book { Id = 3, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Year = 1925, Genre = "Classic", IsAvailable = true, PublisherId = 2 }
        );
    }
}
```

### Step 4: Create and Apply Migration

```bash
dotnet ef migrations add AddPublisherEntity
dotnet ef database update
```

### Step 5: Update Your API Endpoints to Include Related Data

```csharp
// GET - Get all books with publishers
app.MapGet("/api/books", async (BookDbContext db) =>
    await db.Books.Include(b => b.Publisher).ToListAsync())
.WithName("GetAllBooks");
```

By following these steps, you'll have a fully functional EF Core implementation with the ability to update your data models as your application evolves.

Similar code found with 1 license type


## Relational Databases

### 1. SQL Server

**Description:** Microsoft's enterprise-grade relational database system.

**Integration with Entity Framework:**
```csharp
// Add these using statements
using Microsoft.EntityFrameworkCore;
using BookAPI.Data;

// Add before builder.Build()
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Connection String (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BookAPI;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Pros:**
- Robust enterprise features
- Excellent integration with .NET
- Advanced query optimization
- Strong transaction support

**Cons:**
- Higher resource requirements
- Potentially higher licensing costs
- More complex setup compared to lightweight options

### 2. SQLite

**Description:** Lightweight, file-based relational database, ideal for smaller applications or development.

**Integration with Entity Framework:**
```csharp
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
```

**Connection String (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=books.db"
  }
}
```

**Pros:**
- No separate server installation required
- Database is a single file
- Low resource consumption
- Easy deployment

**Cons:**
- Limited concurrent write operations
- Not suitable for high-traffic applications
- Limited advanced database features

### 3. MySQL

**Description:** Popular open-source relational database system.

**Integration with Entity Framework:**
```csharp
// First install: dotnet add package Pomelo.EntityFrameworkCore.MySql
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySqlConnection"),
        new MySqlServerVersion(new Version(8, 0, 28))
    ));
```

**Connection String (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Database=BookAPI;User=root;Password=password;"
  }
}
```

**Pros:**
- Open-source with free community edition
- Good performance
- Cross-platform support
- Large community and resources

**Cons:**
- Less native integration with .NET compared to SQL Server
- Some enterprise features only in paid versions
- Performance limitations at extreme scale

### 4. PostgreSQL

**Description:** Advanced open-source relational database with robust feature set.

**Integration with Entity Framework:**
```csharp
// First install: dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));
```

**Connection String (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Database=BookAPI;Username=postgres;Password=password"
  }
}
```

**Pros:**
- Advanced features (JSON support, complex queries)
- Excellent data integrity
- Strong standards compliance
- Superior handling of concurrent operations
- Open-source

**Cons:**
- Steeper learning curve
- Slightly more complex setup compared to MySQL
- Requires more tuning for optimal performance

## Document Databases

### 1. MongoDB

**Description:** Popular NoSQL document database storing data in JSON-like documents.

**Integration with .NET:**
```csharp
// First install: dotnet add package MongoDB.Driver
using MongoDB.Driver;

// Add to services
builder.Services.AddSingleton<IMongoClient>(sp => 
    new MongoClient(builder.Configuration.GetConnectionString("MongoConnection")));
builder.Services.AddSingleton<IMongoDatabase>(sp => 
    sp.GetRequiredService<IMongoClient>().GetDatabase("BookAPI"));
```

**Configuration (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "MongoConnection": "mongodb://localhost:27017"
  }
}
```

**MongoDB Repository Implementation:**
```csharp
using MongoDB.Driver;
using BookAPI.Models;

public class BookRepository
{
    private readonly IMongoCollection<Book> _books;

    public BookRepository(IMongoDatabase database)
    {
        _books = database.GetCollection<Book>("Books");
    }

    public async Task<List<Book>> GetAllAsync() => 
        await _books.Find(_ => true).ToListAsync();

    public async Task<Book> GetByIdAsync(int id) => 
        await _books.Find(b => b.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Book book) => 
        await _books.InsertOneAsync(book);

    public async Task UpdateAsync(int id, Book book) => 
        await _books.ReplaceOneAsync(b => b.Id == id, book);

    public async Task RemoveAsync(int id) => 
        await _books.DeleteOneAsync(b => b.Id == id);
}
```

**Pros:**
- Schema flexibility
- JSON-native format
- Easy horizontal scaling
- Good performance for read-heavy operations
- Works well with web applications

**Cons:**
- Less robust transaction support
- Not ideal for complex relationships
- Limited join capabilities

### 2. Cloud Document Database Options

#### Azure Cosmos DB

**Description:** Microsoft's globally distributed, multi-model database service.

**Integration with .NET:**
```csharp
// First install: dotnet add package Microsoft.Azure.Cosmos
using Microsoft.Azure.Cosmos;

builder.Services.AddSingleton(sp => 
{
    var connectionString = builder.Configuration.GetConnectionString("CosmosDB");
    return new CosmosClient(connectionString);
});

builder.Services.AddSingleton<CosmosDbService>();
```

**Service Implementation:**
```csharp
using Microsoft.Azure.Cosmos;
using BookAPI.Models;

public class CosmosDbService
{
    private readonly Container _container;

    public CosmosDbService(CosmosClient cosmosClient)
    {
        var database = cosmosClient.GetDatabase("BookDatabase");
        _container = database.GetContainer("Books");
    }

    public async Task<IEnumerable<Book>> GetBooksAsync()
    {
        var query = _container.GetItemQueryIterator<Book>(new QueryDefinition("SELECT * FROM c"));
        var results = new List<Book>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }
        return results;
    }

    // Other CRUD methods
}
```

**Pros:**
- Global distribution
- Multiple consistency levels
- Multiple API interfaces (SQL, MongoDB, Gremlin, etc.)
- Automatic indexing
- Elastic scaling

**Cons:**
- Higher cost compared to self-hosted options
- Complex pricing model
- Potential for unexpected costs with high traffic

#### AWS DynamoDB

**Description:** Amazon's fully managed NoSQL database service.

**Integration with .NET:**
```csharp
// First install: dotnet add package AWSSDK.DynamoDBv2
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();
```

**Pros:**
- Fully managed service
- Millisecond performance at scale
- Automatic scaling
- Point-in-time recovery
- On-demand capacity mode

**Cons:**
- Limited query capabilities
- Restrictive data model
- Less flexibility compared to MongoDB

#### Google Cloud Firestore

**Description:** Scalable NoSQL document database for mobile, web, and server development.

**Integration with .NET:**
```csharp
// First install: dotnet add package Google.Cloud.Firestore
using Google.Cloud.Firestore;

builder.Services.AddSingleton(sp => 
{
    var projectId = builder.Configuration["Firestore:ProjectId"];
    return FirestoreDb.Create(projectId);
});
```

**Pros:**
- Real-time updates
- Offline support
- Strong security rules
- Automatic scaling
- Seamless integration with other Google services

**Cons:**
- Limited join capabilities
- Higher cost at scale
- More complex query syntax

## Caching Solutions

### 1. In-Memory Cache

**Description:** Built-in .NET caching for temporary data storage.

**Integration with .NET:**
```csharp
builder.Services.AddMemoryCache();

// In a controller or service
using Microsoft.Extensions.Caching.Memory;

public class BookService
{
    private readonly IMemoryCache _cache;
    private readonly BookDbContext _context;
    
    public BookService(IMemoryCache cache, BookDbContext context)
    {
        _cache = cache;
        _context = context;
    }
    
    public async Task<List<Book>> GetAllBooksAsync()
    {
        // Try to get from cache
        if (_cache.TryGetValue("all_books", out List<Book> books))
            return books;
            
        // Get from database
        books = await _context.Books.ToListAsync();
        
        // Store in cache for 10 minutes
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            
        _cache.Set("all_books", books, cacheOptions);
        
        return books;
    }
}
```

**Pros:**
- Simple to implement
- No additional dependencies
- Fast performance

**Cons:**
- Limited to single server
- Memory constraints
- No persistence between restarts

### 2. Redis Cache

**Description:** In-memory data structure store used as cache, database, and message broker.

**Integration with .NET:**
```csharp
// First install: dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "BookAPI_";
});
```

**Using Redis Cache:**
```csharp
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public class BookService
{
    private readonly IDistributedCache _cache;
    private readonly BookDbContext _context;
    
    public BookService(IDistributedCache cache, BookDbContext context)
    {
        _cache = cache;
        _context = context;
    }
    
    public async Task<Book> GetBookByIdAsync(int id)
    {
        string cacheKey = $"book_{id}";
        
        // Try to get from cache
        string cachedBook = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedBook))
        {
            return JsonSerializer.Deserialize<Book>(cachedBook);
        }
        
        // Get from database
        var book = await _context.Books.FindAsync(id);
        if (book == null) return null;
        
        // Store in cache for 1 hour
        var cacheOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));
            
        await _cache.SetStringAsync(
            cacheKey, 
            JsonSerializer.Serialize(book), 
            cacheOptions
        );
        
        return book;
    }
}
```

**Pros:**
- Distributed caching across multiple servers
- High performance
- Rich data structures
- Persistence options
- Pub/sub capabilities

**Cons:**
- Requires additional infrastructure
- More complex setup
- Additional management overhead

## Entity Framework Core for Your Project

Entity Framework Core is the ideal ORM solution for your BookAPI project. Here's how to implement it:

### 1. Create DbContext

```csharp
using BookAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookAPI.Data;

public class BookDbContext : DbContext
{
    public BookDbContext(DbContextOptions<BookDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Book> Books { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entity properties
        modelBuilder.Entity<Book>()
            .Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        modelBuilder.Entity<Book>()
            .Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(100);
            
        // Seed data
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "To Kill a Mockingbird", Author = "Harper Lee", Year = 1960, Genre = "Fiction", IsAvailable = true },
            new Book { Id = 2, Title = "1984", Author = "George Orwell", Year = 1949, Genre = "Dystopian", IsAvailable = true },
            new Book { Id = 3, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Year = 1925, Genre = "Classic", IsAvailable = true }
        );
    }
}
```

### 2. Update Program.cs to Use Entity Framework

```csharp
using BookAPI.Models;
using BookAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Book API", Version = "v1" });
});

// Add EF Core with SQLite (or any other provider)
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Ensure database is created with seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BookDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure middleware
app.UseCors("AllowReactApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/HelloWorld", () =>
{
    return "Hello World!";
})
.WithName("HelloWorld")
.WithOpenApi();

// GET - Get all books
app.MapGet("/api/books", async (BookDbContext db) =>
    await db.Books.ToListAsync())
.WithName("GetAllBooks");

// GET - Get a specific book by ID
app.MapGet("/api/books/{id}", async (int id, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    return book == null ? Results.NotFound() : Results.Ok(book);
})
.WithName("GetBookById");

// POST - Add a new book
app.MapPost("/api/books", async (Book book, BookDbContext db) =>
{
    db.Books.Add(book);
    await db.SaveChangesAsync();
    return Results.Created($"/api/books/{book.Id}", book);
})
.WithName("AddBook");

// PUT - Update a book
app.MapPut("/api/books/{id}", async (int id, Book updatedBook, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book == null) return Results.NotFound();
    
    book.Title = updatedBook.Title;
    book.Author = updatedBook.Author;
    book.Year = updatedBook.Year;
    book.Genre = updatedBook.Genre;
    book.IsAvailable = updatedBook.IsAvailable;
    
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("UpdateBook");

// PATCH - Update book availability
app.MapPatch("/api/books/{id}/availability", async (int id, bool isAvailable, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book == null) return Results.NotFound();
    
    book.IsAvailable = isAvailable;
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("UpdateBookAvailability");

// DELETE - Delete a book
app.MapDelete("/api/books/{id}", async (int id, BookDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book == null) return Results.NotFound();
    
    db.Books.Remove(book);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteBook");

app.Run();
```

### 3. Add Connection String to appsettings.json

```json
{
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=books.db"
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

## Entity Framework Core Benefits

1. **LINQ Support**: Type-safe, intuitive queries that are translated to SQL
2. **Change Tracking**: Automatically tracks changes to entities
3. **Database-First or Code-First**: Generate models from existing DB or create DB from models
4. **Migrations**: Easily update database schema as your models evolve
5. **Multiple Database Providers**: Switch between SQL Server, SQLite, PostgreSQL, etc.
6. **Lazy and Eager Loading**: Load related entities as needed or upfront
7. **Concurrency Control**: Built-in optimistic concurrency handling
8. **Transaction Support**: Ensure data consistency across multiple operations

## Comparison Summary for Your Project

| Option | Best For | Consider When |
|--------|----------|--------------|
| **SQLite with EF** | Development, small apps | You want simplicity, single file DB, low resource usage |
| **SQL Server with EF** | Enterprise, Windows-centric | You need robust features, scaling, enterprise support |
| **PostgreSQL with EF** | Open-source, complex data | You want advanced features, JSONB support, multiplatform |
| **MongoDB** | Flexible schema, document data | Your data structure varies or is deeply nested |
| **Redis Cache + EF** | Performance optimization | You need to reduce database load, have many reads |

For your BookAPI project, **SQLite with EF Core** is likely the simplest starting point, with an easy upgrade path to SQL Server or PostgreSQL when needed.
