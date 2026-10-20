using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quipu.Atlas.Domain.Applications;

namespace Quipu.Atlas.Infrastructure.Persistence.Configurations
{
    internal sealed class OnboardingApplicationConfiguration : IEntityTypeConfiguration<OnboardingApplication>
    {
        public void Configure(EntityTypeBuilder<OnboardingApplication> builder)
        {
            builder.ToTable("Applications");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.IdempotencyKey)
                .HasColumnType("varchar(128)")
                .HasMaxLength(128)
                .IsRequired();

            builder.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasDatabaseName("UX_Applications_IdempotencyKey");

            builder.Property(x => x.RequestHash)
                .HasColumnType("binary(32)")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.DecisionReasonCode)
                .HasColumnType("varchar(64)")
                .HasMaxLength(64);

            builder.Property(x => x.RowVersion).IsRowVersion();

            builder.HasOne(x => x.Applicant)
                .WithOne(x => x.Application)
                .HasForeignKey<ApplicantDetails>(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Documents)
                .WithOne(x => x.Application)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.ProviderCheckAttempts)
                .WithOne(x => x.Application)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
