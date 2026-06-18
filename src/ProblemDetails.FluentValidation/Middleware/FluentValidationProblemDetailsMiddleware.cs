using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ProblemDetails.FluentValidation;

/// <summary>
/// Middleware that catches FluentValidation ValidationException and
/// converts it to a standard RFC 9457 ProblemDetails response.
/// </summary>
public class FluentValidationProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;

    public FluentValidationProblemDetailsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            FluentValidationProblemDetailsOptions? options = null;
            
            if (context.RequestServices is not null)
            {
                options = context.RequestServices
                    .GetService(typeof(FluentValidationProblemDetailsOptions))
                    as FluentValidationProblemDetailsOptions;
            }

            options ??= new FluentValidationProblemDetailsOptions();

            var mapper = options.Mapper ?? new DefaultFluentValidationProblemDetailsMapper();

            var problem = mapper.Map(ex.Errors, context, options);

            context.Response.StatusCode = problem.Status ?? StatusCodes.Status422UnprocessableEntity;

            await context.Response.WriteAsJsonAsync(problem, (System.Text.Json.JsonSerializerOptions?)null);
            context.Response.ContentType = "application/problem+json";
        }
    }
}
