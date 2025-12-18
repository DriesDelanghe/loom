---
alwaysApply: true
---

# Loom .NET Code Style Rules

## Project Structure

- **Domain Layer** (`Loom.Services.{Service}.Domain`): Core business entities, enums, domain logic
- **Core Layer** (`Loom.Services.{Service}.Core`): Commands, Queries, Handlers, Services, DbContext
- **Contracts Layer** (`Loom.Services.{Service}.Contracts`): DTOs for API contracts
- **Api Layer** (`Loom.Services.{Service}.Api`): Controllers, Program.cs, API configuration

## Namespace Conventions

- Follow pattern: `Loom.Services.{ServiceName}.{Layer}.{SubFolder}`
- Examples:
  - `Loom.Services.Configuration.Core.Commands`
  - `Loom.Services.Configuration.Core.Commands.Handlers`
  - `Loom.Services.Configuration.Core.Queries`
  - `Loom.Services.Configuration.Domain.Graph`
  - `Loom.Services.Configuration.Domain.Persistence`
  - `Loom.Services.Configuration.Contracts.Dtos.Commands`
  - `Loom.Services.Configuration.Api.Controllers`

## Naming Conventions

### Commands
- Use `record` types
- Name: `{Action}{Entity}Command` (e.g., `AddConnectionCommand`, `PublishWorkflowVersionCommand`)
- Properties: PascalCase
- Parameter ordering: Primary identifiers first (e.g., `WorkflowVersionId`), then related entities, then options
- Place in: `Core/Commands/`

### Command Handlers
- Use `class` types
- Name: `{Action}{Entity}CommandHandler` (e.g., `AddConnectionCommandHandler`)
- Implement: `ICommandHandler<TCommand, TResult>`
- Method: `public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)`
- Place in: `Core/Commands/Handlers/`
- Private fields: Use `_` prefix (e.g., `_dbContext`)
- Parameter ordering in HandleAsync: command first, then cancellationToken
- Extract complex validation to private static methods when appropriate

### Queries
- Use `record` types
- Name: `Get{Entity}Query` or `Get{Entity}DetailsQuery` (e.g., `GetWorkflowVersionDetailsQuery`)
- Properties: PascalCase
- Parameter ordering: Primary identifiers first (e.g., `WorkflowVersionId`)
- Place in: `Core/Queries/`

### Query Handlers
- Use `class` types
- Name: `Get{Entity}QueryHandler` (e.g., `GetWorkflowVersionDetailsQueryHandler`)
- Implement: `IQueryHandler<TQuery, TResult>`
- Method: `public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default)`
- Place in: `Core/Queries/Handlers/`

### DTOs (Data Transfer Objects)
- Use `record` types
- Request DTOs: `{Action}{Entity}Request` (e.g., `AddConnectionRequest`)
- Response DTOs: `{Entity}Response` or `{Entity}DetailsResponse` (e.g., `WorkflowVersionDetailsResponse`)
- Place in: `Contracts/Dtos/Commands/` or `Contracts/Dtos/Queries/`
- Properties: PascalCase

### Domain Models
- Use `class` types
- Name: Entity name without suffix (e.g., `Node`, `Connection`, `WorkflowVersion`)
- Properties: PascalCase
- Required non-nullable strings: Use `= default!` (e.g., `public string Key { get; set; } = default!;`)
- Nullable properties: Use `?` (e.g., `public string? Name { get; set; }`)
- Place in: `Domain/{SubFolder}/` (e.g., `Domain/Graph/`, `Domain/Workflows/`)

### Persistence Entities
- Use `class` types
- Name: `{Entity}Entity` (e.g., `NodeEntity`, `ConnectionEntity`)
- Properties: PascalCase
- Navigation properties: Use `= null!` (e.g., `public WorkflowVersionEntity WorkflowVersion { get; set; } = null!;`)
- Methods:
  - `public {DomainModel} ToDomain()` - converts entity to domain model
  - `public static {Entity}Entity FromDomain({DomainModel} model)` - creates entity from domain model
- Place in: `Domain/Persistence/`

