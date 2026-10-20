using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quipu.Atlas.Application.Providers.IdNow;
using Quipu.Atlas.Application.Providers.WorldCheck;
using Quipu.Atlas.Domain.Applications;
using Quipu.Atlas.Infrastructure;
using Xunit;

namespace Quipu.Atlas.UnitTests.Infrastructure;

public sealed class InfrastructureRegistrationTests
{
    [Fact]
    public async Task AddAtlasInfrastructure_should_use_the_valid_id_now_stub_scenario()
    {
        using var provider = BuildServiceProvider(("Providers:IdNow:Stub:Scenario", "VALID"));
        var client = provider.GetRequiredService<IIdNowProvider>();

        var result = await client.CheckAsync(
            new IdNowCheckRequest("PASSPORT", "document", "selfie"),
            CancellationToken.None);

        result.Status.Should().Be(ProviderAttemptStatus.ResponseReceived);
        result.Value.Should().NotBeNull();
        result.Value!.DocumentResult.Should().Be(IdNowDocumentResult.Valid);
        result.Value.FaceMatch.Should().BeTrue();
    }

    [Fact]
    public async Task AddAtlasInfrastructure_should_map_possible_world_check_matches()
    {
        using var provider = BuildServiceProvider(("Providers:WorldCheck:Stub:Scenario", "POSSIBLE_PEP_MATCH"));
        var client = provider.GetRequiredService<IWorldCheckProvider>();

        var result = await client.CheckAsync(
            new WorldCheckCheckRequest("Ada Lovelace", new DateOnly(1815, 12, 10), "GBR"),
            CancellationToken.None);

        result.Status.Should().Be(ProviderAttemptStatus.ResponseReceived);
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(WorldCheckScreeningStatus.PossibleMatch);
        result.Value.Matches.Should().ContainSingle(match =>
            match.ListType == WorldCheckListType.Pep && match.Score == 0.82m);
    }

    [Fact]
    public async Task AddAtlasInfrastructure_should_return_provider_failure_for_an_http_error_stub()
    {
        using var provider = BuildServiceProvider(
            ("Providers:IdNow:Stub:Scenario", "HTTP_ERROR"));
        var client = provider.GetRequiredService<IIdNowProvider>();

        var result = await client.CheckAsync(
            new IdNowCheckRequest("PASSPORT", "document", "selfie"),
            CancellationToken.None);

        result.Status.Should().Be(ProviderAttemptStatus.Failed);
        result.FailureCode.Should().Be("IDNOW_HTTP_503");
    }

    [Fact]
    public void AddAtlasInfrastructure_should_require_the_database_connection_string()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Providers:IdNow:BaseAddress"] = "https://idnow.test",
                ["Providers:WorldCheck:BaseAddress"] = "https://worldcheck.test"
            })
            .Build();
        var services = new ServiceCollection();

        var action = () => services.AddAtlasInfrastructure(configuration);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Connection string 'Atlas' is missing. Configure ConnectionStrings:Atlas.");
    }

    [Theory]
    [InlineData("ftp://provider.test", "'Providers:IdNow:BaseAddress' must be an absolute HTTP or HTTPS URL.")]
    [InlineData("not-a-url", "'Providers:IdNow:BaseAddress' must be an absolute HTTP or HTTPS URL.")]
    public void AddAtlasInfrastructure_should_reject_invalid_provider_base_addresses(
        string baseAddress,
        string expectedMessage)
    {
        var configuration = CreateConfiguration(
            ("Providers:IdNow:BaseAddress", baseAddress));
        var services = new ServiceCollection();

        var action = () => services.AddAtlasInfrastructure(configuration);

        action.Should().Throw<InvalidOperationException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void AddAtlasInfrastructure_should_reject_invalid_timeout_values()
    {
        var configuration = CreateConfiguration(("Providers:IdNow:TimeoutSeconds", "0"));
        var services = new ServiceCollection();

        var action = () => services.AddAtlasInfrastructure(configuration);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("'Providers:IdNow:TimeoutSeconds' must be greater than zero.");
    }

    private static ServiceProvider BuildServiceProvider(params (string Key, string Value)[] overrides)
    {
        var configuration = CreateConfiguration(overrides);
        var services = new ServiceCollection();
        services.AddAtlasInfrastructure(configuration);
        return services.BuildServiceProvider();
    }

    private static IConfiguration CreateConfiguration(params (string Key, string Value)[] overrides)
    {
        var values = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Atlas"] = "Server=(local);Database=Atlas;Trusted_Connection=True;TrustServerCertificate=True",
            ["Providers:IdNow:BaseAddress"] = "https://idnow.test",
            ["Providers:WorldCheck:BaseAddress"] = "https://worldcheck.test",
            ["Providers:UseStubs"] = "true"
        };

        foreach (var (key, value) in overrides)
        {
            values[key] = value;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
