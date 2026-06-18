using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ProblemDetails.FluentValidation;

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
                var options = ResolveOptions(httpContext);
                var mapper = ResolveMapper(httpContext, options);
                var problem = mapper.Map(result.Errors, httpContext, options);

                httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status422UnprocessableEntity;
                await httpContext.Response.WriteAsJsonAsync(problem, options.JsonSerializerOptions);
                httpContext.Response.ContentType = "application/problem+json";
                return null;
            }
        }

        return await next(context);
    }

    private static FluentValidationProblemDetailsOptions ResolveOptions(HttpContext context)
    {
        if (context.RequestServices is not null)
        {
            var options = context.RequestServices.GetService(typeof(FluentValidationProblemDetailsOptions)) as FluentValidationProblemDetailsOptions;
            if (options is not null)
                return options;

            var ioptions = context.RequestServices.GetService(typeof(Microsoft.Extensions.Options.IOptions<FluentValidationProblemDetailsOptions>)) as Microsoft.Extensions.Options.IOptions<FluentValidationProblemDetailsOptions>;
            if (ioptions is not null)
                return ioptions.Value;
        }

        return new FluentValidationProblemDetailsOptions();
    }

    private static IFluentValidationProblemDetailsMapper ResolveMapper(HttpContext context, FluentValidationProblemDetailsOptions options)
    {
        if (options.Mapper is not null)
            return options.Mapper;

        if (context.RequestServices?.GetService(typeof(IFluentValidationProblemDetailsMapper)) is IFluentValidationProblemDetailsMapper mapper)
            return mapper;

        return new DefaultFluentValidationProblemDetailsMapper();
    }
}
