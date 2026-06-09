namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract.Statements;

public sealed class AbstractVariableDeclarationStatement : BaseAbstractStatement<ConcreteVariableDeclarationStatement>
{
	#region Properties
	public ITokenNode TypeName => Concrete.TypeName;
	public ITokenNode Name => Concrete.Name;
	public IAbstractExpression Value { get; }
	#endregion

	#region Constructors
	public AbstractVariableDeclarationStatement(ConcreteVariableDeclarationStatement concrete, IAbstractExpression value) : base(concrete)
	{
		Value = value;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISyntaxNode> GetChildren() => [TypeName, Name, Value];
	#endregion
}
