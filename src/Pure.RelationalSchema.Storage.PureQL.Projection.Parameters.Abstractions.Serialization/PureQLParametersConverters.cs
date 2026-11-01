using System.Collections;
using System.Text.Json.Serialization;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization;

/// <summary>
/// The <c>System.Text.Json</c> converters of the query parameter abstractions of
/// <c>Pure.RelationalSchema.Storage.PureQL.Projection</c>: <c>IParameterType</c> as
/// <c>{"name", "nullable"}</c>, <c>IParameter</c> as <c>{"type", "value"}</c>, and
/// <c>IListParameter</c> as <c>{"type", "value", "items"}</c>. Add each of them to
/// <c>JsonSerializerOptions.Converters</c>. The converters read and write JSON directly,
/// without reflection, so they are trimming- and native AOT-compatible.
/// </summary>
public sealed record PureQLParametersConverters : IEnumerable<JsonConverter>
{
    /// <summary>
    /// Returns the converters of <c>IParameterType</c>, <c>IParameter</c> and
    /// <c>IListParameter</c>, new instances on each enumeration.
    /// </summary>
    /// <returns>An enumerator over the three converters.</returns>
    public IEnumerator<JsonConverter> GetEnumerator()
    {
        yield return new ParameterTypeConverter();
        yield return new ParameterConverter();
        yield return new ListParameterConverter();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