### Controllers
- Use `class` types
- Name: `{Entity}Controller` (e.g., `ConnectionsController`, `WorkflowsController`)
- Attributes: `[ApiController]` and `[Route("api/[controller]")]`
- Constructor: Inject handlers via constructor
- Private fields: Use `_` prefix (e.g., `_addConnectionHandler`)
- Methods: `public async Task<ActionResult<T>> {Action}([FromBody] {Request} request, CancellationToken cancellationToken)`
- Place in: `Api/Controllers/`

### Services
- Use `class` types for implementations
- Use `interface` types for contracts
- Interface naming: `I{ServiceName}` (e.g., `IWorkflowValidator`, `IWorkflowCompiler`)
- Implementation naming: `{ServiceName}` (e.g., `WorkflowValidator`, `WorkflowCompiler`)
- Place interfaces and implementations in: `Core/Services/`

### Static Classes
- Use for utility/metadata classes (e.g., `NodeTypeMetadata`)
- Private nested classes: Use for internal data structures
  - Name: `{Purpose}Info` or descriptive name (e.g., `NodeTypeInfo`)
  - Properties: Read-only with constructor initialization
  - Example:
```csharp
private class NodeTypeInfo
{
    public NodeCategory Category { get; }
    public IReadOnlyList<string> AllowedOutcomes { get; }
    public bool IsControl { get; }

    public NodeTypeInfo(NodeCategory category, string[] allowedOutcomes, bool isControl)
    {
        Category = category;
        AllowedOutcomes = allowedOutcomes;
        IsControl = isControl;
    }
}
```
- Static readonly fields: Use `private static readonly` for immutable collections/dictionaries
- Example:
```csharp
private static readonly Dictionary<NodeType, NodeTypeInfo> _metadata = new()
{
    { NodeType.Action, new NodeTypeInfo(...) }
};
```

### Enums
- Place in `Domain/{SubFolder}/` (e.g., `Domain/Graph/`, `Domain/Workflows/`)
- Name: PascalCase (e.g., `NodeType`, `WorkflowStatus`, `TriggerType`)
- Values: PascalCase (e.g., `Draft`, `Published`, `Action`, `Condition`)
- One enum per file
- File name matches enum name

## Code Style

### General
- Use `var` for local variables
- Use string interpolation for error messages: `$"Entity {id} not found"`
- Use null-conditional operators where appropriate: `config?.RootElement.GetRawText()`
- Use `default!` for required non-nullable reference types
- Use `null!` for required navigation properties in entities

### Null Checking
- Use traditional null checks: `if (entity == null)`
- Use null-conditional operators: `config?.RootElement.GetRawText()`
- Use null-coalescing when appropriate: `name ?? "Default"`
- Pattern matching can be used but traditional checks are preferred for consistency

### Collection Types
- **Return types**: Use `IReadOnlyList<T>` or `IReadOnlyCollection<T>` for public methods
- **Internal collections**: Use `List<T>`, `Dictionary<TKey, TValue>`, `HashSet<T>` for internal operations
- **Empty collections**: Use `Array.Empty<T>()` for empty arrays
- Example:
```csharp
public static IReadOnlyList<string> GetAllowedOutcomes(NodeType nodeType)
{
    return _metadata.TryGetValue(nodeType, out var info)
        ? info.AllowedOutcomes
        : throw new ArgumentException(...);
}
```

### Collection Initialization
- Arrays: Use `new[] { "value1", "value2" }`
- Dictionaries/Lists: Use `new()` when type can be inferred
- Example:
```csharp
private static readonly Dictionary<NodeType, NodeTypeInfo> _metadata = new()
{
    { NodeType.Action, new NodeTypeInfo(...) }
};

return category switch
{
    NodeCategory.Action => new[] { "Completed", "Failed" },
    _ => Array.Empty<string>()
};
```

### Switch Expressions
- Prefer switch expressions over switch statements when returning values
- Use for mapping, transformations, or simple conditional returns
- Example:
```csharp
return category switch
{
    NodeCategory.Action => new[] { "Completed", "Failed" },
    NodeCategory.Condition => new[] { "True", "False" },
    NodeCategory.Validation => new[] { "Valid", "Invalid" },
    NodeCategory.Control => GetAllowedOutcomes(nodeType),
    _ => Array.Empty<string>()
};
```

