using System.Text.Json;
using System.Text.Json.Serialization;
using Pure.Primitives.Abstractions.String;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.ListParameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Parameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.ParameterType;
using String = Pure.Primitives.String.String;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization;

internal sealed record ParameterJsonModel : IParameter
{
    public ParameterJsonModel(IParameterType type, IString value)
    {
        Type = type;
        Value = value;
    }

    public IParameterType Type { get; }

    public IString Value { get; }
}

internal sealed record ListParameterJsonModel : IListParameter
{
    public ListParameterJsonModel(
        IParameterType type,
        IEnumerable<IParameter> items,
        IString value
    )
    {
        Type = type;
        Items = items;
        Value = value;
    }

    public IParameterType Type { get; }

    public IEnumerable<IParameter> Items { get; }

    public IString Value { get; }
}

/// <summary>
/// Converts <see cref="IParameter"/> to and from <c>{"type", "value"}</c>, such as
/// <c>{"type":{"name":"decimal","nullable":false},"value":"9.99"}</c>. An
/// <see cref="IListParameter"/> is written with its <c>items</c>, each a parameter, and a
/// parameter with <c>items</c> reads as an <see cref="IListParameter"/>, whose
/// <c>value</c> may then be left out: it is built from the items' values. The values are
/// cell text, read and written as they are, without parsing or validation.
/// </summary>
internal sealed class ParameterConverter : JsonConverter<IParameter>
{
    private const string Element = "PureQL parameter";

    private readonly ParameterTypeConverter _types = new ParameterTypeConverter();

    public override IParameter Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        reader.ExpectObject(Element);

        IParameterType? type = null;
        string? value = null;
        List<IParameter>? items = null;

        while (reader.NextProperty(out string property))
        {
            switch (property)
            {
                case "type":
                    type = _types.Read(ref reader, typeof(IParameterType), options);
                    break;
                case "value":
                    value = reader.ExpectString($"property 'value' of the {Element}");
                    break;
                case "items":
                    reader.ExpectArray($"property 'items' of the {Element}");
                    items = [];

                    while (reader.NextItem())
                    {
                        items.Add(Read(ref reader, typeof(IParameter), options));
                    }

                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        IParameterType parameterType =
            type ?? throw Utf8JsonReaderExtensions.MissingProperty("type", Element);

        return items is null
            ? new ParameterJsonModel(
                parameterType,
                new String(
                    value
                        ?? throw Utf8JsonReaderExtensions.MissingProperty(
                            "value",
                            Element
                        )
                )
            )
            : new ListParameterJsonModel(
                parameterType,
                items,
                new String(value ?? ListText.Of(items))
            );
    }

    public override void Write(
        Utf8JsonWriter writer,
        IParameter value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();
        writer.WritePropertyName("type");
        _types.Write(writer, value.Type, options);
        writer.WriteString("value", value.Value.TextValue);

        if (value is IListParameter list)
        {
            writer.WriteStartArray("items");

            foreach (IParameter item in list.Items)
            {
                Write(writer, item, options);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
}
