namespace OwlDomain.ParsingTools.Syntax.Lexing;

/// <summary>
/// 	Represents the result of a lexing operation.
/// </summary>
public interface ILexerResult
{
	#region Properties
	/// <summary>The source file that was lexed.</summary>
	ISourceFile Source { get; }

	/// <summary>The diagnostics that occurred during the lexing process.</summary>
	IDiagnosticBag Diagnostics { get; }

	/// <summary>The lexed tokens.</summary>
	IReadOnlyList<ITokenNode> Tokens { get; }
	#endregion
}

/// <summary>
/// 	Represents the result of a lexing operation.
/// </summary>
public sealed class LexerResult : ILexerResult
{
	#region Properties
	/// <inheritdoc/>
	public ISourceFile Source { get; }

	/// <inheritdoc/>
	public IDiagnosticBag Diagnostics { get; }

	/// <inheritdoc/>
	public IReadOnlyList<ITokenNode> Tokens { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new lexer result.</summary>
	/// <param name="source">The source file that was lexed.</param>
	/// <param name="diagnostics">The diagnostics that occurred during the lexing process.</param>
	/// <param name="tokens">The lexed tokens.</param>
	public LexerResult(ISourceFile source, IDiagnosticBag diagnostics, IReadOnlyList<ITokenNode> tokens)
	{
		Source = source;
		Diagnostics = diagnostics;
		Tokens = tokens;
	}
	#endregion
}
