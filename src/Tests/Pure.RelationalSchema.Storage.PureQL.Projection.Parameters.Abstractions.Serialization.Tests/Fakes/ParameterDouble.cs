using Pure.Primitives.Abstractions.String;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Parameter;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.ParameterType;
using String = Pure.Primitives.String.String;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization.Tests.Fakes;

internal sealed record ParameterDouble : IParameter
{
    public ParameterDouble(string name, bool nullable, string value)
    {
        Type = new ParameterTypeDouble(name, nullable);
        Value = new String(value);
    }

    public IParameterType Type { get; }

    public IString Value { get; }
}
