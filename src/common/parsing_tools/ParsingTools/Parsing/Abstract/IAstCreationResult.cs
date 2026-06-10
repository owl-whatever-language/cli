namespace OwlDomain.ParsingTools.Parsing.Abstract;

/// <summary>
/// 	Represents the result of converting a concrete syntax tree (CST) into an abstract syntax tree (AST).
/// </summary>
public interface IAstCreationResult : IStageResult
{
	#region Properties
	/// <summary>The generated abstract syntax tree (AST).</summary>
	IAbstractSyntaxTree Tree { get; }
	#endregion
}

/// <summary>
/// 	Represents the result of converting a concrete syntax tree (CST) into an abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TAbstract">The type of the generated abstract syntax tree (AST).</typeparam>
public interface IAstCreationResult<out TAbstract> : IAstCreationResult
	where TAbstract : notnull, IAbstractSyntaxTree
{
	#region Properties
	/// <summary>The generated abstract syntax tree (AST).</summary>
	new TAbstract Tree { get; }
	IAbstractSyntaxTree IAstCreationResult.Tree => Tree;
	#endregion
}

/// <summary>
/// 	Represents the result of converting a concrete syntax tree (CST) into an abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TAbstract">The type of the generated abstract syntax tree (AST).</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</typeparam>
public interface IAstCreationResult<out TAbstract, out TConcrete> : IAstCreationResult<TAbstract>
	where TAbstract : notnull, IAbstractSyntaxTree<TConcrete>
	where TConcrete : notnull, IConcreteSyntaxTree
{
}

/// <summary>
/// 	Represents the base implementation for a result of converting a concrete syntax tree (CST) into an abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TAbstract">The type of the generated abstract syntax tree (AST).</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</typeparam>
public abstract class BaseAstCreationResult<TAbstract, TConcrete> : StageResult, IAstCreationResult<TAbstract, TConcrete>
	where TAbstract : notnull, IAbstractSyntaxTree<TConcrete>
	where TConcrete : notnull, IConcreteSyntaxTree
{
	#region Properties
	/// <inheritdoc/>
	public override string Name => "ast_creation";

	/// <inheritdoc/>
	public TAbstract Tree { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseAstCreationResult{TAbstract, TConcrete}"/> properties.</summary>
	/// <param name="diagnostics">The diagnostics that occurred during the stage.</param>
	/// <param name="duration">The amount of time it took for the stage to finish processing.</param>
	/// <param name="tree">The generated abstract syntax tree (AST).</param>
	protected BaseAstCreationResult(IDiagnosticBag diagnostics, TimeSpan duration, TAbstract tree) : base(diagnostics, duration)
	{
		Tree = tree;
	}
	#endregion
}
