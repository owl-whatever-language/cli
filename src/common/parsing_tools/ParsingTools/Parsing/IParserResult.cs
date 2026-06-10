namespace OwlDomain.ParsingTools.Parsing;

/// <summary>
/// 	Represents the result of a parsing operation.
/// </summary>
public interface IParserResult : IStageResult
{
	#region Properties
	/// <summary>The concrete syntax tree (CST) that was parsed.</summary>
	IConcreteSyntaxTree Tree { get; }
	#endregion
}

/// <summary>
/// 	Represents the result of a parsing operation.
/// </summary>
/// <typeparam name="TTree">The type of the concrete syntax tree (CST) in the parsed document.</typeparam>
public interface IParserResult<out TTree> : IParserResult
	where TTree : notnull, IConcreteSyntaxTree
{
	#region Properties
	/// <summary>The syntax tree that was parsed.</summary>
	new TTree Tree { get; }
	IConcreteSyntaxTree IParserResult.Tree => Tree;
	#endregion
}

/// <summary>
/// 	Represents the result of a parsing operation.
/// </summary>
/// <typeparam name="TTree">The type of the concrete syntax tree (CST) in the parsed document.</typeparam>
/// <typeparam name="TTreeRoot">The type of the root node in the concrete syntax tree (CST).</typeparam>
public interface IParserResult<out TTree, out TTreeRoot> : IParserResult<TTree>
	where TTree : notnull, IConcreteSyntaxTree<TTreeRoot>
	where TTreeRoot : notnull, IConcreteSyntaxNode
{
}

/// <summary>
/// 	Represents the result of a parsing operation.
/// </summary>
/// <typeparam name="TTree">The type of the concrete syntax tree (CST) in the parsed document.</typeparam>
/// <typeparam name="TTreeRoot">The type of the root node in the concrete syntax tree (CST).</typeparam>
public abstract class BaseParserResult<TTree, TTreeRoot> : StageResult, IParserResult<TTree, TTreeRoot>
	where TTree : notnull, IConcreteSyntaxTree<TTreeRoot>
	where TTreeRoot : notnull, IConcreteSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override string Name => "parsing";

	/// <inheritdoc/>
	public TTree Tree { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseParserResult{TTree, TReeRoot}"/> properties.</summary>
	/// <param name="tree">The concrete syntax tree (CST) that was parsed.</param>
	/// <param name="diagnostics">The diagnostics that occurred during the parsing process.</param>
	/// <param name="duration">The amount of time it took to parse the source file.</param>
	protected BaseParserResult(TTree tree, IDiagnosticBag diagnostics, TimeSpan duration) : base(diagnostics, duration)
	{
		Tree = tree;
	}
	#endregion
}
