using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ProblemDetails.FluentValidation;

/// <summary>
/// Extension methods for registering ProblemDetails.FluentValidation services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ProblemDetails.FluentValidation services to the DI container.
    /// Call this in Program.cs before building the app.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional delegate to configure response options.</param>
    public static IServiceCollection AddFluentValidationProblemDetails(
        this IServiceCollection services,
        Action<FluentValidationProblemDetailsOptions>? configure = null)
    {
        var options = new FluentValidationProblemDetailsOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.TryAddSingleton<IFluentValidationProblemDetailsMapper, DefaultFluentValidationProblemDetailsMapper>();

        return services;
    }
}
