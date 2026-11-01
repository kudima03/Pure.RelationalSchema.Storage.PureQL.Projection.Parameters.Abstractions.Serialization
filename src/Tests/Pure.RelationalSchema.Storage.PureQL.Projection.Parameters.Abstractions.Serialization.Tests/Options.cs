using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization.Tests;

internal static class Options
{
    public static JsonSerializerOptions WithConverters()
    {
        JsonSerializerOptions options = new JsonSerializerOptions();

        foreach (JsonConverter converter in new PureQLParametersConverters())
        {
            options.Converters.Add(converter);
        }

        return options;
    }
}
