namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Declared;

public interface IDeclaredLocalVariable : IMutableDeclaredSymbol<IConcreteVariableDeclarationStatementSyntax>, ILocalVariable
{
	#region Properties
	new IType Type { get; set; }
	#endregion
}

public sealed class DeclaredLocalVariable : BaseDeclaredSymbol<IConcreteVariableDeclarationStatementSyntax>, IDeclaredLocalVariable
{
	#region Properties
	public override string? Name => Declaration.Name.Value as string;
	public override ClassificationKind Classification => ClassificationKind.Variable;
	public IType Type
	{
		get;
		set
		{
			field?.ThrowIfInvalidShadow(value);
			field = value;
		}
	}
	#endregion

	#region Constructors
	public DeclaredLocalVariable(IConcreteVariableDeclarationStatementSyntax declaration) : base(declaration)
	{
		Type = SpecialTypes.Unknown;
	}
	#endregion

	#region Methods
	public override TextFragmentCollection GetDebugText()
	{
		TextFragmentCollection fragments = [];

		fragments.AddRange(Type);
		fragments.Add(" ", ClassificationKind.Whitespace);
		fragments.Add(Name ?? "???", ClassificationKind.Variable);

		return fragments;
	}
	#endregion
}
