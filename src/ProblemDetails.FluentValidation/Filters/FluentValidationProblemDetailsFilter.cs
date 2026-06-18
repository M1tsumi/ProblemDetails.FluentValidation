using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ProblemDetails.FluentValidation;

/// <summary>
/// Minimal API endpoint filter that validates the request using FluentValidation
/// and returns a standard RFC 9457 ProblemDetails response on failure.
/// </summary>
public class FluentValidationProblemDetailsFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        foreach (var argument in context.Arguments)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            var validator = httpContext.RequestServices.GetService(validatorType) as IValidator;

            if (validator is null)
                continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                FluentValidationProblemDetailsOptions? options = null;

                if (httpContext.RequestServices is not null)
                {
                    options = httpContext.RequestServices
                        .GetService(typeof(FluentValidationProblemDetailsOptions))
                        as FluentValidationProblemDetailsOptions;
                }

                options ??= new FluentValidationProblemDetailsOptions();

                var mapper = options.Mapper ?? new DefaultFluentValidationProblemDetailsMapper();
                var problem = mapper.Map(result.Errors, httpContext, options);

                httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status422UnprocessableEntity;

                await httpContext.Response.WriteAsJsonAsync(problem, (System.Text.Json.JsonSerializerOptions?)null);
                httpContext.Response.ContentType = "application/problem+json";
                return null;
            }
        }

        return await next(context);
    }
}
