namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics;

public static class DiagnosticExtensions
{
	extension(DiagnosticBag diagnostics)
	{
		#region Methods
		public Diagnostic ReportAmbiguity(IDiagnosticProvider provider, ISyntaxToken token, string kind, string kindPlural)
		{
			string? name = token.Value as string;
			Debug.Assert(name is not null);

			return diagnostics
				.BuildError(provider, $"{kind}_ambiguity")
				.Add(token, lines => lines.AddLine($"Multiple {kindPlural} named '{name}' were found, but they couldn't be disambiguated."));
		}
		public Diagnostic ReportNotFound(IDiagnosticProvider provider, ISyntaxToken token, string kind, int symbolCount, ISymbol? alternative)
		{
			string? name = token.Value as string;
			Debug.Assert(name is not null);

			string kindNatural = kind.Replace('_', ' ');

			Diagnostic diagnostic = diagnostics
				.BuildError(provider, $"{kind}_not_found")
				.Add(token, lines =>
				{
					if (alternative is not null && symbolCount is 0)
					{
						TextFragment fragment = new(alternative.Name, alternative.Classification ?? ClassificationKind.Identifier);
						lines.AddLine($"No accessible {kindNatural} named '{name}' could be found, did you mean to use '", fragment, "' instead?");
					}
					else
						lines.AddLine($"No accessible {kindNatural} named '{name}' could be found.");

					if (symbolCount is 1)
						lines.AddLine($"But a symbol with the same name was found.");
					else if (symbolCount > 1)
						lines.AddLine("But several symbols with the same name were found.");
				});

			if (alternative is not null && symbolCount is 0)
				diagnostic.TryAddDeclaration(alternative);

			return diagnostic;
		}
		public Diagnostic ReportDuplicate(IDiagnosticProvider provider, ISyntaxToken token, ISymbol symbol, ISymbolGroup alternative)
		{
			return diagnostics
				.BuildError(provider, "duplicate_symbol")
				.Add(token, lines =>
				{
					lines.AddLine($"A symbol named '{symbol.Name}' already exists in this scope.");
					if (symbol is IFunction && alternative.OfType<IFunction>().Any())
						lines.AddLine("function overloading is not yet supported.");
				})
				.TryAddDeclaration(alternative.FirstOrDefault());
		}
		#endregion
	}

	extension(Diagnostic diagnostic)
	{
		#region Methods
		public Diagnostic TryAddDeclaration(ISymbol? symbol)
		{
			ISyntaxNode? position = symbol switch
			{
				IDeclaredFunctionParameter parameter => parameter.Declaration,
				IDeclaredLocalVariable variable => variable.Declaration.Name,
				IDeclaredFunction function => function.Declaration.Signature,

				_ => null
			};

			if (position is not null)
			{
				Debug.Assert(symbol is not null);

				TextFragment fragment = new(symbol.Name, symbol.Classification ?? ClassificationKind.Identifier);
				diagnostic.Add(position, lines => lines.AddLine("This is where '", fragment, "' is declared."));
			}

			return diagnostic;
		}
		public Diagnostic TryAddDeclaration(ISemanticGetExpressionSyntax? get) => TryAddDeclaration(diagnostic, get?.Symbol);
		public Diagnostic TryAddDeclaration(ISemanticExpressionSyntax? expression)
		{
			return expression switch
			{
				ISemanticGetExpressionSyntax get => TryAddDeclaration(diagnostic, get),

				_ => diagnostic
			};
		}
		public Diagnostic TryAddDeclaration(ICallableTypeParameter? parameter)
		{
			return parameter switch
			{
				IDeclaredFunctionParameter declared => TryAddDeclaration(diagnostic, declared),

				_ => diagnostic,
			};
		}
		#endregion
	}
}
