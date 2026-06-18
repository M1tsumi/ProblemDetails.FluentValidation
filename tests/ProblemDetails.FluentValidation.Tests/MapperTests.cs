using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using ProblemDetails.FluentValidation;

namespace ProblemDetails.FluentValidation.Tests;

public class MapperTests
{
    private readonly DefaultFluentValidationProblemDetailsMapper _mapper = new();
    private readonly FluentValidationProblemDetailsOptions _options = new();

    private static HttpContext CreateHttpContext(string path = "/api/test")
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = path;
        ctx.TraceIdentifier = "test-request-id";
        return ctx;
    }

    [Fact]
    public void Map_sets_basic_properties()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "'Email' is not a valid email address.")
        };

        var httpContext = CreateHttpContext();
        var result = _mapper.Map(failures, httpContext, _options);

        Assert.Equal(422, result.Status);
        Assert.Equal("Unprocessable Entity", result.Title);
        Assert.Equal("https://httpstatuses.io/422", result.Type);
        Assert.Equal("One or more validation errors occurred.", result.Detail);
        Assert.Equal("/api/test", result.Instance);
    }

    [Fact]
    public void Map_groups_errors_by_property_name()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "'Email' is required."),
            new("Email", "'Email' is not a valid email address."),
            new("Name", "'Name' must not be empty."),
        };

        var httpContext = CreateHttpContext();
        var result = (ValidationProblemDetails)_mapper.Map(failures, httpContext, _options);

        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(2, result.Errors["Email"].Length);
        Assert.Equal(1, result.Errors["Name"].Length);
    }

    [Fact]
    public void Map_includes_trace_id()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "Invalid email.")
        };

        var httpContext = CreateHttpContext();
        httpContext.TraceIdentifier = "my-trace-123";

        var result = _mapper.Map(failures, httpContext, _options);

        Assert.True(result.Extensions.ContainsKey("traceId"));
        Assert.Equal("my-trace-123", result.Extensions["traceId"]);
    }

    [Fact]
    public void Map_includes_request_id()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "Invalid email.")
        };

        var httpContext = CreateHttpContext();
        httpContext.TraceIdentifier = "req-456";

        var result = _mapper.Map(failures, httpContext, _options);

        Assert.True(result.Extensions.ContainsKey("requestId"));
        Assert.Equal("req-456", result.Extensions["requestId"]);
    }

    [Fact]
    public void Map_respects_include_validation_errors_false()
    {
        var options = new FluentValidationProblemDetailsOptions
        {
            IncludeValidationErrors = false
        };

        var failures = new List<ValidationFailure>
        {
            new("Email", "Invalid email.")
        };

        var httpContext = CreateHttpContext();
        var result = (ValidationProblemDetails)_mapper.Map(failures, httpContext, options);

        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Map_invokes_callback()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "Invalid.")
        };

        var tenantId = "";
        var options = new FluentValidationProblemDetailsOptions
        {
            OnProblemDetailsCreated = (problem, ctx, _) =>
            {
                problem.Extensions["tenantId"] = "tenant-007";
                tenantId = "called";
            }
        };

        var httpContext = CreateHttpContext();
        var result = _mapper.Map(failures, httpContext, options);

        Assert.Equal("tenant-007", result.Extensions["tenantId"]);
        Assert.Equal("called", tenantId);
    }

    [Fact]
    public void Map_handles_empty_failures()
    {
        var failures = new List<ValidationFailure>();
        var httpContext = CreateHttpContext();

        var result = _mapper.Map(failures, httpContext, _options);

        Assert.NotNull(result);
        Assert.Equal(422, result.Status);
    }
}
