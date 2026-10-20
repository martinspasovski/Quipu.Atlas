using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quipu.Atlas.Domain.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.Atlas.Infrastructure.Persistence.Configurations
{

    internal sealed class WorldCheckResultConfiguration : IEntityTypeConfiguration<WorldCheckResult>
    {
        public void Configure(EntityTypeBuilder<WorldCheckResult> builder)
        {
            builder.ToTable("WorldCheckResults");
            builder.HasKey(x => x.ProviderCheckAttemptId);

            builder.Property(x => x.CaseId).HasColumnType("varchar(128)").HasMaxLength(128).IsRequired();
            builder.Property(x => x.Status).HasConversion<int>().IsRequired();

            builder.HasMany(x => x.Matches)
                .WithOne(x => x.WorldCheckResult)
                .HasForeignKey(x => x.WorldCheckResultId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
