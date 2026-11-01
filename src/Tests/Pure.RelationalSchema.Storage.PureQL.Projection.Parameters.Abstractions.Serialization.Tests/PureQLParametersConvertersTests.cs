using System.Collections;
using System.Text.Json.Serialization;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.ListParameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Parameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.ParameterType;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization.Tests;

public sealed record PureQLParametersConvertersTests
{
    [Fact]
    public void EnumeratesConverterOfEachAbstraction()
    {
        JsonConverter[] converters = [.. new PureQLParametersConverters()];

        Assert.Collection(
            converters,
            converter => Assert.True(converter.CanConvert(typeof(IParameterType))),
            converter => Assert.True(converter.CanConvert(typeof(IParameter))),
            converter => Assert.True(converter.CanConvert(typeof(IListParameter)))
        );
    }

    [Fact]
    public void ConvertersConvertOnlyTheirAbstraction()
    {
        JsonConverter[] converters = [.. new PureQLParametersConverters()];

        Assert.False(converters[0].CanConvert(typeof(IParameter)));
        Assert.False(converters[1].CanConvert(typeof(IListParameter)));
        Assert.False(converters[2].CanConvert(typeof(IParameter)));
    }

    [Fact]
    public void NonGenericEnumeratorReturnsAllConverters()
    {
        IEnumerable converters = new PureQLParametersConverters();

        int count = 0;

        foreach (object converter in converters)
        {
            _ = Assert.IsType<JsonConverter>(converter, exactMatch: false);
            count++;
        }

        Assert.Equal(3, count);
    }
}