### Dictionary Lookups
- Use `TryGetValue` with out parameters for safe lookups
- Throw exceptions for invalid lookups when appropriate
- Example:
```csharp
return _metadata.TryGetValue(nodeType, out var info)
    ? info.Category
    : throw new ArgumentException($"Unknown node type: {nodeType}", nameof(nodeType));
```

### Async/Await
- Always use `async Task<T>` for async methods
- Always use `await` for async operations
- Always include `CancellationToken cancellationToken = default` parameter
- Use `FirstOrDefaultAsync()`, `ToListAsync()`, `AnyAsync()`, `SaveChangesAsync()` for EF Core operations

### Error Handling
- Use `InvalidOperationException` for business logic errors (e.g., "Entity not found", "Invalid state")
- Use `ArgumentException` for invalid parameters (e.g., "Unknown node type")
- Error messages: Be descriptive and include relevant context (IDs, names, etc.)
- Format: `throw new InvalidOperationException($"Entity {id} not found");`

### Entity Framework Core
- Use `Include()` and `ThenInclude()` for eager loading
- Use `FirstOrDefaultAsync()` for single entity queries
- Use `ToListAsync()` for collection queries
- Use `AnyAsync()` for existence checks
- Use `Where()` for filtering before async operations
- Always pass `cancellationToken` to async EF Core methods

### LINQ
- Use method syntax (e.g., `.Where()`, `.Select()`, `.FirstOrDefault()`)
- Use query syntax only when it significantly improves readability
- Chain operations for readability
- Materialize when needed:
  - Use `.ToList()` when you need a list that will be modified or enumerated multiple times
  - Use `.ToHashSet()` for uniqueness checks or fast lookups
  - Use `.ToDictionary()` for key-value lookups
  - Keep as `IQueryable<T>` for EF Core queries that will be further filtered
- Example:
```csharp
var nodeIds = version.Nodes.Select(n => n.Id).ToHashSet();
var nodeKeys = version.Nodes.Select(n => n.Key).ToList();
```

### Dependency Injection
- Use constructor injection
- Register services in `Program.cs` using `builder.Services.AddScoped<T>()`
- Register handlers explicitly (not via reflection)

### Validation
- Validate in command handlers before performing operations
- Check for null entities: `if (entity == null) throw new InvalidOperationException(...)`
- Check business rules: `if (version.Status != WorkflowStatus.Draft) throw new InvalidOperationException(...)`
- Use descriptive error messages
- Validate early: Check prerequisites before performing operations
- Validate in dedicated methods when logic is complex (e.g., `ValidateTriggerBindings()`)

### File Organization
- One class/record per file
- File name matches type name
- Group related files in folders:
  - Commands: `Commands/`
  - Command Handlers: `Commands/Handlers/`
  - Queries: `Queries/`
  - Query Handlers: `Queries/Handlers/`
  - Services: `Services/`
  - Controllers: `Controllers/`
  - Domain Models: `Domain/{SubFolder}/`
  - Persistence: `Domain/Persistence/`
  - DTOs: `Contracts/Dtos/{SubFolder}/`

### Using Statements
- Group using statements in this order:
  1. System namespaces (System.*, Microsoft.*)
  2. Third-party libraries (ProtoBuf, etc.)
  3. Project namespaces (Loom.Services.*)
- Order within groups: Alphabetically
- Use file-scoped namespaces when possible
- Example:
```csharp
using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;
```

### Comments
- Avoid unnecessary comments - code should be self-documenting
- Use XML documentation comments for public APIs if needed
- Use comments only when code behavior is non-obvious

### Formatting
- Use 4 spaces for indentation
- Place opening braces on new line
- Use blank lines to separate logical sections
- Remove trailing whitespace
- End files with a single blank line

## Example Patterns

