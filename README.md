# ProblemDetails.FluentValidation

[![NuGet](https://img.shields.io/nuget/v/ProblemDetails.FluentValidation)](https://www.nuget.org/packages/ProblemDetails.FluentValidation)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ProblemDetails.FluentValidation)](https://www.nuget.org/packages/ProblemDetails.FluentValidation)
[![CI](https://github.com/M1tsumi/ProblemDetails.FluentValidation/actions/workflows/ci.yml/badge.svg)](https://github.com/M1tsumi/ProblemDetails.FluentValidation/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/github/license/M1tsumi/ProblemDetails.FluentValidation)](https://opensource.org/licenses/MIT)

RFC 9457 (formerly RFC 7807) `application/problem+json` error responses for FluentValidation in ASP.NET Core.

## Why

When FluentValidation throws a `ValidationException`, you get a generic 500 error in production. This package converts that into a standard `application/problem+json` response so your API consumers get structured, machine-readable error details in the same format Microsoft's own `ProblemDetails` uses.

No manual try-catch blocks or custom middleware to write.

## Install

```shell
dotnet add package ProblemDetails.FluentValidation
```

## Quick start

### Middleware (catches any thrown ValidationException globally)

```csharp
using ProblemDetails.FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFluentValidationProblemDetails();
builder.Services.AddScoped<IValidator<Customer>, CustomerValidator>();

var app = builder.Build();

app.UseFluentValidationProblemDetails();

app.MapPost("/customers", (Customer customer) =>
{
    return Results.Ok(customer);
});

app.Run();
```

When validation fails, the API returns:

```json
{
  "type": "https://httpstatuses.io/422",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "instance": "/customers",
  "errors": {
    "Email": ["'Email' is not a valid email address."],
    "Name": ["'Name' must not be empty."]
  }
}
```

### Endpoint filter (per-endpoint control for Minimal APIs)

```csharp
app.MapPost("/customers", (Customer customer) => { ... })
   .AddEndpointFilter<FluentValidationProblemDetailsFilter>();
```

Same response shape as the middleware, but only activates on endpoints you choose. Useful when you want validation error handling alongside other endpoint filters.

## Configuration

```csharp
builder.Services.AddFluentValidationProblemDetails(options =>
{
    options.StatusCode = 422;
    options.Type = "https://httpstatuses.io/422";
    options.Title = "Unprocessable Entity";
    options.Detail = "One or more validation errors occurred.";
    options.IncludeTraceId = true;
    options.IncludeRequestId = true;
    options.IncludeValidationErrors = true;

    options.OnProblemDetailsCreated = (problem, httpContext, failures) =>
    {
        problem.Extensions["tenant"] = "acme-corp";
        problem.Extensions["requestId"] = httpContext.TraceIdentifier;
    };
});
```

| Option | Default | Description |
|---|---|---|
| `StatusCode` | `422` | HTTP status code returned for validation failures |
| `Type` | `https://httpstatuses.io/422` | URI that identifies the problem type |
| `Title` | `"Unprocessable Entity"` | Short human-readable summary |
| `Detail` | `"One or more validation errors occurred."` | Human-readable explanation |
| `IncludeTraceId` | `true` | Adds `traceId` from `Activity.Current` or `HttpContext.TraceIdentifier` |
| `IncludeRequestId` | `true` | Adds `requestId` from `HttpContext.TraceIdentifier` |
| `IncludeValidationErrors` | `true` | Groups field-level errors by property name under `errors` |
| `JsonSerializerOptions` | `null` | Custom `System.Text.Json` serialization options |
| `Mapper` | `null` | Replace the default mapper with a custom implementation |
| `OnProblemDetailsCreated` | `null` | Callback invoked after building the response for custom enrichment |

### Using IOptions

Options also work with the standard `IOptions<T>` pattern:

```csharp
builder.Services.Configure<FluentValidationProblemDetailsOptions>(config =>
{
    config.IncludeTraceId = false;
});
```

## Custom mapper

Replace the entire response shape by implementing `IFluentValidationProblemDetailsMapper`:

```csharp
public class CustomMapper : IFluentValidationProblemDetailsMapper
{
    public ProblemDetail Map(
        IEnumerable<ValidationFailure> failures,
        HttpContext httpContext,
        FluentValidationProblemDetailsOptions options)
    {
        // Return any ProblemDetails shape you need
    }
}

builder.Services.AddSingleton<IFluentValidationProblemDetailsMapper, CustomMapper>();
```

The mapper is resolved from DI first. If you set `options.Mapper` directly, that takes precedence.

## Requirements

- .NET 8.0 or later
- FluentValidation 11.x or 12.x

## License

MIT
