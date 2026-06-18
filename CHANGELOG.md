# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-06-18

### Added

- Middleware that catches FluentValidation `ValidationException` and returns RFC 9457 `application/problem+json` responses
- Endpoint filter for per-endpoint validation in Minimal APIs
- `DefaultFluentValidationProblemDetailsMapper` that groups field errors by property name
- Configurable options: status code, type URI, title, detail, trace ID, request ID, validation errors toggle
- `OnProblemDetailsCreated` callback for custom enrichment
- `IFluentValidationProblemDetailsMapper` for replacing the response shape
- Multi-target: `net8.0`, `net9.0`, `net10.0`
