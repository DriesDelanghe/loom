using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core;

namespace Loom.Services.Configuration.Core.Tests.TestHelpers;

public static class InMemoryDbContextFactory
{
    public static ConfigurationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ConfigurationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ConfigurationDbContext(options);
    }
}

