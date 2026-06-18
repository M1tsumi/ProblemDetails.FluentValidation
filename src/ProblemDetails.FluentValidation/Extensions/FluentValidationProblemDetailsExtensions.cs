using Microsoft.AspNetCore.Builder;

namespace ProblemDetails.FluentValidation;

/// <summary>
/// Extension methods for adding ProblemDetails.FluentValidation middleware to the request pipeline.
/// </summary>
public static class FluentValidationProblemDetailsExtensions
{
    /// <summary>
    /// Adds middleware that catches FluentValidation ValidationException and
    /// returns standard RFC 9457 ProblemDetails responses.
    /// Place this after UseRouting and before UseEndpoints / MapControllers.
    /// </summary>
    public static IApplicationBuilder UseFluentValidationProblemDetails(this IApplicationBuilder app)
    {
        return app.UseMiddleware<FluentValidationProblemDetailsMiddleware>();
    }
}
