using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quipu.Atlas.Domain.Applications;

namespace Quipu.Atlas.Infrastructure.Persistence.Configurations
{
    internal sealed class WorldCheckMatchConfiguration : IEntityTypeConfiguration<WorldCheckMatch>
    {
        public void Configure(EntityTypeBuilder<WorldCheckMatch> builder)
        {
            builder.ToTable("WorldCheckMatches");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ListType).HasConversion<int>().IsRequired();
            builder.Property(x => x.Score).HasPrecision(5, 4).IsRequired();
            builder.Property(x => x.Subject).HasMaxLength(256).IsRequired();
        }
    }
}
