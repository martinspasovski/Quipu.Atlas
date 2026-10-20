using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.Atlas.Application.Providers.IdNow
{
    public sealed record IdNowCheckRequest(
        string DocumentType,
        string DocumentImage,
        string SelfieImage);
}

