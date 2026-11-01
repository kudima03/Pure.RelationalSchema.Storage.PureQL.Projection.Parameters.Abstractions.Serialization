using Pure.Primitives.Abstractions.String;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.ListParameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Parameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.ParameterType;
using String = Pure.Primitives.String.String;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization.Tests.Fakes;

internal sealed record ListParameterDouble : IListParameter
{
    public ListParameterDouble(string name, string value, params IParameter[] items)
    {
        Type = new ParameterTypeDouble(name, false);
        Items = items;
        Value = new String(value);
    }

    public IParameterType Type { get; }

    public IEnumerable<IParameter> Items { get; }

    public IString Value { get; }
}
