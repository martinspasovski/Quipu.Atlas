using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Quipu.Atlas.Infrastructure.Persistence
{

    public sealed class AtlasDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AtlasDbContext>
    {
        public AtlasDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();

            var connectionString = configuration.GetConnectionString("Atlas")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:Atlas was not found in appsettings.json.");

            var options = new DbContextOptionsBuilder<AtlasDbContext>()
                .UseSqlServer(
                    connectionString,
                    sqlServer => sqlServer.MigrationsAssembly(typeof(AtlasDbContext).Assembly.GetName().Name))
                .Options;

            return new AtlasDbContext(options);
        }
    }
}
