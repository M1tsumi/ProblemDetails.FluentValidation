using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ProblemDetails.FluentValidation;

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
            var options = ResolveOptions(context);
            var mapper = ResolveMapper(context, options);
            var problem = mapper.Map(ex.Errors, context, options);

            context.Response.StatusCode = problem.Status ?? StatusCodes.Status422UnprocessableEntity;
            await context.Response.WriteAsJsonAsync(problem, options.JsonSerializerOptions);
            context.Response.ContentType = "application/problem+json";
        }
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
