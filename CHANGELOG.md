# Changelog

All notable changes to `Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0-preview.0.1.0] - 2026-11-01

### Added

- `PureQLParametersConverters`: the `System.Text.Json` converters of the query parameter abstractions, the package's only public type. `IParameterType` is written as `{"name","nullable"}`, `IParameter` as `{"type","value"}` with the value as cell text, and `IListParameter` with its `items` as well. An object with `items` reads as a list parameter, whose `value` may be left out and is then built from the items. The converters do not use reflection and are AOT-compatible.

[Unreleased]: https://github.com/kudima03/Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization/compare/0.1.0-preview.0.1.0...HEAD
[0.1.0-preview.0.1.0]: https://github.com/kudima03/Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization/releases/tag/0.1.0-preview.0.1.0
