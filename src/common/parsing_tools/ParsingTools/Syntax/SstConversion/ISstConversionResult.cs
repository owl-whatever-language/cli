namespace OwlDomain.ParsingTools.Syntax.SstConversion;

/// <summary>
/// 	Represents the result of converting a abstract syntax tree (AST) into an semantic syntax tree (SST).
/// </summary>
public interface ISstConversionResult : IStageResult
{
	#region Properties
	/// <summary>The generated semantic syntax tree (SST).</summary>
	ISemanticSyntaxTree Tree { get; }
	#endregion
}

/// <summary>
/// 	Represents the result of converting a abstract syntax tree (AST) into an semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TTree">The type of the generated semantic syntax tree (SST).</typeparam>
public interface ISstConversionResult<out TTree> : ISstConversionResult
	where TTree : notnull, ISemanticSyntaxTree
{
	#region Properties
	/// <summary>The generated semantic syntax tree (SST).</summary>
	new TTree Tree { get; }
	ISemanticSyntaxTree ISstConversionResult.Tree => Tree;
	#endregion
}

/// <summary>
/// 	Represents the result of converting a abstract syntax tree (AST) into an semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TTree">The type of the generated semantic syntax tree (SST).</typeparam>
/// <typeparam name="TTreeRoot">The type of the root node in the semantic syntax tree (SST).</typeparam>
public interface ISstConversionResult<out TTree, out TTreeRoot> : ISstConversionResult<TTree>
	where TTree : notnull, ISemanticSyntaxTree<TTreeRoot>
	where TTreeRoot : notnull, ISemanticSyntaxNode
{
}

/// <summary>
/// 	Represents the result of converting a abstract syntax tree (AST) into an semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TTree">The type of the generated semantic syntax tree (SST).</typeparam>
/// <typeparam name="TTreeRoot">The type of the root node in the semantic syntax tree (SST).</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST) that the semantic syntax tree (SST) is modelled after.</typeparam>
public interface ISstConversionResult<out TTree, out TTreeRoot, out TAbstract> : ISstConversionResult<TTree, TTreeRoot>
	where TTree : notnull, ISemanticSyntaxTree<TTreeRoot, TAbstract>
	where TTreeRoot : notnull, ISemanticSyntaxNode
	where TAbstract : notnull, IAbstractSyntaxTree
{
}

/// <summary>
/// 	Represents the result of converting a abstract syntax tree (AST) into an semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TTree">The type of the generated semantic syntax tree (SST).</typeparam>
/// <typeparam name="TTreeRoot">The type of the root node in the semantic syntax tree (SST).</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST) that the semantic syntax tree (SST) is modelled after.</typeparam>
/// <typeparam name="TAbstractRoot">The type of the root node in the abstract syntax tree (AST).</typeparam>
public interface ISstConversionResult<out TTree, out TTreeRoot, out TAbstract, out TAbstractRoot> : ISstConversionResult<TTree, TTreeRoot, TAbstract>
	where TTree : notnull, ISemanticSyntaxTree<TTreeRoot, TAbstract, TAbstractRoot>
	where TTreeRoot : notnull, ISemanticSyntaxNode<TAbstractRoot>
	where TAbstract : notnull, IAbstractSyntaxTree<TAbstractRoot>
	where TAbstractRoot : notnull, IAbstractSyntaxNode
{
}

/// <summary>
/// 	Represents the base implementation for a result of converting a abstract syntax tree (AST) into an semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TTree">The type of the generated semantic syntax tree (SST).</typeparam>
/// <typeparam name="TTreeRoot">The type of the root node in the semantic syntax tree (SST).</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST) that the semantic syntax tree (SST) is modelled after.</typeparam>
/// <typeparam name="TAbstractRoot">The type of the root node in the abstract syntax tree (AST).</typeparam>
public abstract class BaseSstConversionResult<TTree, TTreeRoot, TAbstract, TAbstractRoot> : StageResult, ISstConversionResult<TTree, TTreeRoot, TAbstract, TAbstractRoot>
	where TTree : notnull, ISemanticSyntaxTree<TTreeRoot, TAbstract, TAbstractRoot>
	where TTreeRoot : notnull, ISemanticSyntaxNode<TAbstractRoot>
	where TAbstract : notnull, IAbstractSyntaxTree<TAbstractRoot>
	where TAbstractRoot : notnull, IAbstractSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override string Name => "sst_conversion";

	/// <inheritdoc/>
	public TTree Tree { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseSstConversionResult{TTree, TTreeRoot, TAbstract, TAbstractRoot}"/> properties.</summary>
	/// <param name="tree">The generated semantic syntax tree (SST).</param>
	/// <param name="diagnostics">The diagnostics that occurred during the stage.</param>
	/// <param name="duration">The amount of time it took for the stage to finish processing.</param>
	protected BaseSstConversionResult(TTree tree, IDiagnosticBag diagnostics, TimeSpan duration) : base(diagnostics, duration)
	{
		Tree = tree;
	}
	#endregion
}
