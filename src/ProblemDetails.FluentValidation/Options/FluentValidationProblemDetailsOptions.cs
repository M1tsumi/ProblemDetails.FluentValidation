using Microsoft.AspNetCore.Http;
using FluentValidation.Results;

namespace ProblemDetails.FluentValidation;

/// <summary>
/// Options for configuring the FluentValidation ProblemDetails middleware.
/// </summary>
public class FluentValidationProblemDetailsOptions
{
    /// <summary>
    /// HTTP status code returned for validation failures.
    /// Default is 422 (Unprocessable Entity).
    /// </summary>
    public int StatusCode { get; set; } = StatusCodes.Status422UnprocessableEntity;

    /// <summary>
    /// URI that identifies the problem type.
    /// Default points to the RFC for Unprocessable Entity.
    /// </summary>
    public string Type { get; set; } = "https://httpstatuses.io/422";

    /// <summary>
    /// Short human-readable summary of the problem type.
    /// </summary>
    public string Title { get; set; } = "Unprocessable Entity";

    /// <summary>
    /// Human-readable explanation of this specific error occurrence.
    /// </summary>
    public string Detail { get; set; } = "One or more validation errors occurred.";

    /// <summary>
    /// When true, adds the current TraceId (from Activity.Current) to the response extensions.
    /// </summary>
    public bool IncludeTraceId { get; set; } = true;

    /// <summary>
    /// When true, adds the HttpContext.TraceIdentifier to the response extensions.
    /// </summary>
    public bool IncludeRequestId { get; set; } = true;

    /// <summary>
    /// When true, field-level validation errors are included in the response.
    /// </summary>
    public bool IncludeValidationErrors { get; set; } = true;

    /// <summary>
    /// Replace the default mapper with a custom implementation.
    /// When set, this takes precedence over a mapper registered in DI.
    /// </summary>
    public IFluentValidationProblemDetailsMapper? Mapper { get; set; }

    /// <summary>
    /// Callback invoked after the ProblemDetails object is built.
    /// Use this to add custom enrichment like tenant ID, correlation ID, etc.
    /// </summary>
    public Action<ProblemDetail, HttpContext, IEnumerable<ValidationFailure>>? OnProblemDetailsCreated { get; set; }

    /// <summary>
    /// JSON serialization options used when writing the ProblemDetails response.
    /// When null (default), System.Text.Json's default settings are used.
    /// </summary>
    public System.Text.Json.JsonSerializerOptions? JsonSerializerOptions { get; set; }
}
