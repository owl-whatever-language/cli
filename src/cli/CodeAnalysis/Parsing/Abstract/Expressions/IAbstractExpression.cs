namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract.Expressions;

public interface IAbstractExpression : IAbstractSyntaxNode
{
	#region Properties
	/// <summary>The concrete syntax node that this abstract syntax node is modelled after.</summary>
	new IConcreteExpression Concrete { get; }
	IConcreteSyntaxNode IAbstractSyntaxNode.Concrete => Concrete;
	#endregion
}

public interface IAbstractExpression<T> : IAbstractExpression, IAbstractSyntaxNode<T>
	where T : notnull, IConcreteExpression
{
	#region Properties
	IConcreteSyntaxNode IAbstractSyntaxNode.Concrete => ((IAbstractSyntaxNode<T>)this).Concrete;
	IConcreteExpression IAbstractExpression.Concrete => ((IAbstractSyntaxNode<T>)this).Concrete;
	#endregion
}

public abstract class BaseAbstractExpression<T> : BaseAbstractSyntaxNode<T>, IAbstractExpression
	where T : notnull, IConcreteExpression
{
	#region Properties
	IConcreteExpression IAbstractExpression.Concrete => Concrete;
	IConcreteSyntaxNode IAbstractSyntaxNode.Concrete => Concrete;
	#endregion

	#region Constructors
	protected BaseAbstractExpression(T concrete) : base(concrete) { }
	#endregion
}
