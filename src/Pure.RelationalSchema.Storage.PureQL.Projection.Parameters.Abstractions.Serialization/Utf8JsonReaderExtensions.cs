using System.Text.Json;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization;

/// <summary>
/// Forward-only reading helpers shared by the converters. A value of the wrong JSON type,
/// <c>null</c> included, is a <see cref="JsonException"/>; properties the converters do
/// not know are skipped by the caller.
/// </summary>
internal static class Utf8JsonReaderExtensions
{
    public static void ExpectObject(this ref Utf8JsonReader reader, string element)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected a JSON object for the {element}.");
        }
    }

    public static void ExpectArray(this ref Utf8JsonReader reader, string element)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected a JSON array for the {element}.");
        }
    }

    public static string ExpectString(this ref Utf8JsonReader reader, string element)
    {
        return reader.TokenType == JsonTokenType.String
            ? reader.GetString()!
            : throw new JsonException($"Expected a JSON string for the {element}.");
    }

    public static bool ExpectBoolean(this ref Utf8JsonReader reader, string element)
    {
        return reader.TokenType is JsonTokenType.True or JsonTokenType.False
            ? reader.GetBoolean()
            : throw new JsonException($"Expected a JSON boolean for the {element}.");
    }

    /// <summary>
    /// Advances to the value of the next property of the current object. Returns
    /// <c>false</c> at the end of the object.
    /// </summary>
    public static bool NextProperty(this ref Utf8JsonReader reader, out string name)
    {
        _ = reader.Read();

        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            name = null!;
            return false;
        }

        name = reader.GetString()!;
        _ = reader.Read();
        return true;
    }

    /// <summary>
    /// Advances to the next item of the current array. Returns <c>false</c> at its end.
    /// </summary>
    public static bool NextItem(this ref Utf8JsonReader reader)
    {
        _ = reader.Read();

        return reader.TokenType != JsonTokenType.EndArray;
    }

    public static JsonException MissingProperty(string property, string element)
    {
        return new JsonException(
            $"The {element} is missing the required property '{property}'."
        );
    }
}
