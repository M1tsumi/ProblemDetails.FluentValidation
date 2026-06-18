using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ProblemDetails.FluentValidation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFluentValidationProblemDetails(
        this IServiceCollection services,
        Action<FluentValidationProblemDetailsOptions>? configure = null)
    {
        var options = new FluentValidationProblemDetailsOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));

        services.TryAddSingleton<IFluentValidationProblemDetailsMapper, DefaultFluentValidationProblemDetailsMapper>();

        return services;
    }
}
