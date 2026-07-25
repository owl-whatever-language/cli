namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Declared;

public interface IDeclaredFunction : IMutableDeclaredSymbol<IConcreteFunctionDeclarationStatementSyntax>, IFunction
{
	#region Properties
	new IReadOnlyList<IDeclaredFunctionParameter> Parameters { get; }
	new IDeclaredFunctionReturn Return { get; }

	IReadOnlyList<IFunctionParameter> IFunction.Parameters => Parameters;
	IFunctionReturn IFunction.Return => Return;
	#endregion
}

public sealed class DeclaredFunction : BaseDeclaredSymbol<IConcreteFunctionDeclarationStatementSyntax>, IDeclaredFunction
{
	#region Properties
	public override string? Name => Declaration.Signature.Name.Value as string;
	public override ClassificationKind Classification => ClassificationKind.Function;
	public IReadOnlyList<IDeclaredFunctionParameter> Parameters { get; }
	public IDeclaredFunctionReturn Return { get; }
	public ICallableFunction AsCallable { get; }
	#endregion

	#region Constructors
	public DeclaredFunction(IConcreteFunctionDeclarationStatementSyntax declaration) : base(declaration)
	{
		DeclaredFunctionParameter[] parameters = new DeclaredFunctionParameter[declaration.Signature.Parameters.Values.Count];
		for (int i = 0; i < parameters.Length; i++)
			parameters[i] = new(declaration.Signature.Parameters.Values[i], i);

		Parameters = parameters;
		Return = new DeclaredFunctionReturn(declaration.Signature.Return);

		AsCallable = new CallableFunction(this);
	}
	#endregion

	#region Methods
	public override TextFragmentCollection GetDebugText()
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
