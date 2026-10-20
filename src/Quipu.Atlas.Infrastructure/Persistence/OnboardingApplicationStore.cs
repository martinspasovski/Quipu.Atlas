using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Quipu.Atlas.Api.Persistence;
using Quipu.Atlas.Domain.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.Atlas.Infrastructure.Persistence
{
    internal sealed class OnboardingApplicationStore(
        IDbContextFactory<AtlasDbContext> dbContextFactory) : IOnboardingApplicationStore
    {
        public async Task<bool> TryCreateAsync(
            OnboardingApplication application,
            CancellationToken cancellationToken)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            dbContext.Applications.Add(application);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
            {
                return false;
            }
        }

        public async Task<OnboardingApplication?> FindByIdempotencyKeyAsync(
            string idempotencyKey,
            CancellationToken cancellationToken)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await dbContext.Applications
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Applicant)
                .Include(x => x.Documents)
                .Include(x => x.ProviderCheckAttempts)
                    .ThenInclude(x => x.IdNowResult)
                .Include(x => x.ProviderCheckAttempts)
                    .ThenInclude(x => x.WorldCheckResult)
                        .ThenInclude(x => x!.Matches)
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
        }

        public async Task<bool> TryAcquireProcessingLeaseAsync(
           Guid applicationId,
           Guid processingToken,
           DateTimeOffset nowUtc,
           DateTimeOffset leaseExpiresAtUtc,
           CancellationToken cancellationToken)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var updatedRows = await dbContext.Applications
                .Where(x => x.Id == applicationId &&
                            x.Status == ApplicationStatus.ChecksInProgress &&
                            (x.ProcessingLeaseExpiresAtUtc == null || x.ProcessingLeaseExpiresAtUtc <= nowUtc))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.ProcessingToken, processingToken)
                    .SetProperty(x => x.ProcessingLeaseExpiresAtUtc, leaseExpiresAtUtc)
                    .SetProperty(x => x.UpdatedAtUtc, nowUtc), cancellationToken);

            return updatedRows == 1;
        }

        public async Task SaveCheckOutcomeAsync(
         Guid applicationId,
         Guid processingToken,
         IReadOnlyCollection<ProviderCheckAttempt> providerAttempts,
         ApplicationStatus status,
         string? reasonCode,
         DateTimeOffset updatedAtUtc,
         CancellationToken cancellationToken)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var application = await dbContext.Applications
                .SingleOrDefaultAsync(
                    x => x.Id == applicationId && x.ProcessingToken == processingToken,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Processing lease for application '{applicationId}' is no longer owned by this request.");

            foreach (var attempt in providerAttempts)
            {
                application.ProviderCheckAttempts.Add(attempt);
            }

            application.Status = status;
            application.DecisionReasonCode = reasonCode;
            application.UpdatedAtUtc = updatedAtUtc;
            application.ProcessingToken = null;
            application.ProcessingLeaseExpiresAtUtc = null;

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            for (Exception? current = exception; current is not null; current = current.InnerException)
            {
                if (current is SqlException { Number: 2601 or 2627 })
                {
                    return true;
                }
            }

            return false;
        }
    }
}
