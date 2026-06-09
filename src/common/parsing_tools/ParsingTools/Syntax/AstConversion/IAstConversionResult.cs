namespace OwlDomain.ParsingTools.Syntax.AstConversion;

/// <summary>
/// 	Represents the result of converting a concrete syntax tree (CST) into an abstract syntax tree (AST).
/// </summary>
public interface IAstConversionResult : IStageResult
{
	#region Properties
	/// <summary>The generated abstract syntax tree (AST).</summary>
	IAbstractSyntaxTree Tree { get; }
	#endregion
}

/// <summary>
/// 	Represents the result of converting a concrete syntax tree (CST) into an abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TTree">The type of the generated abstract syntax tree (AST).</typeparam>
public interface IAstConversionResult<out TTree> : IAstConversionResult
	where TTree : notnull, IAbstractSyntaxTree
{
	#region Properties
	/// <summary>The generated abstract syntax tree (AST).</summary>
	new TTree Tree { get; }
	IAbstractSyntaxTree IAstConversionResult.Tree => Tree;
	#endregion
}

/// <summary>
/// 	Represents the result of converting a concrete syntax tree (CST) into an abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TTree">The type of the generated abstract syntax tree (AST).</typeparam>
/// <typeparam name="TTreeRoot">The type of the root node in the abstract syntax tree (AST).</typeparam>
public interface IAstConversionResult<out TTree, out TTreeRoot> : IAstConversionResult<TTree>
	where TTree : notnull, IAbstractSyntaxTree<TTreeRoot>
	where TTreeRoot : notnull, IAbstractSyntaxNode
{
}

/// <summary>
/// 	Represents the result of converting a concrete syntax tree (CST) into an abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TTree">The type of the generated abstract syntax tree (AST).</typeparam>
/// <typeparam name="TTreeRoot">The type of the root node in the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</typeparam>
public interface IAstConversionResult<out TTree, out TTreeRoot, out TConcrete> : IAstConversionResult<TTree, TTreeRoot>
	where TTree : notnull, IAbstractSyntaxTree<TTreeRoot, TConcrete>
	where TTreeRoot : notnull, IAbstractSyntaxNode
	where TConcrete : notnull, IConcreteSyntaxTree
{
}

/// <summary>
/// 	Represents the result of converting a concrete syntax tree (CST) into an abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TTree">The type of the generated abstract syntax tree (AST).</typeparam>
/// <typeparam name="TTreeRoot">The type of the root node in the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</typeparam>
/// <typeparam name="TConcreteRoot">The type of the root node in the concrete syntax tree (CST).</typeparam>
public interface IAstConversionResult<out TTree, out TTreeRoot, out TConcrete, out TConcreteRoot> : IAstConversionResult<TTree, TTreeRoot, TConcrete>
	where TTree : notnull, IAbstractSyntaxTree<TTreeRoot, TConcrete, TConcreteRoot>
	where TTreeRoot : notnull, IAbstractSyntaxNode<TConcreteRoot>
	where TConcrete : notnull, IConcreteSyntaxTree<TConcreteRoot>
	where TConcreteRoot : notnull, IConcreteSyntaxNode
{
}

/// <summary>
/// 	Represents the base implementation for a result of converting a concrete syntax tree (CST) into an abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TTree">The type of the generated abstract syntax tree (AST).</typeparam>
/// <typeparam name="TTreeRoot">The type of the root node in the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</typeparam>
/// <typeparam name="TConcreteRoot">The type of the root node in the concrete syntax tree (CST).</typeparam>
public abstract class BaseAstConversionResult<TTree, TTreeRoot, TConcrete, TConcreteRoot> : StageResult, IAstConversionResult<TTree, TTreeRoot, TConcrete, TConcreteRoot>
	where TTree : notnull, IAbstractSyntaxTree<TTreeRoot, TConcrete, TConcreteRoot>
	where TTreeRoot : notnull, IAbstractSyntaxNode<TConcreteRoot>
	where TConcrete : notnull, IConcreteSyntaxTree<TConcreteRoot>
	where TConcreteRoot : notnull, IConcreteSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override string Name => "ast_conversion";

	/// <inheritdoc/>
	public TTree Tree { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseAstConversionResult{TTree, TTreeRoot, TConcrete, TConcreteRoot}"/> properties.</summary>
	/// <param name="tree">The generated abstract syntax tree (AST).</param>
	/// <param name="diagnostics">The diagnostics that occurred during the stage.</param>
	/// <param name="duration">The amount of time it took for the stage to finish processing.</param>
	protected BaseAstConversionResult(TTree tree, IDiagnosticBag diagnostics, TimeSpan duration) : base(diagnostics, duration)
	{
		Tree = tree;
	}
	#endregion
}
