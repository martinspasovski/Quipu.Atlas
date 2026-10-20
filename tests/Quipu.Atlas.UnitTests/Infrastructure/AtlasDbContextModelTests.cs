using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Quipu.Atlas.Domain.Applications;
using Quipu.Atlas.Infrastructure.Persistence;
using Xunit;

namespace Quipu.Atlas.UnitTests.Infrastructure;

public sealed class AtlasDbContextModelTests
{
    [Fact]
    public void Model_should_map_all_onboarding_entities_to_the_expected_tables()
    {
        using var context = CreateContext();

        context.Model.FindEntityType(typeof(OnboardingApplication))!.GetTableName().Should().Be("Applications");
        context.Model.FindEntityType(typeof(ApplicantDetails))!.GetTableName().Should().Be("ApplicantDetails");
        context.Model.FindEntityType(typeof(ApplicationDocument))!.GetTableName().Should().Be("ApplicationDocuments");
        context.Model.FindEntityType(typeof(ProviderCheckAttempt))!.GetTableName().Should().Be("ProviderCheckAttempts");
        context.Model.FindEntityType(typeof(IdNowResult))!.GetTableName().Should().Be("IdNowResults");
        context.Model.FindEntityType(typeof(WorldCheckResult))!.GetTableName().Should().Be("WorldCheckResults");
        context.Model.FindEntityType(typeof(WorldCheckMatch))!.GetTableName().Should().Be("WorldCheckMatches");
    }

    [Fact]
    public void Application_model_should_enforce_idempotency_key_uniqueness_and_length()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(OnboardingApplication))!;
        var idempotencyKey = entity.FindProperty(nameof(OnboardingApplication.IdempotencyKey))!;
        var uniqueIndex = entity.GetIndexes().Single(index =>
            index.GetDatabaseName() == "UX_Applications_IdempotencyKey");

        idempotencyKey.GetMaxLength().Should().Be(128);
        idempotencyKey.IsNullable.Should().BeFalse();
        uniqueIndex.IsUnique.Should().BeTrue();
    }

    [Fact]
    public void Model_should_define_the_application_relationships_as_required_foreign_keys()
    {
        using var context = CreateContext();
        var application = context.Model.FindEntityType(typeof(OnboardingApplication))!;

        application.FindNavigation(nameof(OnboardingApplication.Applicant))!.ForeignKey.IsRequired.Should().BeTrue();
        application.FindNavigation(nameof(OnboardingApplication.Documents))!.ForeignKey.IsRequired.Should().BeTrue();
        application.FindNavigation(nameof(OnboardingApplication.ProviderCheckAttempts))!.ForeignKey.IsRequired.Should().BeTrue();
    }

    [Fact]
    public void Provider_attempt_model_should_prevent_duplicate_attempt_numbers_per_provider()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(ProviderCheckAttempt))!;
        var index = entity.GetIndexes().Single(index =>
            index.GetDatabaseName() == "UX_ProviderCheckAttempts_Application_Provider_Attempt");

        index.IsUnique.Should().BeTrue();
        index.Properties.Select(property => property.Name).Should()
            .Equal(nameof(ProviderCheckAttempt.ApplicationId), nameof(ProviderCheckAttempt.Provider), nameof(ProviderCheckAttempt.AttemptNumber));
    }

    private static AtlasDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AtlasDbContext>()
            .UseSqlServer("Server=(local);Database=Atlas;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new AtlasDbContext(options);
    }
}
