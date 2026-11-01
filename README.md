# Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization

AOT-compatible `System.Text.Json` converters for the query parameter abstractions of `Pure.RelationalSchema.Storage.PureQL.Projection`: `IParameterType`, `IParameter` and `IListParameter`.

[![.NET build & test](https://github.com/kudima03/Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization/actions/workflows/build-and-test.yml/badge.svg?branch=main)](https://github.com/kudima03/Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization/actions/workflows/build-and-test.yml)
[![Build and Deploy](https://github.com/kudima03/Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization/actions/workflows/publish-nuget.yml/badge.svg?branch=main)](https://github.com/kudima03/Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization/actions/workflows/publish-nuget.yml)
[![NuGet](https://img.shields.io/nuget/v/Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization)](https://www.nuget.org/packages/Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Overview

[`Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions`](https://github.com/kudima03/Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions) describes a value bound to a query parameter of `Pure.RelationalSchema.Storage.PureQL.Projection`: its PureQL type and its value as invariant cell text, or, for a list parameter, its list type and its items. Those parameters are meant to be stored or sent as data and turned into PureQL literals when the query runs.

This package makes them JSON. It converts the interfaces, not concrete types, so any implementation, such as those of [`Pure.RelationalSchema.Storage.PureQL.Projection.Parameters`](https://github.com/kudima03/Pure.RelationalSchema.Storage.PureQL.Projection.Parameters), is written the same way, and reading gives back an implementation of the interface. The converters read and write JSON directly with `Utf8JsonReader` and `Utf8JsonWriter`, without reflection, so they work under trimming and native AOT.

## Installation

```
dotnet add package Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization
```

## Public API

| Name | Namespace | Description |
|------|-----------|-------------|
| `PureQLParametersConverters` | `Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization` | `IEnumerable<JsonConverter>`: the converters of `IParameterType`, `IParameter` and `IListParameter`, to add to `JsonSerializerOptions.Converters` |

The converters themselves are internal: `PureQLParametersConverters` is the only public type.

## JSON format

Property names are fixed and camel case, whatever the `PropertyNamingPolicy`.

### Parameter type

`IParameterType` is an object with its `name` and whether it is `nullable`:

```json
{ "name": "decimal", "nullable": false }
```

### Parameter

`IParameter` is an object with its `type` and its `value`, the cell text as a JSON string:

```json
{ "type": { "name": "decimal", "nullable": false }, "value": "9.99" }
```

A typed null has the empty string as its value:

```json
{ "type": { "name": "date", "nullable": true }, "value": "" }
```

### List parameter

`IListParameter` adds its `items`, each a parameter of the item type:

```json
{
  "type": { "name": "stringList", "nullable": false },
  "value": "[\"paid\",\"shipped\"]",
  "items": [
    { "type": { "name": "string", "nullable": false }, "value": "paid" },
    { "type": { "name": "string", "nullable": false }, "value": "shipped" }
  ]
}
```

The default encoder of `System.Text.Json` writes the quotes inside `value` as `"`; that is the same JSON string.

### Reading

- `type`, `name` and `nullable` are required, and `value` is required for a parameter. A missing property, or a value of the wrong JSON type, `null` included, is a `JsonException`.
- An object with `items` reads as an `IListParameter`, even through the `IParameter` converter, so a list of mixed parameters round-trips. Through the `IListParameter` converter, `items` is required.
- The `value` of a list parameter may be left out: it is informational, so it is then built from the items' values, as the list parameters of `Pure.RelationalSchema.Storage.PureQL.Projection.Parameters` build it (`["paid","shipped"]`, and `[]` for an empty list).
- Properties may come in any order. Unknown properties are skipped; when a property repeats, the last one wins.
- Names and values are read and written as they are, without parsing or validation: checking a type name or a value's cell text, and turning a parameter into a PureQL literal, belong to the implementations.

## Usage

Add the converters to the options:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization;

JsonSerializerOptions options = new JsonSerializerOptions();

foreach (JsonConverter converter in new PureQLParametersConverters())
{
    options.Converters.Add(converter);
}
```

Then serialize parameters as their interface, not their concrete type, so that the converters are picked:

```csharp
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Parameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.ListParameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Parameter;
using String = Pure.Primitives.String.String;

IParameter[] parameters =
[
    new StringParameter(new String("Ada")),
    new NullableDateParameter(),
    new StringListParameter([new String("paid"), new String("shipped")]),
];

string json = JsonSerializer.Serialize(parameters, options);
IParameter[] read = JsonSerializer.Deserialize<IParameter[]>(json, options)!;
```

A parameter is written with the value it has when it is written: a relative one, such as `CurrentDateParameter`, reads back as a fixed date.

### Native AOT

Under native AOT, give the options a source-generated context for the types you serialize, and serialize through its `JsonTypeInfo`. The converters in `Converters` take precedence over the generated metadata:

```csharp
using System.Text.Json.Serialization.Metadata;

JsonSerializerOptions options = new JsonSerializerOptions
{
    TypeInfoResolver = ParametersJsonContext.Default,
};

foreach (JsonConverter converter in new PureQLParametersConverters())
{
    options.Converters.Add(converter);
}

JsonTypeInfo<IParameter[]> info =
    (JsonTypeInfo<IParameter[]>)options.GetTypeInfo(typeof(IParameter[]));

string json = JsonSerializer.Serialize(parameters, info);
IParameter[] read = JsonSerializer.Deserialize(json, info)!;

[JsonSerializable(typeof(IParameter[]))]
internal sealed partial class ParametersJsonContext : JsonSerializerContext;
```

## Design Principles

- **Interface-preserving** — the converters target the abstractions, so any implementation serializes the same way
- **Mirrors the abstractions** — one JSON property per interface property, values kept as cell text
- **No validation** — names and values are carried as they are; the implementations parse and check them
- **AOT-compatible** — `IsAotCompatible=true`, no reflection
- **One public type** — `PureQLParametersConverters`; the converters are internal

## Dependencies

- [`Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions`](https://github.com/kudima03/Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions) — `IParameterType`, `IParameter` and `IListParameter`
- [`Pure.Primitives`](https://github.com/kudima03/Pure.Primitives) — the `String`, `True` and `False` values a read parameter holds

## Target Frameworks

- .NET 8
- .NET 9
- .NET 10

## Related packages

- `Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions` — the interfaces converted here
- `Pure.RelationalSchema.Storage.PureQL.Projection.Parameters` — their implementations, one per PureQL literal and list literal
- `Pure.RelationalSchema.Storage.PureQL.Projection` — runs PureQL queries over stored relational data and binds their parameters
