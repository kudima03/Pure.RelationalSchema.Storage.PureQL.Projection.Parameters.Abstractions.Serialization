using System.Text.Json;
using System.Text.Json.Serialization;
using Pure.Primitives.Abstractions.Bool;
using Pure.Primitives.Abstractions.String;
using Pure.Primitives.Bool;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.ParameterType;
using String = Pure.Primitives.String.String;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization;

internal sealed record ParameterTypeJsonModel : IParameterType
{
    public ParameterTypeJsonModel(IString name, IBool nullable)
    {
        Name = name;
        Nullable = nullable;
    }

    public IString Name { get; }

    public IBool Nullable { get; }
}

/// <summary>
/// Converts <see cref="IParameterType"/> to and from <c>{"name", "nullable"}</c>, such as
/// <c>{"name":"decimal","nullable":false}</c>. The name is not validated: any PureQL
/// type name, list type names included, is read and written as it is.
/// </summary>
internal sealed class ParameterTypeConverter : JsonConverter<IParameterType>
{
    private const string Element = "PureQL parameter type";

    public override IParameterType Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        reader.ExpectObject(Element);

        string? name = null;
        bool? nullable = null;

        while (reader.NextProperty(out string property))
        {
            switch (property)
            {
                case "name":
                    name = reader.ExpectString($"property 'name' of the {Element}");
                    break;
                case "nullable":
                    nullable = reader.ExpectBoolean(
                        $"property 'nullable' of the {Element}"
                    );
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return new ParameterTypeJsonModel(
            new String(
                name ?? throw Utf8JsonReaderExtensions.MissingProperty("name", Element)
            ),
            (
                nullable
                ?? throw Utf8JsonReaderExtensions.MissingProperty("nullable", Element)
            )
                ? new True()
                : new False()
        );
    }

    public override void Write(
        Utf8JsonWriter writer,
        IParameterType value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();
        writer.WriteString("name", value.Name.TextValue);
        writer.WriteBoolean("nullable", value.Nullable.BoolValue);
        writer.WriteEndObject();
    }
}
