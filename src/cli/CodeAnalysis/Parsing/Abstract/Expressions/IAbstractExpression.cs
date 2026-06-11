namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract.Expressions;

public interface IAbstractExpression : IAbstractSyntaxNode
{
	#region Properties
	/// <summary>The concrete syntax node that this abstract syntax node is modelled after.</summary>
	new IConcreteExpression Concrete { get; }
	IConcreteSyntaxNode IAbstractSyntaxNode.Concrete => Concrete;
	#endregion
}

public interface IAbstractExpression<TConcrete> : IAbstractExpression, IAbstractSyntaxNode<TConcrete>
	where TConcrete : notnull, IConcreteExpression
{
	#region Properties
	new TConcrete Concrete { get; }
	IConcreteSyntaxNode IAbstractSyntaxNode.Concrete => Concrete;
	IConcreteExpression IAbstractExpression.Concrete => Concrete;
	#endregion
}

public abstract class BaseAbstractExpression<TConcrete> : BaseAbstractSyntaxNode<TConcrete>, IAbstractExpression<TConcrete>
	where TConcrete : notnull, IConcreteExpression
{
	#region Constructors
	protected BaseAbstractExpression(TConcrete concrete) : base(concrete) { }
	#endregion
}
