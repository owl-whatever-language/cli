namespace OwlDomain.ParsingTools.Parsing;

public abstract class BaseParser<TToken> : BaseParser
	where TToken : notnull, ISyntaxToken
{
	#region Constructors
	protected BaseParser(ISourceFile source, IReadOnlyList<ISyntaxToken> tokens) : base(source, tokens) { }
	#endregion

	#region Match methods
	protected bool Match(SyntaxKind kind, [NotNullWhen(true)] out TToken? token)
	{
		if (Match(kind, out ISyntaxToken? untyped))
		{
			token = Convert(untyped);
			return true;
		}

		token = default;
		return false;
	}
	protected bool Match(SyntaxKind kind, ClassificationKind classification, [NotNullWhen(true)] out TToken? token)
	{
		if (Match(kind, out ISyntaxToken? untyped))
		{
			token = Convert(untyped, classification);
			return true;
		}

		token = default;
		return false;
	}
	protected bool MatchAny([NotNullWhen(true)] out TToken? token, ClassificationKind classification, params ReadOnlySpan<SyntaxKind> kinds)
	{
		if (MatchAny(out ISyntaxToken? untyped, kinds))
		{
			token = Convert(untyped, classification);
			return true;
		}

		token = default;
		return false;
	}
	protected bool MatchAny([NotNullWhen(true)] out TToken? token, ClassificationKind classification, params IReadOnlyCollection<SyntaxKind> kinds)
	{
		if (MatchAny(out ISyntaxToken? untyped, kinds))
		{
			token = Convert(untyped, classification);
			return true;
		}

		token = default;
		return false;
	}
	#endregion

	#region Expect methods
	protected TToken ExpectSilent(SyntaxKind kind)
	{
		ISyntaxToken token = ExpectSilentCore(kind);
		return Convert(token);
	}
	protected TToken ExpectSilent(SyntaxKind kind, ClassificationKind classification)
	{
		ISyntaxToken token = ExpectSilentCore(kind);
		return Convert(token, classification);
	}
	protected TToken Expect(SyntaxKind kind, ClassificationKind classification, Action<TToken> callback)
	{
		if (Match(kind, classification, out TToken? token))
			return token;

		token = Fabricate(kind, classification);
		callback.Invoke(token);

		return token;
	}
	protected TToken ExpectMatching(SyntaxKind kind, ClassificationKind classification, Action<TToken> callback)
	{
		if (Match(kind, classification, out TToken? end) is false)
		{
			end = Fabricate(kind, classification);
			callback.Invoke(end);
		}

		return end;
	}
	protected TToken Expect(SyntaxKind kind, Action<ISyntaxToken> message)
	{
		ISyntaxToken token = ExpectCore(kind, message);
		return Convert(token);
	}
	#endregion

	#region Fabricate methods
	protected TToken Fabricate(SyntaxKind kind)
	{
		ISyntaxToken token = FabricateCore(kind);
		return Convert(token);
	}
	protected TToken Fabricate(SyntaxKind kind, ClassificationKind classification)
	{
		ISyntaxToken token = FabricateCore(kind);
		return Convert(token, classification);
	}
	#endregion

	#region Convert methods
	[return: NotNullIfNotNull(nameof(token))]
	protected abstract TToken? Convert(ISyntaxToken? token, ClassificationKind? classification = null);
	#endregion
}
