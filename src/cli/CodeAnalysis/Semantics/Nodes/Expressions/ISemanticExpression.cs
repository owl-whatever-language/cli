namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Expressions;

public interface ISemanticExpression : ISemanticSyntaxNode
{
	#region Properties
	/// <summary>The abstract syntax node that this semantic syntax node is modelled after.</summary>
	new IAbstractExpression Abstract { get; }
	IAbstractSyntaxNode ISemanticSyntaxNode.Abstract => Abstract;

	ITypeInfo? Type { get; }
	#endregion
}

public interface ISemanticExpression<TAbstract> : ISemanticExpression, ISemanticSyntaxNode<TAbstract>
	where TAbstract : notnull, IAbstractExpression
{
	#region Properties
	IAbstractSyntaxNode ISemanticSyntaxNode.Abstract => ((ISemanticSyntaxNode<TAbstract>)this).Abstract;
	IAbstractExpression ISemanticExpression.Abstract => ((ISemanticSyntaxNode<TAbstract>)this).Abstract;
	#endregion
}

public abstract class BaseSemanticExpression<TAbstract> : BaseSemanticSyntaxNode<TAbstract>, ISemanticExpression
	where TAbstract : notnull, IAbstractExpression
{
	#region Properties
	IAbstractExpression ISemanticExpression.Abstract => Abstract;
	IAbstractSyntaxNode ISemanticSyntaxNode.Abstract => Abstract;
	public ITypeInfo? Type { get; }
	#endregion

	#region Constructors
	protected BaseSemanticExpression(TAbstract @abstract, ITypeInfo? type) : base(@abstract)
	{
		Type = type;
	}
	#endregion
}
