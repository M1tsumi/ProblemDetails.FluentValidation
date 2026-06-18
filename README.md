# ProblemDetails.FluentValidation

RFC 9457 (formerly RFC 7807) `application/problem+json` error responses for FluentValidation in ASP.NET Core.

## Why

When FluentValidation throws a `ValidationException`, you get a generic 500 error in production. This package converts that into a standard `application/problem+json` response so your API consumers get structured, machine-readable error details — the same format Microsoft's own `ProblemDetails` uses.

No more manual try-catch blocks or custom middleware to write.

## Install

```shell
dotnet add package ProblemDetails.FluentValidation
```

## Quick start

### Middleware (catches any thrown ValidationException)

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

When validation fails, you get:

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

### Endpoint filter (Minimal APIs)

```csharp
app.MapPost("/customers", (Customer customer) => { ... })
   .AddEndpointFilter<FluentValidationProblemDetailsFilter>();
```

Works the same way but gives you per-endpoint control instead of global middleware.

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
| `StatusCode` | 422 | HTTP status code for the response |
| `Type` | `https://httpstatuses.io/422` | URI for the problem type |
| `Title` | "Unprocessable Entity" | Short title |
| `Detail` | "One or more validation errors occurred." | Human-readable explanation |
| `IncludeTraceId` | `true` | Adds `traceId` from `Activity.Current` or `HttpContext.TraceIdentifier` |
| `IncludeRequestId` | `true` | Adds `requestId` from `HttpContext.TraceIdentifier` |
| `IncludeValidationErrors` | `true` | Groups field-level errors by property name under `.Errors` |
| `OnProblemDetailsCreated` | `null` | Callback for custom enrichment |

## Custom mapper

Implement `IFluentValidationProblemDetailsMapper` to completely replace the response shape:

```csharp
public class CustomMapper : IFluentValidationProblemDetailsMapper
{
    public ProblemDetails Map(
        IEnumerable<ValidationFailure> failures,
        HttpContext httpContext,
        FluentValidationProblemDetailsOptions options)
    {
        // Build whatever response shape you need
    }
}

builder.Services.AddSingleton<IFluentValidationProblemDetailsMapper, CustomMapper>();
```

## Requirements

- .NET 8.0 or later
- FluentValidation 11.x or 12.x

## License

MIT
