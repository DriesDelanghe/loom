using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class CreateDataModelCommandHandler : ICommandHandler<CreateDataModelCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public CreateDataModelCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(CreateDataModelCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.DataModels
            .FirstOrDefaultAsync(m => m.TenantId == command.TenantId && m.Key == command.Key, cancellationToken);

        if (existing != null)
            throw new InvalidOperationException($"Data model with key '{command.Key}' already exists for tenant {command.TenantId}");

        var entity = new DataModelEntity
        {
            Id = Guid.NewGuid(),
            TenantId = command.TenantId,
            Key = command.Key,
            Name = command.Name,
            Description = command.Description,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.DataModels.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
