using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quipu.Atlas.Domain.Applications;

namespace Quipu.Atlas.Infrastructure.Persistence.Configurations
{
    internal sealed class ApplicantDetailsConfiguration : IEntityTypeConfiguration<ApplicantDetails>
    {
        public void Configure(EntityTypeBuilder<ApplicantDetails> builder)
        {
            builder.ToTable("ApplicantDetails");
            builder.HasKey(x => x.ApplicationId);

            builder.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.DateOfBirth)
                .HasColumnType("date").
                IsRequired();

            builder.Property(x => x.MarketCode)
                .HasColumnType("char(2)")
                .HasMaxLength(2)
                .IsRequired();

            builder.Property(x => x.NationalityCode)
                .HasColumnType("varchar(3)")
                .HasMaxLength(3);

            builder.Property(x => x.IdentifierType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.IdentifierValue)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(x => x.IssuingCountryCode)
                .HasColumnType("char(3)")
                .HasMaxLength(3);

            builder.Property(x => x.Email)
                .HasMaxLength(254)
                .IsRequired();

            builder.Property(x => x.Phone)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(x => x.TermsAcceptedAtUtc)
                .IsRequired();
        }
    }
}
