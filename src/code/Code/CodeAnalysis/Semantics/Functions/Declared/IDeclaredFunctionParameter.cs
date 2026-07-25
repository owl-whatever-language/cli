namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Declared;

public interface IDeclaredFunctionParameter : IMutableDeclaredSymbol<IConcreteFunctionParameterSyntax>, IFunctionParameter
{
	#region Properties
	new IType Type { get; set; }
	#endregion
}

public sealed class DeclaredFunctionParameter : BaseDeclaredSymbol<IConcreteFunctionParameterSyntax>, IDeclaredFunctionParameter
{
	#region Properties
	public override string? Name => Declaration.Name.Value as string;
	public override ClassificationKind Classification => ClassificationKind.Parameter;
	public int Index { get; }
	public IType Type
	{
		get;
		set
		{
			field?.ThrowIfInvalidShadow(value);
			field = value;
		}
	}
	public ICallableFunctionParameter AsCallable { get; }
	#endregion

	#region Constructors
	public DeclaredFunctionParameter(IConcreteFunctionParameterSyntax declaration, int index) : base(declaration)
	{
		Index = index;
		Type = SpecialTypes.Unknown;
		AsCallable = new CallableFunctionParameter(this);
	}
	#endregion

	#region Methods
	public override TextFragmentCollection GetDebugText()
	{
		TextFragmentCollection fragments = [];

		fragments.AddRange(Type);
		fragments.Add(" ", ClassificationKind.Whitespace);
		fragments.Add(Name ?? "???", ClassificationKind.Parameter);

		return fragments;
	}
	#endregion
}
