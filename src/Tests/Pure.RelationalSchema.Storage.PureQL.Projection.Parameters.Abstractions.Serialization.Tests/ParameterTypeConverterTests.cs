using System.Text.Json;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.ParameterType;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization.Tests.Fakes;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization.Tests;

public sealed record ParameterTypeConverterTests
{
    private readonly JsonSerializerOptions _options = Options.WithConverters();

    [Theory]
    [InlineData(
        "decimal",
        false, /*lang=json,strict*/
        """{"name":"decimal","nullable":false}"""
    )]
    [InlineData(
        "date",
        true, /*lang=json,strict*/
        """{"name":"date","nullable":true}"""
    )]
    [InlineData(
        "stringList",
        false, /*lang=json,strict*/
        """{"name":"stringList","nullable":false}"""
    )]
    public void Writes(string name, bool nullable, string json)
    {
        IParameterType type = new ParameterTypeDouble(name, nullable);

        Assert.Equal(json, JsonSerializer.Serialize(type, _options));
    }

    [Theory]
    [InlineData( /*lang=json,strict*/
        """{"name":"decimal","nullable":false}""",
        "decimal",
        false
    )]
    [InlineData( /*lang=json,strict*/
        """{"nullable":true,"name":"date"}""",
        "date",
        true
    )]
    [InlineData( /*lang=json,strict*/
        """{"name":"","nullable":false}""",
        "",
        false
    )]
    public void Reads(string json, string name, bool nullable)
    {
        IParameterType type = JsonSerializer.Deserialize<IParameterType>(json, _options)!;

        Assert.Equal(name, type.Name.TextValue);
        Assert.Equal(nullable, type.Nullable.BoolValue);
    }

    [Fact]
    public void SkipsUnknownProperties()
    {
        IParameterType type = JsonSerializer.Deserialize<IParameterType>(
            /*lang=json,strict*/
            """{"kind":{"name":"x","nullable":true},"name":"uuid","list":[1,{"a":[]}],"nullable":true,"note":null}""",
            _options
        )!;

        Assert.Equal("uuid", type.Name.TextValue);
        Assert.True(type.Nullable.BoolValue);
    }

    [Fact]
    public void ReadsTheLastOfRepeatedProperties()
    {
        IParameterType type = JsonSerializer.Deserialize<IParameterType>(
            """{"name":"integer","nullable":true,"name":"time","nullable":false}""",
            _options
        )!;

        Assert.Equal("time", type.Name.TextValue);
        Assert.False(type.Nullable.BoolValue);
    }

    [Fact]
    public void RoundTrips()
    {
        IParameterType type = new ParameterTypeDouble("datetime", true);

        IParameterType read = JsonSerializer.Deserialize<IParameterType>(
            JsonSerializer.Serialize(type, _options),
            _options
        )!;

        Assert.Equal("datetime", read.Name.TextValue);
        Assert.True(read.Nullable.BoolValue);
    }

    [Theory]
    [InlineData( /*lang=json,strict*/
        "\"decimal\"",
        "Expected a JSON object for the PureQL parameter type."
    )]
    [InlineData( /*lang=json,strict*/
        "[]",
        "Expected a JSON object for the PureQL parameter type."
    )]
    [InlineData( /*lang=json,strict*/
        """{"nullable":false}""",
        "The PureQL parameter type is missing the required property 'name'."
    )]
    [InlineData( /*lang=json,strict*/
        """{"name":"decimal"}""",
        "The PureQL parameter type is missing the required property 'nullable'."
    )]
    [InlineData( /*lang=json,strict*/
        "{}",
        "The PureQL parameter type is missing the required property 'name'."
    )]
    [InlineData( /*lang=json,strict*/
        """{"name":1,"nullable":false}""",
        "Expected a JSON string for the property 'name' of the PureQL parameter type."
    )]
    [InlineData( /*lang=json,strict*/
        """{"name":null,"nullable":false}""",
        "Expected a JSON string for the property 'name' of the PureQL parameter type."
    )]
    [InlineData( /*lang=json,strict*/
        """{"name":"decimal","nullable":"false"}""",
        "Expected a JSON boolean for the property 'nullable' of the PureQL parameter type."
    )]
    [InlineData( /*lang=json,strict*/
        """{"name":"decimal","nullable":null}""",
        "Expected a JSON boolean for the property 'nullable' of the PureQL parameter type."
    )]
    [InlineData( /*lang=json,strict*/
        """{"name":"decimal","nullable":0}""",
        "Expected a JSON boolean for the property 'nullable' of the PureQL parameter type."
    )]
    public void ThrowsOnInvalidJson(string json, string message)
    {
        JsonException exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<IParameterType>(json, _options)
        );

        Assert.Equal(message, exception.Message);
    }
}
