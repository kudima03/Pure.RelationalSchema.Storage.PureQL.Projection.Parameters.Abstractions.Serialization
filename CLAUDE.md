# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All `dotnet` commands must be run from the `./src` directory.

```bash
dotnet restore
dotnet tool restore
dotnet build --no-restore -warnaserror
dotnet test --no-build
dotnet format --verify-no-changes             # check code style (CI enforces this)
dotnet csharpier check .                      # check formatting (CI enforces this)
dotnet format && dotnet csharpier format .    # auto-fix code style
dotnet stryker --mutation-level Complete --break-at 98
dotnet pack --configuration Release -p:Version=<version> --output .
```

CI additionally passes `-p:AssemblyVersion` (pinned to the major) and `-p:FileVersion`; see `.github/workflows/publish-nuget.yml`.

## Architecture

This is a **serialization library**: `System.Text.Json` converters for the interfaces of `Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions` (`IParameterType`, `IParameter`, `IListParameter`). It defines no abstractions and no parsing or validation of names and values: those belong to `Pure.RelationalSchema.Storage.PureQL.Projection.Parameters`.

**Public API:** exactly one public type, `PureQLParametersConverters`, a sealed record implementing `IEnumerable<JsonConverter>` that yields a new instance of each converter. Keep the converters and their JSON models `internal`.

**Converters:** each reads with `Utf8JsonReader` and writes with `Utf8JsonWriter`, and calls nested converters directly (never `JsonSerializer`), so there is no reflection:

- `ParameterTypeConverter : JsonConverter<IParameterType>` — `{"name","nullable"}`
- `ParameterConverter : JsonConverter<IParameter>` — `{"type","value"}`, plus `"items"` for an `IListParameter`; an object with `items` reads as an `IListParameter`, and its `value` may be omitted, in which case `ListText` builds it from the items (escaped as the implementations' `ListText` escapes it)
- `ListParameterConverter : JsonConverter<IListParameter>` — delegates to `ParameterConverter` and requires `items`

Read results are internal `*JsonModel` records over `Pure.Primitives` (`String`, `True`, `False`). Property names are fixed camel case. Unknown properties are skipped; missing required properties and wrong token types (`null` included) throw `JsonException` with the messages built in `Utf8JsonReaderExtensions`. Tests assert those messages, so a message change needs a test change.

**Multi-targeting:** net8.0, net9.0, net10.0. `IsAotCompatible` is `true`: trim and AOT analyzer warnings fail the build under `-warnaserror`.

**Documentation:** `GenerateDocumentationFile` is on, so public members need XML doc comments (CS1591).

**Package validation:** `EnablePackageValidation = true`, with `PackageValidationBaselineVersion` set to the last published version (bump it after each release); breaking API changes against it fail the build.

**Publishing:** triggered by pushing a semver tag matching `*.*.*`. The tag name becomes the `PackageVersion`. Packages are published to both GitHub Packages and NuGet.org.

**Tests:** xUnit project at `src/Tests/Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization.Tests/`, targeting net10.0. It exercises the converters only through `PureQLParametersConverters` and `JsonSerializer`, with doubles in `Fakes/` and, to check the built list value, the published `Pure.RelationalSchema.Storage.PureQL.Projection.Parameters`. CI enforces ≥98% line coverage (warning below 99%) and a 98% mutation score with dotnet-stryker.

## Code Style

Enforced via `.editorconfig`, `dotnet format --verify-no-changes` and `dotnet csharpier check .` in CI:

- No `var`: always use explicit types
- No expression-bodied methods or constructors. Properties, indexers, accessors and lambdas may be expression-bodied
- File-scoped namespace declarations
- `using` directives outside the namespace, System directives sorted first
- Private fields: `_camelCase`
- Max line length: 90 characters
- Braces on new lines (`csharp_new_line_before_open_brace = all`)
- In test JSON, do not write `"` inside raw string literals: `dotnet format` rewrites it. Interpolate a constant instead (see `Quote` in the tests)

## Commit Messages

Conventional Commits (`feat: …`, `fix: …`, `test: …`, `docs: …`, `chore: …`, `ci: …`, `build: …`), without a scope or body. Do not mention Claude or AI assistance in commit messages or pull requests.
