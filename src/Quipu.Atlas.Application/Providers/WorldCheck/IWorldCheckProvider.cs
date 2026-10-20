using Quipu.Atlas.Domain.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.Atlas.Application.Providers.WorldCheck
{
    public interface IWorldCheckProvider
    {
        Task<ProviderCallResult<WorldCheckCheckResponse>> CheckAsync(
            WorldCheckCheckRequest request,
            CancellationToken cancellationToken);
    }

}



