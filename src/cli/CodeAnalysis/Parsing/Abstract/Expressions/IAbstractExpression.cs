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
	IConcreteSyntaxNode IAbstractSyntaxNode.Concrete => ((IAbstractSyntaxNode<TConcrete>)this).Concrete;
	IConcreteExpression IAbstractExpression.Concrete => ((IAbstractSyntaxNode<TConcrete>)this).Concrete;
	#endregion
}

public abstract class BaseAbstractExpression<TConcrete> : BaseAbstractSyntaxNode<TConcrete>, IAbstractExpression
	where TConcrete : notnull, IConcreteExpression
{
	#region Properties
	IConcreteExpression IAbstractExpression.Concrete => Concrete;
	IConcreteSyntaxNode IAbstractSyntaxNode.Concrete => Concrete;
	#endregion

	#region Constructors
	protected BaseAbstractExpression(TConcrete concrete) : base(concrete) { }
	#endregion
}
