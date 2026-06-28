namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Declared;

public interface IDeclaredFunctionParameter : IDeclaredSymbol<IConcreteFunctionParameterSyntax>, IFunctionParameter
{
	#region Properties
	new IType Type { get; set; }
	#endregion
}

public sealed class DeclaredFunctionParameter : IDeclaredFunctionParameter
{
	#region Properties
	public IConcreteFunctionParameterSyntax Declaration
	{
		get;
		set
		{
			field?.ThrowIfInvalidShadow(value);
			field = value;
		}
	}
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
	public string? Name => Declaration.Name.Value as string;
	string ISymbol.Name => Name ?? SymbolHelpers.ThrowSymbolWithoutNameException<string>();
	public ICallableFunctionParameter AsCallable { get; }
	#endregion

	#region Constructors
	public DeclaredFunctionParameter(IConcreteFunctionParameterSyntax declaration, int index)
	{
		Declaration = declaration;
		Index = index;
		Type = SpecialTypes.Unknown;
		AsCallable = new CallableFunctionParameter(this);
	}
	#endregion

	#region Methods
	TextFragmentCollection IDebugTreePrintable.GetFragments()
	{
		TextFragmentCollection fragments = [];

		fragments.AddRange(Type);
		fragments.Add(" ", ClassificationKind.Whitespace);
		fragments.Add(Name ?? "???", ClassificationKind.Parameter);

		return fragments;
	}
	#endregion
}
