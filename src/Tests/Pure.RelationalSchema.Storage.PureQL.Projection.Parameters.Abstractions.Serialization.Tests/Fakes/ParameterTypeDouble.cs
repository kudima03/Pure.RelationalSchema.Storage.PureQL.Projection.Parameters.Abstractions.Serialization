using Pure.Primitives.Abstractions.Bool;
using Pure.Primitives.Abstractions.String;
using Pure.Primitives.Bool;
using Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.ParameterType;
using String = Pure.Primitives.String.String;

namespace Pure.RelationalSchema.Storage.PureQL.Projection.Parameters.Abstractions.Serialization.Tests.Fakes;

internal sealed record ParameterTypeDouble : IParameterType
{
    public ParameterTypeDouble(string name, bool nullable)
    {
        Name = new String(name);
        Nullable = nullable ? new True() : new False();
    }

    public IString Name { get; }

    public IBool Nullable { get; }
}
