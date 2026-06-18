using FluentValidation;
using ProblemDetails.FluentValidation;
using SampleApi.Models;
using SampleApi.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFluentValidationProblemDetails(options =>
{
    options.IncludeTraceId = true;
    options.IncludeRequestId = true;
});

builder.Services.AddScoped<IValidator<Customer>, CustomerValidator>();

var app = builder.Build();

app.UseFluentValidationProblemDetails();

app.MapPost("/customers", (Customer customer) =>
{
    return Results.Ok(customer);
});

app.MapGet("/throw", () =>
{
    throw new ValidationException("Something went wrong during processing");
});

app.Run();
