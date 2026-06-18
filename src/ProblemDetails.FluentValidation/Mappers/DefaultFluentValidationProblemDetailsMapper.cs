using System.Diagnostics;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace ProblemDetails.FluentValidation;

/// <summary>
/// Default implementation of IFluentValidationProblemDetailsMapper.
/// Maps FluentValidation errors to RFC 9457 ProblemDetails with field-level errors,
/// trace ID, and request ID.
/// </summary>
public class DefaultFluentValidationProblemDetailsMapper : IFluentValidationProblemDetailsMapper
{
    public ProblemDetail Map(IEnumerable<ValidationFailure> failures, HttpContext httpContext, FluentValidationProblemDetailsOptions options)
    {
        var problem = new ValidationProblemDetail
        {
            Type = options.Type,
            Title = options.Title,
            Status = options.StatusCode,
            Detail = options.Detail,
            Instance = httpContext.Request.Path,
        };

        if (options.IncludeValidationErrors)
        {
            var errors = failures
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            problem.Errors = errors;
        }

        if (options.IncludeTraceId)
        {
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            problem.Extensions["traceId"] = traceId;
        }

        if (options.IncludeRequestId)
        {
            problem.Extensions["requestId"] = httpContext.TraceIdentifier;
        }

        options.OnProblemDetailsCreated?.Invoke(problem, httpContext, failures);

        return problem;
    }
}
