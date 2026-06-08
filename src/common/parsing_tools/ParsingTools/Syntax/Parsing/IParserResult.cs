namespace OwlDomain.ParsingTools.Syntax.Parsing;

/// <summary>
/// 	Represents the result of a parsing operation.
/// </summary>
public interface IParserResult
{
	#region Properties
	/// <summary>The source file that was parsed.</summary>
	ISourceFile Source { get; }

	/// <summary>The diagnostics that occurred during the parsing process.</summary>
	IDiagnosticBag Diagnostics { get; }
	#endregion
}

/// <summary>
/// 	Represents the result of a parsing operation.
/// </summary>
/// <typeparam name="T">The type of the root node in the parsed document.</typeparam>
public interface IParserResult<out T> : IParserResult
	where T : notnull, IConcreteSyntaxNode
{
	#region Properties
	/// <summary>The root node that was parsed.</summary>
	public T Root { get; }
	#endregion
}

/// <summary>
/// 	Represents the result of a parsing operation.
/// </summary>
/// <typeparam name="T">The type of the root node in the parsed document.</typeparam>
public class ParserResult<T> : IParserResult<T>
	where T : notnull, IConcreteSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public ISourceFile Source { get; }

	/// <inheritdoc/>
	public IDiagnosticBag Diagnostics { get; }

	/// <inheritdoc/>
	public T Root { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="ParserResult{T}"/> instance.</summary>
	/// <param name="source">The source file that was parsed.</param>
	/// <param name="diagnostics">The diagnostics that occurred during the parsing process.</param>
	/// <param name="root">The root node that was parsed.</param>
	public ParserResult(ISourceFile source, IDiagnosticBag diagnostics, T root)
	{
		Source = source;
		Diagnostics = diagnostics;
		Root = root;
	}
	#endregion
}
