namespace OwlDomain.ParsingTools.Finalisation;

/// <summary>
/// 	Represents the result of the syntax tree finalisation stage.
/// </summary>
public interface ISyntaxFinalisationResult : IStageResult
{
	#region Properties
	/// <summary>The generated final syntax tree (FST).</summary>
	IFinalSyntaxTree Tree { get; }
	#endregion
}

/// <summary>
/// 	Represents the result of the syntax tree finalisation stage.
/// </summary>
/// <typeparam name="TFinal">The type of the final syntax tree (FST).</typeparam>
public interface ISyntaxFinalisationResult<out TFinal> : ISyntaxFinalisationResult
where TFinal : notnull, IFinalSyntaxTree
{
	#region Properties
	/// <summary>The generated final syntax tree (FST).</summary>
	new TFinal Tree { get; }
	IFinalSyntaxTree ISyntaxFinalisationResult.Tree => Tree;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for the result of the syntax tree finalisation stage.
/// </summary>
/// <typeparam name="TFinal">The type of the final syntax tree (FST).</typeparam>
public abstract class BaseSyntaxFinalisationResult<TFinal> : StageResult, ISyntaxFinalisationResult<TFinal>
	where TFinal : notnull, IFinalSyntaxTree
{
	#region Properties
	/// <inheritdoc/>
	public override string Name => "syntax_finalisation";

	/// <inheritdoc/>
	public TFinal Tree { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseSyntaxFinalisationResult{TFinal}"/> properties.</summary>
	/// <param name="diagnostics">The diagnostics that occurred during the stage.</param>
	/// <param name="duration">The amount of time it took for the stage to finish processing.</param>
	/// <param name="tree">The generated abstract syntax tree (AST).</param>
	protected BaseSyntaxFinalisationResult(IDiagnosticBag diagnostics, TimeSpan duration, TFinal tree) : base(diagnostics, duration)
	{
		Tree = tree;
	}
	#endregion
}
