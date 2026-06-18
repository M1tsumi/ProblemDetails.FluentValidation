using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace ProblemDetails.FluentValidation;

/// <summary>
/// Maps FluentValidation validation failures to a standard ProblemDetails response.
/// </summary>
public interface IFluentValidationProblemDetailsMapper
{
    /// <summary>
    /// Converts FluentValidation validation failures into a ProblemDetails object.
    /// </summary>
    /// <param name="failures">The validation failures from FluentValidation.</param>
    /// <param name="httpContext">The current HTTP context for extracting request metadata.</param>
    /// <param name="options">Configuration options controlling the shape of the response.</param>
    /// <returns>A ProblemDetails instance following the RFC 9457 specification.</returns>
    ProblemDetail Map(IEnumerable<ValidationFailure> failures, HttpContext httpContext, FluentValidationProblemDetailsOptions options);
}
