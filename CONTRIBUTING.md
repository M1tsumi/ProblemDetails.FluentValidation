# Contributing

Pull requests are welcome.

## Prerequisites

- .NET 8.0 SDK or later

## Building

```shell
dotnet build
```

## Testing

```shell
dotnet test
```

## Releasing

1. Update `CHANGELOG.md` with the new version.
2. Update the `Version` property in `src/ProblemDetails.FluentValidation/ProblemDetails.FluentValidation.csproj`.
3. Tag the commit with the version and push:

```shell
git tag v1.0.0
git push origin v1.0.0
```

The publish workflow will build, pack, and push to NuGet.org.
