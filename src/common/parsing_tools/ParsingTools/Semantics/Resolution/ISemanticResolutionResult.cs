namespace OwlDomain.ParsingTools.Semantics.Resolution;

/// <summary>
/// 	Represents the result of the final semantic resolution which generates a semantic syntax tree (SST).
/// </summary>
public interface ISemanticResolutionResult : IStageResult
{
	#region Properties
	/// <summary>The generated semantic syntax tree (SST).</summary>
	ISemanticSyntaxTree Tree { get; }
	#endregion
}

/// <summary>
/// 	Represents the result of the final semantic resolution which generates a semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TSemantic">The type of the generated semantic syntax tree (SST).</typeparam>
public interface ISemanticResolutionResult<out TSemantic> : ISemanticResolutionResult
	where TSemantic : notnull, ISemanticSyntaxTree
{
	#region Properties
	/// <summary>The generated semantic syntax tree (SST).</summary>
	new TSemantic Tree { get; }
	ISemanticSyntaxTree ISemanticResolutionResult.Tree => Tree;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for the result of the final semantic resolution which generates a semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TSemantic">The type of the generated semantic syntax tree (SST).</typeparam>
public abstract class BaseSemanticResolutionResult<TSemantic> : StageResult, ISemanticResolutionResult<TSemantic>
	where TSemantic : notnull, ISemanticSyntaxTree
{
	#region Properties
	/// <inheritdoc/>
	public override string Name => "semantic_resolution";

	/// <inheritdoc/>
	public TSemantic Tree { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseSemanticResolutionResult{TSemantic}"/> properties.</summary>
	/// <param name="diagnostics">The diagnostics that occurred during the stage.</param>
	/// <param name="duration">The amount of time it took for the stage to finish processing.</param>
	/// <param name="tree">The generated semantic syntax tree (SST).</param>
	protected BaseSemanticResolutionResult(IDiagnosticBag diagnostics, TimeSpan duration, TSemantic tree) : base(diagnostics, duration)
	{
		Tree = tree;
	}
	#endregion
}
