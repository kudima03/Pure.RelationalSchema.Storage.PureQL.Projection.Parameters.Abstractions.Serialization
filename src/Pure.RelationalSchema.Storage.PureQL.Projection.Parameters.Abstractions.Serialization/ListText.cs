using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Parameter;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization;

/// <summary>
/// The informational value of a list parameter read without one: a JSON array of its
/// items' values, each as a JSON string, in order, such as <c>["paid","shipped"]</c>, and
/// <c>[]</c> for an empty list. It is escaped as the list parameters of
/// <c>Pure.RelationalSchema.Storage.PureQL.Projection.Parameters</c> escape it.
/// </summary>
internal static class ListText
{
    private static JsonWriterOptions Options =>
        new JsonWriterOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static string Of(IEnumerable<IParameter> items)
    {
        ArrayBufferWriter<byte> buffer = new ArrayBufferWriter<byte>();

        using (Utf8JsonWriter writer = new Utf8JsonWriter(buffer, Options))
        {
            writer.WriteStartArray();

            foreach (IParameter item in items)
            {
                writer.WriteStringValue(item.Value.TextValue);
            }

            writer.WriteEndArray();
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }
}
