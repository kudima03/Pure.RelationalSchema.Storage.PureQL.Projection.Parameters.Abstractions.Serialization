using System.Text.Json;
using System.Text.Json.Serialization;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.ListParameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Parameter;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization;

/// <summary>
/// Converts <see cref="IListParameter"/> to and from
/// <c>{"type", "value", "items"}</c>, as <see cref="ParameterConverter"/> does for a
/// list parameter. The <c>items</c> are required; the <c>value</c> may be left out.
/// </summary>
internal sealed class ListParameterConverter : JsonConverter<IListParameter>
{
    private const string Element = "PureQL list parameter";

    private readonly ParameterConverter _parameters = new ParameterConverter();

    public override IListParameter Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return _parameters.Read(ref reader, typeof(IParameter), options) as IListParameter
            ?? throw Utf8JsonReaderExtensions.MissingProperty("items", Element);
    }

    public override void Write(
        Utf8JsonWriter writer,
        IListParameter value,
        JsonSerializerOptions options
    )
    {
        _parameters.Write(writer, value, options);
    }
}
