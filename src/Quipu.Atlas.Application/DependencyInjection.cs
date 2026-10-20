using Microsoft.Extensions.DependencyInjection;
using Quipu.Atlas.Application.Submit;
using Quipu.Atlas.Domain.Decisions;

namespace Quipu.Atlas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAtlasApplication(this IServiceCollection services)
    {
        services.AddScoped<OnboardingDecisionService>();
        services.AddScoped<ISubmitApplicationService, SubmitApplicationService>();

        return services;
    }
}
