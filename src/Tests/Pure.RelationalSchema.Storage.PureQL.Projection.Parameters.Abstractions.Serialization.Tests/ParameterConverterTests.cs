using System.Text.Json;
using Pure.Primitives.String;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.ListParameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Parameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization.Tests.Fakes;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.ListParameter;
using String = Pure.Primitives.String.String;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization.Tests;

public sealed record ParameterConverterTests
{
    // The default encoder writes a quote inside a JSON string as \u0022.
    private const string Quote = "\\u0022";

    private readonly JsonSerializerOptions _options = Options.WithConverters();

    [Fact]
    public void Writes()
    {
        IParameter parameter = new ParameterDouble("decimal", false, "9.99");

        Assert.Equal(
            /*lang=json,strict*/
            """{"type":{"name":"decimal","nullable":false},"value":"9.99"}""",
            JsonSerializer.Serialize(parameter, _options)
        );
    }

    [Fact]
    public void WritesTypedNull()
    {
        IParameter parameter = new ParameterDouble("date", true, "");

        Assert.Equal(
            /*lang=json,strict*/
            """{"type":{"name":"date","nullable":true},"value":""}""",
            JsonSerializer.Serialize(parameter, _options)
        );
    }

    [Fact]
    public void WritesListParameterWithItems()
    {
        IParameter parameter = new ListParameterDouble(
            "integerList",
            """["1","-42"]""",
            new ParameterDouble("integer", false, "1"),
            new ParameterDouble("integer", false, "-42")
        );

        Assert.Equal(
            $$"""{"type":{"name":"integerList","nullable":false},"value":"[{{Quote}}1{{Quote}},{{Quote}}-42{{Quote}}]","items":[{"type":{"name":"integer","nullable":false},"value":"1"},{"type":{"name":"integer","nullable":false},"value":"-42"}]}""",
            JsonSerializer.Serialize(parameter, _options)
        );
    }

    [Fact]
    public void WritesEmptyListParameter()
    {
        IParameter parameter = new ListParameterDouble("uuidList", "[]");

        Assert.Equal(
            /*lang=json,strict*/
            """{"type":{"name":"uuidList","nullable":false},"value":"[]","items":[]}""",
            JsonSerializer.Serialize(parameter, _options)
        );
    }

    [Fact]
    public void WritesParametersInACollection()
    {
        IParameter[] parameters =
        [
            new ParameterDouble("boolean", false, "True"),
            new ListParameterDouble("stringList", "[]"),
        ];

        Assert.Equal(
            /*lang=json,strict*/
            """[{"type":{"name":"boolean","nullable":false},"value":"True"},{"type":{"name":"stringList","nullable":false},"value":"[]","items":[]}]""",
            JsonSerializer.Serialize(parameters, _options)
        );
    }

    [Fact]
    public void Reads()
    {
        IParameter parameter = JsonSerializer.Deserialize<IParameter>(
            /*lang=json,strict*/
            """{"type":{"name":"decimal","nullable":false},"value":"9.99"}""",
            _options
        )!;

        Assert.IsNotType<IListParameter>(parameter, exactMatch: false);
        Assert.Equal("decimal", parameter.Type.Name.TextValue);
        Assert.False(parameter.Type.Nullable.BoolValue);
        Assert.Equal("9.99", parameter.Value.TextValue);
    }

    [Fact]
    public void ReadsTypedNullInAnyPropertyOrder()
    {
        IParameter parameter = JsonSerializer.Deserialize<IParameter>(
            /*lang=json,strict*/
            """{"value":"","type":{"nullable":true,"name":"string"}}""",
            _options
        )!;

        Assert.Equal("string", parameter.Type.Name.TextValue);
        Assert.True(parameter.Type.Nullable.BoolValue);
        Assert.Equal("", parameter.Value.TextValue);
    }

