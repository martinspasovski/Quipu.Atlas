using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quipu.Atlas.Domain.Applications;

namespace Quipu.Atlas.Infrastructure.Persistence.Configurations
{
    internal sealed class ApplicationDocumentConfiguration : IEntityTypeConfiguration<ApplicationDocument>
    {
        public void Configure(EntityTypeBuilder<ApplicationDocument> builder)
        {
            builder.ToTable("ApplicationDocuments");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.StorageKey)
                .HasColumnType("varchar(512)")
                .HasMaxLength(512)
                .IsRequired();

            builder.Property(x => x.ContentType)
                .HasColumnType("varchar(128)")
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.SizeInBytes)
                .IsRequired();

            builder.Property(x => x.Sha256)
                .HasColumnType("binary(32)")
                .IsRequired();

            builder.Property(x => x.ReceivedAtUtc)
                .IsRequired();
        }
    }
}