### Command Handler Pattern
```csharp
using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class AddEntityCommandHandler : ICommandHandler<AddEntityCommand, Guid>
{
    private readonly ConfigurationDbContext _dbContext;

    public AddEntityCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddEntityCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Entities
            .FirstOrDefaultAsync(e => e.Id == command.EntityId, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException($"Entity {command.EntityId} not found");

        // Business logic validation
        if (entity.Status != EntityStatus.Valid)
            throw new InvalidOperationException("Entity is not in valid state");

        // Create new entity
        var newEntity = new EntityEntity
        {
            Id = Guid.NewGuid(),
            // ... properties
        };

        _dbContext.Entities.Add(newEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return newEntity.Id;
    }
}
```

### Query Handler Pattern
```csharp
using Microsoft.EntityFrameworkCore;

namespace Loom.Services.Configuration.Core.Queries.Handlers;

public class GetEntityQueryHandler : IQueryHandler<GetEntityQuery, EntityDto>
{
    private readonly ConfigurationDbContext _dbContext;

    public GetEntityQueryHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EntityDto> HandleAsync(GetEntityQuery query, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Entities
            .Include(e => e.RelatedEntities)
            .FirstOrDefaultAsync(e => e.Id == query.EntityId, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException($"Entity {query.EntityId} not found");

        return new EntityDto(
            entity.Id,
            entity.Name,
            // ... map properties
        );
    }
}
```

### Controller Pattern
```csharp
using Loom.Services.Configuration.Contracts.Dtos;
using Loom.Services.Configuration.Contracts.Dtos.Commands;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Loom.Services.Configuration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EntitiesController : ControllerBase
{
    private readonly AddEntityCommandHandler _addEntityHandler;

    public EntitiesController(AddEntityCommandHandler addEntityHandler)
    {
        _addEntityHandler = addEntityHandler;
    }

    [HttpPost]
    public async Task<ActionResult<IdResponse>> AddEntity(
        [FromBody] AddEntityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddEntityCommand(
            request.Property1,
            request.Property2
        );

        var id = await _addEntityHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }
}
```

### Domain Model Pattern
```csharp
namespace Loom.Services.Configuration.Domain.Entities;

public class Entity
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public string? Name { get; set; }
    public EntityType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Persistence Entity Pattern
```csharp
using Loom.Services.Configuration.Domain.Entities;

namespace Loom.Services.Configuration.Domain.Persistence;

public class EntityEntity
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public string? Name { get; set; }
    public EntityType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public WorkflowVersionEntity WorkflowVersion { get; set; } = null!;

    public Entity ToDomain()
    {
        return new Entity
        {
            Id = Id,
            Key = Key,
            Name = Name,
            Type = Type,
            CreatedAt = CreatedAt
        };
    }

    public static EntityEntity FromDomain(Entity entity)
    {
        return new EntityEntity
        {
            Id = entity.Id,
            Key = entity.Key,
            Name = entity.Name,
            Type = entity.Type,
            CreatedAt = entity.CreatedAt
        };
    }
}
```

## When Adding New Features

1. **Commands**: Create command record in `Core/Commands/`
2. **Command Handler**: Create handler class in `Core/Commands/Handlers/`
3. **DTOs**: Create request/response records in `Contracts/Dtos/Commands/` or `Contracts/Dtos/Queries/`
4. **Controller**: Add endpoint in appropriate controller in `Api/Controllers/`
5. **Registration**: Register handler in `Program.cs` using `AddScoped<T>()`
6. **Validation**: Add validation logic in handler or dedicated validator service
7. **Tests**: Add unit tests in `tests/{Project}.Tests/` following same patterns

## Additional Patterns

### Method Parameter Ordering
- Primary identifiers first (IDs, keys)
- Related entities second
- Options/configuration last
- CancellationToken always last (if present)
- Example: `HandleAsync(AddConnectionCommand command, CancellationToken cancellationToken = default)`

### Return Types
- Public methods: Return `IReadOnlyList<T>` or `IReadOnlyCollection<T>` when returning collections
- Internal methods: Can return `List<T>`, `HashSet<T>`, etc. as needed
- Avoid returning `IEnumerable<T>` from EF Core queries (materialize first)

### Exception Messages
- Include relevant context: IDs, names, current state
- Use string interpolation: `$"Entity {id} not found"`
- Be specific: `"Workflow version must have at least one trigger binding"` not `"Invalid workflow"`
