namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract.Expressions;

public sealed class AbstractAccessExpression : BaseAbstractExpression<ConcreteAccessExpression>
{
	#region Properties
	public ITokenNode Name => Concrete.Name;
	public string? Value => Name.Value as string;
	#endregion

	#region Constructors
	public AbstractAccessExpression(ConcreteAccessExpression concrete) : base(concrete) { }
	#endregion

	#region Methods
	public override IEnumerable<ISyntaxNode> GetChildren() => [Name];
	#endregion
}
