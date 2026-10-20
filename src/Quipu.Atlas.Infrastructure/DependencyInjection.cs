using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quipu.Atlas.Api.Persistence;
using Quipu.Atlas.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.Atlas.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAtlasInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Atlas");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'Atlas' is missing. Configure ConnectionStrings:Atlas.");
            }

            services.AddDbContextFactory<AtlasDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    sqlServer => sqlServer.MigrationsAssembly(typeof(AtlasDbContext).Assembly.GetName().Name)));
            services.AddScoped<IOnboardingApplicationStore, OnboardingApplicationStore>();

            return services;
        }
    }
}
