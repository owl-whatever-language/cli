namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract.Statements;

public interface IAbstractStatement : IAbstractSyntaxNode
{
	#region Properties
	/// <summary>The concrete syntax node that this abstract syntax node is modelled after.</summary>
	new IConcreteStatement Concrete { get; }
	IConcreteSyntaxNode IAbstractSyntaxNode.Concrete => Concrete;
	#endregion
}

public interface IAbstractStatement<TConcrete> : IAbstractStatement, IAbstractSyntaxNode<TConcrete>
	where TConcrete : notnull, IConcreteStatement
{
	#region Properties
	new TConcrete Concrete { get; }
	IConcreteSyntaxNode IAbstractSyntaxNode.Concrete => Concrete;
	IConcreteStatement IAbstractStatement.Concrete => Concrete;
	#endregion
}

public abstract class BaseAbstractStatement<TConcrete> : BaseAbstractSyntaxNode<TConcrete>, IAbstractStatement<TConcrete>
	where TConcrete : notnull, IConcreteStatement
{
	#region Constructors
	protected BaseAbstractStatement(TConcrete concrete) : base(concrete) { }
	#endregion
}
