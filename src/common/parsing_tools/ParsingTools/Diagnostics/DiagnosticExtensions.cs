namespace OwlDomain.ParsingTools.Diagnostics;

public static class DiagnosticExtensions
{
	extension(DiagnosticBag diagnostics)
	{
		#region Parser methods
		public Diagnostic ReportInfiniteLoop(IDiagnosticProvider provider, ISyntaxToken token)
		{
			StackTrace trace = new();

			return diagnostics
				.BuildError(provider, "infinite_parsing_loop", trace)
				.Add(token, lines => lines.AddLine("The parser got stuck in an infinite loop without it being accounted for."));
		}
		public Diagnostic ReportDuplicate(IDiagnosticProvider provider, ISyntaxToken token)
		{
			Debug.Assert(token.IsFabricated is false);

			string kind = token.Kind.Name;

			return diagnostics
				.BuildError(provider, $"duplicate_{kind}")
				.Add(token, lines => lines.AddLine($"A duplicate {kind.Replace('_', ' ')} '", token.ToFragment(), "' was encountered here."));
		}
		public Diagnostic ReportExpected(IDiagnosticProvider provider, ISyntaxToken token, string lexeme, string purpose)
		{
			Debug.Assert(token.IsFabricated);

			string kind = token.Kind.Name;
			string kindNatural = kind.Replace('_', ' ');
			TextFragment fragment = token.ToFragment(lexeme);

			return diagnostics
				.BuildError(provider, $"expected_{kind}")
				.Add(token, token.Position.Start, lines => lines.AddLine($"Expected {kindNatural.IndefiniteArticle} {kindNatural} '", fragment, $"' here to {purpose}."));
		}
		public Diagnostic ReportExpected(IDiagnosticProvider provider, ISyntaxToken token, string message)
		{
			Debug.Assert(token.IsFabricated);

			string kind = token.Kind.Name;

			return diagnostics
				.BuildError(provider, $"expected_{kind}")
				.Add(token, token.Position.Start, lines => lines.AddLine(message));
		}
		public Diagnostic ReportExpected(IDiagnosticProvider provider, ISyntaxToken token, Action<TextFragmentLineCollection> message)
		{
			Debug.Assert(token.IsFabricated);

			string kind = token.Kind.Name;

			return diagnostics
				.BuildError(provider, $"expected_{kind}")
				.Add(token, token.Position.Start, message);
		}
		public Diagnostic ReportExpectedClosing(
			IDiagnosticProvider provider,
			ISyntaxToken openingToken,
			ISyntaxToken closingToken,
			string closingLexeme,
			string purpose)
		{
			Debug.Assert(closingToken.IsFabricated);

			TextFragment openingFragment = openingToken.ToFragment(openingToken.Lexeme);
			TextFragment closingFragment = closingToken.ToFragment(closingLexeme);

			string openingKind = openingToken.Kind.Name;
			string closingKind = closingToken.Kind.Name;

			string openingNatural = openingKind.Replace('_', ' ');
			string closingNatural = closingKind.Replace('_', ' ');

			Diagnostic diagnostic = diagnostics
				.BuildError(provider, $"expected_{closingKind}")
				.Add(closingToken, closingToken.Position.Start, lines => lines.AddLine($"Expected {closingNatural.IndefiniteArticle} {closingNatural} '", closingFragment, $"' here to {purpose}."));

			if (openingToken.IsFabricated is false)
				diagnostic.Add(openingToken, lines => lines.AddLine($"It needs to match this {openingNatural} '", openingFragment, "'."));

			return diagnostic;
		}
		public Diagnostic ReportExpectedEndOfInput(IDiagnosticProvider provider, ISyntaxToken token)
		{
			return diagnostics
				.BuildError(provider, "expected_end_of_input")
				.Add(token, lines => lines.AddLine("Expected the end of the input."));
		}
		#endregion
	}
}
