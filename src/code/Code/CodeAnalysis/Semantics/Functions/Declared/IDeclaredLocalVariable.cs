namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Declared;

public interface IDeclaredLocalVariable : IDeclaredSymbol<IConcreteVariableDeclarationStatementSyntax>, ILocalVariable
{
	#region Properties
	new IType Type { get; set; }
	#endregion
}

public sealed class DeclaredLocalVariable : IDeclaredLocalVariable
{
	#region Properties
	public IConcreteVariableDeclarationStatementSyntax Declaration
	{
		get;
		set
		{
			field?.ThrowIfInvalidShadow(value);
			field = value;
		}
	}
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
	#endregion

	#region Constructors
	public DeclaredLocalVariable(IConcreteVariableDeclarationStatementSyntax declaration)
	{
		Declaration = declaration;
		Type = SpecialTypes.Unknown;
	}
	#endregion

	#region Methods
	public TextFragmentCollection GetDebugText()
	{
		TextFragmentCollection fragments = [];

		fragments.AddRange(Type);
		fragments.Add(" ", ClassificationKind.Whitespace);
		fragments.Add(Name ?? "???", ClassificationKind.Variable);

		return fragments;
	}
	#endregion
}
