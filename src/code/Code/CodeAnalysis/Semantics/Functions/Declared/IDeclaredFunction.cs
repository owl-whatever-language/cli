namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Declared;

public interface IDeclaredFunction : IDeclaredSymbol<IConcreteFunctionDeclarationStatementSyntax>, IFunction
{
	#region Properties
	new string? Name { get; }
	new IReadOnlyList<IDeclaredFunctionParameter> Parameters { get; }
	new IDeclaredFunctionReturn Return { get; }

	IReadOnlyList<IFunctionParameter> IFunction.Parameters => Parameters;
	IFunctionReturn IFunction.Return => Return;
	#endregion
}

public sealed class DeclaredFunction : IDeclaredFunction
{
	#region Properties
	public IConcreteFunctionDeclarationStatementSyntax Declaration
	{
		get;
		set
		{
			field?.ThrowIfInvalidShadow(value);
			field = value;
		}
	}
	public string? Name => Declaration.Signature.Name.Value as string;
	string ISymbol.Name => Name ?? SymbolHelpers.ThrowSymbolWithoutNameException<string>();
	public IReadOnlyList<IDeclaredFunctionParameter> Parameters { get; }
	public IDeclaredFunctionReturn Return { get; }
	public ICallableFunction AsCallable { get; }
	#endregion

	#region Constructors
	public DeclaredFunction(IConcreteFunctionDeclarationStatementSyntax declaration)
	{
		Declaration = declaration;

		DeclaredFunctionParameter[] parameters = new DeclaredFunctionParameter[declaration.Signature.Parameters.Values.Count];
		for (int i = 0; i < parameters.Length; i++)
			parameters[i] = new(declaration.Signature.Parameters.Values[i], i);

		Parameters = parameters;
		Return = new DeclaredFunctionReturn(declaration.Signature.Return);

		AsCallable = new CallableFunction(this);
	}
	#endregion

	#region Methods
	public TextFragmentCollection GetDebugText()
	{
		TextFragmentCollection fragments = [];

		fragments.Add(Name ?? "???", ClassificationKind.Function);
		fragments.Add("(", ClassificationKind.Punctuation);

		for (int i = 0; i < Parameters.Count; i++)
		{
			if (i > 0)
				fragments.Add(", ", ClassificationKind.Punctuation);

			fragments.AddRange(Parameters[i]);
		}

		fragments.Add(")", ClassificationKind.Punctuation);

		if (Return.Type != SpecialTypes.Void)
		{
			fragments.Add(": ", ClassificationKind.Punctuation);
			fragments.AddRange(Return.Type);
		}

		return fragments;
	}
	#endregion
}
