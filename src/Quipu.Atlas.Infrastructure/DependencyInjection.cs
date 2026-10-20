using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quipu.Atlas.Api.Persistence;
using Quipu.Atlas.Application.Providers.IdNow;
using Quipu.Atlas.Application.Providers.WorldCheck;
using Quipu.Atlas.Infrastructure.Persistence;
using Quipu.Atlas.Infrastructure.Providers.IdNow;
using Quipu.Atlas.Infrastructure.Providers.WorldCheck;

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

            var useStubs = ReadBoolean(configuration, "Providers:UseStubs", defaultValue: true);
            var idNowBaseAddress = ReadBaseAddress(configuration, "Providers:IdNow:BaseAddress");
            var worldCheckBaseAddress = ReadBaseAddress(configuration, "Providers:WorldCheck:BaseAddress");
            var idNowTimeout = ReadPositiveInt(configuration, "Providers:IdNow:TimeoutSeconds", 5);
            var worldCheckTimeout = ReadPositiveInt(configuration, "Providers:WorldCheck:TimeoutSeconds", 5);

            var idNowClient = services.AddHttpClient<IIdNowProvider, IdNowHttpClient>(client =>
            {
                client.BaseAddress = idNowBaseAddress;
                client.Timeout = TimeSpan.FromSeconds(idNowTimeout);
            });

            var worldCheckClient = services.AddHttpClient<IWorldCheckProvider, WorldCheckHttpClient>(client =>
            {
                client.BaseAddress = worldCheckBaseAddress;
                client.Timeout = TimeSpan.FromSeconds(worldCheckTimeout);
            });

            if (useStubs)
            {
                var idNowScenario = configuration["Providers:IdNow:Stub:Scenario"] ?? "VALID";
                var idNowDelay = ReadNonNegativeInt(configuration, "Providers:IdNow:Stub:DelayMilliseconds", 0);
                var worldCheckScenario = configuration["Providers:WorldCheck:Stub:Scenario"] ?? "CLEAR";
                var worldCheckDelay = ReadNonNegativeInt(configuration, "Providers:WorldCheck:Stub:DelayMilliseconds", 0);

                idNowClient.ConfigurePrimaryHttpMessageHandler(
                    () => new IdNowStubHandler(idNowScenario, idNowDelay));
                worldCheckClient.ConfigurePrimaryHttpMessageHandler(
                    () => new WorldCheckStubHandler(worldCheckScenario, worldCheckDelay));
            }

            return services;
        }

        private static Uri ReadBaseAddress(IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException($"'{key}' must be an absolute HTTP or HTTPS URL.");
            }

            return uri;
        }

        private static int ReadPositiveInt(IConfiguration configuration, string key, int defaultValue)
        {
            var value = ReadNonNegativeInt(configuration, key, defaultValue);
            if (value == 0)
            {
                throw new InvalidOperationException($"'{key}' must be greater than zero.");
            }

            return value;
        }

        private static int ReadNonNegativeInt(IConfiguration configuration, string key, int defaultValue)
        {
            var rawValue = configuration[key];
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return defaultValue;
            }

            if (!int.TryParse(rawValue, out var value) || value < 0)
            {
                throw new InvalidOperationException($"'{key}' must be a non-negative integer.");
            }

            return value;
        }

        private static bool ReadBoolean(IConfiguration configuration, string key, bool defaultValue)
        {
            var rawValue = configuration[key];
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return defaultValue;
            }

            if (!bool.TryParse(rawValue, out var value))
            {
                throw new InvalidOperationException($"'{key}' must be true or false.");
            }

            return value;
        }
    }
}
