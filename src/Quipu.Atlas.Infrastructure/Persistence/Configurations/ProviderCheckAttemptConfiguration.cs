using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quipu.Atlas.Domain.Applications;

namespace Quipu.Atlas.Infrastructure.Persistence.Configurations
{
    internal sealed class ProviderCheckAttemptConfiguration : IEntityTypeConfiguration<ProviderCheckAttempt>
    {
        public void Configure(EntityTypeBuilder<ProviderCheckAttempt> builder)
        {
            builder.ToTable("ProviderCheckAttempts");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Provider).HasConversion<int>().IsRequired();
            builder.Property(x => x.AttemptNumber).IsRequired();
            builder.Property(x => x.Status).HasConversion<int>().IsRequired();
            builder.Property(x => x.ProviderReference).HasColumnType("varchar(128)").HasMaxLength(128);
            builder.Property(x => x.FailureCode).HasColumnType("varchar(64)").HasMaxLength(64);
            builder.Property(x => x.StartedAtUtc).IsRequired();

            builder.HasIndex(x => new { x.ApplicationId, x.Provider, x.AttemptNumber })
                .IsUnique()
                .HasDatabaseName("UX_ProviderCheckAttempts_Application_Provider_Attempt");

            builder.HasOne(x => x.IdNowResult)
                .WithOne(x => x.ProviderCheckAttempt)
                .HasForeignKey<IdNowResult>(x => x.ProviderCheckAttemptId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorldCheckResult)
                .WithOne(x => x.ProviderCheckAttempt)
                .HasForeignKey<WorldCheckResult>(x => x.ProviderCheckAttemptId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
