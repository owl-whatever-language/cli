namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Statements;

public interface ISemanticStatement : ISemanticSyntaxNode
{
	#region Properties
	/// <summary>The abstract syntax node that this semantic syntax node is modelled after.</summary>
	new IAbstractStatement Abstract { get; }
	IAbstractSyntaxNode ISemanticSyntaxNode.Abstract => Abstract;
	#endregion
}

public interface ISemanticStatement<T> : ISemanticStatement, ISemanticSyntaxNode<T>
	where T : notnull, IAbstractStatement
{
	#region Properties
	IAbstractSyntaxNode ISemanticSyntaxNode.Abstract => ((ISemanticSyntaxNode<T>)this).Abstract;
	IAbstractStatement ISemanticStatement.Abstract => ((ISemanticSyntaxNode<T>)this).Abstract;
	#endregion
}

public abstract class BaseSemanticStatement<T> : BaseSemanticSyntaxNode<T>, ISemanticStatement
	where T : notnull, IAbstractStatement
{
	#region Properties
	IAbstractStatement ISemanticStatement.Abstract => Abstract;
	IAbstractSyntaxNode ISemanticSyntaxNode.Abstract => Abstract;
	#endregion

	#region Constructors
	protected BaseSemanticStatement(T @abstract) : base(@abstract) { }
	#endregion
}
