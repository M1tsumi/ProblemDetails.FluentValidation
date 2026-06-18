using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ProblemDetails.FluentValidation;

namespace ProblemDetails.FluentValidation.Tests;

public class IntegrationTests
{
    [Fact]
    public async Task Middleware_catches_validation_exception_and_sets_422()
    {
        var options = new FluentValidationProblemDetailsOptions();
        var mapper = new DefaultFluentValidationProblemDetailsMapper();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Request.Path = "/test";
        httpContext.TraceIdentifier = "test-trace";

        var middleware = new FluentValidationProblemDetailsMiddleware(
            _ => throw new ValidationException("Test failure")
        );

        await middleware.InvokeAsync(httpContext);

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, httpContext.Response.StatusCode);
        Assert.Equal("application/problem+json", httpContext.Response.ContentType);
    }

    [Fact]
    public async Task Middleware_skips_non_validation_exceptions()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var middleware = new FluentValidationProblemDetailsMiddleware(
            _ => throw new InvalidOperationException("Boom")
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            middleware.InvokeAsync(httpContext));
    }

    [Fact]
    public async Task Middleware_passthrough_on_success()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var invoked = false;

        var middleware = new FluentValidationProblemDetailsMiddleware(ctx =>
        {
            invoked = true;
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(httpContext);

        Assert.True(invoked);
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Filter_returns_422_on_validation_failure()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var services = new ServiceCollection();
        services.AddFluentValidationProblemDetails();
        services.AddScoped<IValidator<TestRequest>, TestRequestValidator>();
        var serviceProvider = services.BuildServiceProvider();
        httpContext.RequestServices = serviceProvider;

        // Build minimal API filter context
        var endpointMetadata = new EndpointMetadataCollection();
        var endpoint = new Endpoint(
            _ => Task.FromResult<object?>("ok"),
            endpointMetadata,
            "test");
        httpContext.SetEndpoint(endpoint);

        var filter = new FluentValidationProblemDetailsFilter();

        // Simulate an endpoint invocation with a TestRequest argument
        var filterContext = new DefaultEndpointFilterInvocationContext(httpContext, new TestRequest { Name = "" });

        var result = await filter.InvokeAsync(filterContext, _ => new ValueTask<object?>("ok"));

        Assert.Null(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Filter_passthrough_on_valid_request()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var services = new ServiceCollection();
        services.AddFluentValidationProblemDetails();
        services.AddScoped<IValidator<TestRequest>, TestRequestValidator>();
        var serviceProvider = services.BuildServiceProvider();
        httpContext.RequestServices = serviceProvider;

        var filter = new FluentValidationProblemDetailsFilter();
        var filterContext = new DefaultEndpointFilterInvocationContext(httpContext, new TestRequest { Name = "Alice" });

        var result = await filter.InvokeAsync(filterContext, _ => new ValueTask<object?>("ok"));

        Assert.Equal("ok", result);
    }
}

public class TestRequest
{
    public string Name { get; set; } = "";
}

public class TestRequestValidator : AbstractValidator<TestRequest>
{
    public TestRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}
