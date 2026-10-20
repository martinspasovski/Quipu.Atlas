using Microsoft.EntityFrameworkCore;
using Quipu.Atlas.Domain.Applications;
using Quipu.Atlas.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.Atlas.Infrastructure.Persistence
{
    public sealed class AtlasDbContext(DbContextOptions<AtlasDbContext>options) : DbContext(options)
    {
        public DbSet<OnboardingApplication> Applications => Set<OnboardingApplication>();
        public DbSet<ApplicantDetails> ApplicantDetails=> Set<ApplicantDetails>();
        public DbSet<ApplicationDocument> ApplicationDocuments => Set<ApplicationDocument>();
        public DbSet<ProviderCheckAttempt> ProviderCheckAttempts => Set<ProviderCheckAttempt>();
        public DbSet<IdNowResult> IdNowResults => Set<IdNowResult>();
        public DbSet<WorldCheckResult> WorldCheckResults => Set<WorldCheckResult>();
        public DbSet<WorldCheckMatch> WorldCheckMatchs => Set<WorldCheckMatch>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OnboardingApplicationConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicantDetailsConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new ProviderCheckAttemptConfiguration());
            modelBuilder.ApplyConfiguration(new IdNowResultConfiguration());
            modelBuilder.ApplyConfiguration(new WorldCheckResultConfiguration());
            modelBuilder.ApplyConfiguration(new WorldCheckMatchConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
