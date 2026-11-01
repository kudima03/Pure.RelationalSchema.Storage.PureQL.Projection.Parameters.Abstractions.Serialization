using System.Text.Json;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.ListParameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Parameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization.Tests.Fakes;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization.Tests;

public sealed record ListParameterConverterTests
{
    // The default encoder writes a quote inside a JSON string as \u0022.
    private const string Quote = "\\u0022";

    private readonly JsonSerializerOptions _options = Options.WithConverters();

    [Fact]
    public void Writes()
    {
        IListParameter parameter = new ListParameterDouble(
            "booleanList",
            """["True"]""",
            new ParameterDouble("boolean", false, "True")
        );

        Assert.Equal(
            $$"""{"type":{"name":"booleanList","nullable":false},"value":"[{{Quote}}True{{Quote}}]","items":[{"type":{"name":"boolean","nullable":false},"value":"True"}]}""",
            JsonSerializer.Serialize(parameter, _options)
        );
    }

    [Fact]
    public void Reads()
    {
        IListParameter parameter = JsonSerializer.Deserialize<IListParameter>(
            /*lang=json,strict*/
            """{"type":{"name":"dateList","nullable":false},"items":[{"type":{"name":"date","nullable":false},"value":"2026-10-29"}]}""",
            _options
        )!;

        Assert.Equal("dateList", parameter.Type.Name.TextValue);
        Assert.False(parameter.Type.Nullable.BoolValue);
        Assert.Equal("""["2026-10-29"]""", parameter.Value.TextValue);
        IParameter item = Assert.Single(parameter.Items);
        Assert.Equal("date", item.Type.Name.TextValue);
        Assert.Equal("2026-10-29", item.Value.TextValue);
    }

    [Fact]
    public void RoundTrips()
    {
        IListParameter parameter = new ListParameterDouble(
            "timeList",
            "informational",
            new ParameterDouble("time", false, "08:30:00"),
            new ParameterDouble("time", false, "08:30:00.25")
        );

        IListParameter read = JsonSerializer.Deserialize<IListParameter>(
            JsonSerializer.Serialize(parameter, _options),
            _options
        )!;

        Assert.Equal("timeList", read.Type.Name.TextValue);
        Assert.Equal("informational", read.Value.TextValue);
        Assert.Equal(
            ["08:30:00", "08:30:00.25"],
            read.Items.Select(item => item.Value.TextValue)
        );
    }

    [Theory]
    [InlineData( /*lang=json,strict*/
        """{"type":{"name":"stringList","nullable":false},"value":"[]"}""",
        "The PureQL list parameter is missing the required property 'items'."
    )]
    [InlineData( /*lang=json,strict*/
        "[]",
        "Expected a JSON object for the PureQL parameter."
    )]
    [InlineData( /*lang=json,strict*/
        """{"items":[]}""",
        "The PureQL parameter is missing the required property 'type'."
    )]
    public void ThrowsOnInvalidJson(string json, string message)
    {
        JsonException exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<IListParameter>(json, _options)
        );

        Assert.Equal(message, exception.Message);
    }
}
