namespace OwlDomain.ParsingTools.Lexing;

/// <summary>
/// 	Represents the result of a lexing operation.
/// </summary>
public interface ILexerResult : IStageResult
{
	#region Properties
	/// <summary>The source file that was lexed.</summary>
	ISourceFile Source { get; }

	/// <summary>The lexed tokens.</summary>
	IReadOnlyList<ITokenNode> Tokens { get; }
	#endregion
}

/// <summary>
/// 	Represents the result of a lexing operation.
/// </summary>
public sealed class LexerResult : StageResult, ILexerResult
{
	#region Properties
	/// <inheritdoc/>
	public override string Name => "lexing";

	/// <inheritdoc/>
	public ISourceFile Source { get; }

	/// <inheritdoc/>
	public IReadOnlyList<ITokenNode> Tokens { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new lexer result.</summary>
	/// <param name="diagnostics">The diagnostics that occurred during the lexing process.</param>
	/// <param name="duration">The amount of time it took to lex the source file.</param>
	/// <param name="source">The source file that was lexed.</param>
	/// <param name="tokens">The lexed tokens.</param>
	public LexerResult(IDiagnosticBag diagnostics, TimeSpan duration, ISourceFile source, IReadOnlyList<ITokenNode> tokens) : base(diagnostics, duration)
	{
		Source = source;
		Tokens = tokens;
	}
	#endregion
}
