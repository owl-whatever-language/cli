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
public abstract class BaseParserResult<TTree> : StageResult, IParserResult<TTree>
	where TTree : notnull, IConcreteSyntaxTree
{
	#region Properties
	/// <inheritdoc/>
	public override string Name => "parsing";

	/// <inheritdoc/>
	public TTree Tree { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseParserResult{TTree}"/> properties.</summary>
	/// <param name="diagnostics">The diagnostics that occurred during the parsing process.</param>
	/// <param name="duration">The amount of time it took to parse the source file.</param>
	/// <param name="tree">The concrete syntax tree (CST) that was parsed.</param>
	protected BaseParserResult(IDiagnosticBag diagnostics, TimeSpan duration, TTree tree) : base(diagnostics, duration)
	{
		Tree = tree;
	}
	#endregion
}