    [Fact]
    public void ReadsListParameterWithItems()
    {
        IListParameter parameter = Assert.IsType<IListParameter>(
            JsonSerializer.Deserialize<IParameter>(
                /*lang=json,strict*/
                """{"type":{"name":"stringList","nullable":false},"value":"given","items":[{"type":{"name":"string","nullable":false},"value":"paid"},{"value":"shipped","type":{"name":"string","nullable":false}}]}""",
                _options
            ),
            exactMatch: false
        );

        Assert.Equal("stringList", parameter.Type.Name.TextValue);
        Assert.False(parameter.Type.Nullable.BoolValue);
        Assert.Equal("given", parameter.Value.TextValue);
        Assert.Collection(
            parameter.Items,
            item =>
            {
                Assert.Equal("string", item.Type.Name.TextValue);
                Assert.False(item.Type.Nullable.BoolValue);
                Assert.Equal("paid", item.Value.TextValue);
            },
            item =>
            {
                Assert.Equal("string", item.Type.Name.TextValue);
                Assert.Equal("shipped", item.Value.TextValue);
            }
        );
    }

    [Theory]
    [InlineData( /*lang=json,strict*/
        """[]""",
        "[]"
    )]
    [InlineData( /*lang=json,strict*/
        """[{"type":{"name":"string","nullable":false},"value":"paid"},{"type":{"name":"string","nullable":false},"value":"shipped"}]""",
        """["paid","shipped"]"""
    )]
    [InlineData( /*lang=json,strict*/
        """[{"type":{"name":"string","nullable":false},"value":"a\"b\\c"},{"type":{"name":"string","nullable":false},"value":"café <&>"}]""",
        """["a\"b\\c","café <&>"]"""
    )]
    [InlineData( /*lang=json,strict*/
        """[{"type":{"name":"decimal","nullable":false},"value":"1"},{"type":{"name":"decimal","nullable":false},"value":"2.5"}]""",
        """["1","2.5"]"""
    )]
    public void BuildsTheValueOfAListParameterReadWithoutOne(string items, string value)
    {
        IParameter parameter = JsonSerializer.Deserialize<IParameter>(
            $$"""{"type":{"name":"stringList","nullable":false},"items":{{items}}}""",
            _options
        )!;

        _ = Assert.IsType<IListParameter>(parameter, exactMatch: false);
        Assert.Equal(value, parameter.Value.TextValue);
    }

    [Fact]
    public void BuildsTheValueAsTheListParametersOfTheImplementationsDo()
    {
        IListParameter list = new StringListParameter([
            new String("a\"b\\c"),
            new String("café <&>"),
            new String("tab\t"),
            new EmptyString(),
        ]);

        IParameter read = JsonSerializer.Deserialize<IParameter>(
            $$"""{"type":{"name":"stringList","nullable":false},"items":{{JsonSerializer.Serialize(list.Items, _options)}}}""",
            _options
        )!;

        Assert.Equal(list.Value.TextValue, read.Value.TextValue);
    }

    [Fact]
    public void RoundTripsAListParameterOfTheImplementations()
    {
        IListParameter list = new StringListParameter([
            new String("paid"),
            new String("shipped"),
        ]);

        IListParameter read = Assert.IsType<IListParameter>(
            JsonSerializer.Deserialize<IParameter>(
                JsonSerializer.Serialize<IParameter>(list, _options),
                _options
            ),
            exactMatch: false
        );

        Assert.Equal("stringList", read.Type.Name.TextValue);
        Assert.False(read.Type.Nullable.BoolValue);
        Assert.Equal("""["paid","shipped"]""", read.Value.TextValue);
        Assert.Equal(
            ["paid", "shipped"],
            read.Items.Select(item => item.Value.TextValue)
        );
        Assert.All(read.Items, item => Assert.Equal("string", item.Type.Name.TextValue));
    }

    [Fact]
    public void ReadsParametersInACollection()
    {
        IParameter[] parameters = JsonSerializer.Deserialize<IParameter[]>(
            /*lang=json,strict*/
            """[{"type":{"name":"boolean","nullable":false},"value":"True"},{"type":{"name":"stringList","nullable":false},"items":[]}]""",
            _options
        )!;

        Assert.Equal("True", parameters[0].Value.TextValue);
        Assert.Empty(
            Assert.IsType<IListParameter>(parameters[1], exactMatch: false).Items
        );
    }

