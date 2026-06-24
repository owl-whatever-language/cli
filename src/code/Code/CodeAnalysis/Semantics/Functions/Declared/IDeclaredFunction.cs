namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Declared;

public interface IDeclaredFunction : IDeclaredSymbol<IConcreteFunctionDeclarationStatementSyntax>, IFunction
{
	#region Properties
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
	public string? Name => Declaration.Name.Value as string;
	string ISymbol.Name => Name ?? SymbolHelpers.ThrowSymbolWithoutNameException<string>();
	public IReadOnlyList<IDeclaredFunctionParameter> Parameters { get; }
	public IDeclaredFunctionReturn Return { get; }
	public ICallableFunction AsCallable { get; }
	#endregion

	#region Constructors
	public DeclaredFunction(IConcreteFunctionDeclarationStatementSyntax declaration)
	{
		Declaration = declaration;

		DeclaredFunctionParameter[] parameters = new DeclaredFunctionParameter[declaration.Parameters.Values.Count];
		for (int i = 0; i < parameters.Length; i++)
			parameters[i] = new(declaration.Parameters.Values[i], i);

		Parameters = parameters;
		Return = new DeclaredFunctionReturn(declaration.Return);

		AsCallable = new CallableFunction(this);
	}
	#endregion
}
