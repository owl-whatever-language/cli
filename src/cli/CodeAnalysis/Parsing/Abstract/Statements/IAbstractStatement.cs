namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract.Statements;

public interface IAbstractStatement : IAbstractSyntaxNode
{
	#region Properties
	/// <summary>The concrete syntax node that this abstract syntax node is modelled after.</summary>
	new IConcreteStatement Concrete { get; }
	IConcreteSyntaxNode IAbstractSyntaxNode.Concrete => Concrete;
	#endregion
}

public interface IAbstractStatement<T> : IAbstractStatement, IAbstractSyntaxNode<T>
	where T : notnull, IConcreteStatement
{
	#region Properties
	IConcreteSyntaxNode IAbstractSyntaxNode.Concrete => ((IAbstractSyntaxNode<T>)this).Concrete;
	IConcreteStatement IAbstractStatement.Concrete => ((IAbstractSyntaxNode<T>)this).Concrete;
	#endregion
}

public abstract class BaseAbstractStatement<T> : BaseAbstractSyntaxNode<T>, IAbstractStatement
	where T : notnull, IConcreteStatement
{
	#region Properties
	IConcreteStatement IAbstractStatement.Concrete => Concrete;
	IConcreteSyntaxNode IAbstractSyntaxNode.Concrete => Concrete;
	#endregion

	#region Constructors
	protected BaseAbstractStatement(T concrete) : base(concrete) { }
	#endregion
}