    [Fact]
    public void SkipsUnknownProperties()
    {
        IParameter parameter = JsonSerializer.Deserialize<IParameter>(
            /*lang=json,strict*/
            """{"name":"price","type":{"name":"integer","nullable":false},"tags":[{"value":"x"}],"value":"-42","meta":{"items":[]},"note":null}""",
            _options
        )!;

        Assert.IsNotType<IListParameter>(parameter, exactMatch: false);
        Assert.Equal("integer", parameter.Type.Name.TextValue);
        Assert.Equal("-42", parameter.Value.TextValue);
    }

    [Fact]
    public void RoundTripsSpecialCharacters()
    {
        IParameter parameter = new ParameterDouble("string", false, "\"<é>\"\n\\");

        IParameter read = JsonSerializer.Deserialize<IParameter>(
            JsonSerializer.Serialize(parameter, _options),
            _options
        )!;

        Assert.Equal("\"<é>\"\n\\", read.Value.TextValue);
    }

    [Theory]
    [InlineData( /*lang=json,strict*/
        "\"9.99\"",
        "Expected a JSON object for the PureQL parameter."
    )]
    [InlineData( /*lang=json,strict*/
        "[]",
        "Expected a JSON object for the PureQL parameter."
    )]
    [InlineData( /*lang=json,strict*/
        """{"value":"9.99"}""",
        "The PureQL parameter is missing the required property 'type'."
    )]
    [InlineData( /*lang=json,strict*/
        """{"items":[]}""",
        "The PureQL parameter is missing the required property 'type'."
    )]
    [InlineData( /*lang=json,strict*/
        """{"type":{"name":"decimal","nullable":false}}""",
        "The PureQL parameter is missing the required property 'value'."
    )]
    [InlineData( /*lang=json,strict*/
        """{"type":{"name":"decimal","nullable":false},"value":9.99}""",
        "Expected a JSON string for the property 'value' of the PureQL parameter."
    )]
    [InlineData( /*lang=json,strict*/
        """{"type":{"name":"decimal","nullable":false},"value":null}""",
        "Expected a JSON string for the property 'value' of the PureQL parameter."
    )]
    [InlineData( /*lang=json,strict*/
        """{"type":"decimal","value":"9.99"}""",
        "Expected a JSON object for the PureQL parameter type."
    )]
    [InlineData( /*lang=json,strict*/
        """{"type":null,"value":"9.99"}""",
        "Expected a JSON object for the PureQL parameter type."
    )]
    [InlineData( /*lang=json,strict*/
        """{"type":{"name":"decimal"},"value":"9.99"}""",
        "The PureQL parameter type is missing the required property 'nullable'."
    )]
    [InlineData( /*lang=json,strict*/
        """{"type":{"name":"stringList","nullable":false},"items":{}}""",
        "Expected a JSON array for the property 'items' of the PureQL parameter."
    )]
    [InlineData( /*lang=json,strict*/
        """{"type":{"name":"stringList","nullable":false},"items":null}""",
        "Expected a JSON array for the property 'items' of the PureQL parameter."
    )]
    [InlineData( /*lang=json,strict*/
        """{"type":{"name":"stringList","nullable":false},"items":["paid"]}""",
        "Expected a JSON object for the PureQL parameter."
    )]
    [InlineData( /*lang=json,strict*/
        """{"type":{"name":"stringList","nullable":false},"items":[null]}""",
        "Expected a JSON object for the PureQL parameter."
    )]
    [InlineData( /*lang=json,strict*/
        """{"type":{"name":"stringList","nullable":false},"items":[{"type":{"name":"string","nullable":false}}]}""",
        "The PureQL parameter is missing the required property 'value'."
    )]
    public void ThrowsOnInvalidJson(string json, string message)
    {
        JsonException exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<IParameter>(json, _options)
        );

        Assert.Equal(message, exception.Message);
    }
}
