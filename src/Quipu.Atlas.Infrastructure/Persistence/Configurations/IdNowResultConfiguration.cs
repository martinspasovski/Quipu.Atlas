using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quipu.Atlas.Domain.Applications;

namespace Quipu.Atlas.Infrastructure.Persistence.Configurations
{
    internal sealed class IdNowResultConfiguration : IEntityTypeConfiguration<IdNowResult>
    {
        public void Configure(EntityTypeBuilder<IdNowResult> builder)
        {
            builder.ToTable("IdNowResults");
            builder.HasKey(x => x.ProviderCheckAttemptId);

            builder.Property(x => x.DocumentResult)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.FaceMatch)
                .IsRequired();

            builder.Property(x => x.Confidence)
                .HasPrecision(5, 4)
                .IsRequired();
        }
    }
}
