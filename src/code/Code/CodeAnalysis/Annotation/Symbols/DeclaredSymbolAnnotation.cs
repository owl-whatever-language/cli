namespace OwlDomain.Owl.Code.CodeAnalysis.Annotation.Symbols;

public sealed class DeclaredSymbolAnnotation : CodeAnnotation
{
	#region Properties
	public override string Kind => "declared_symbol_annotation";
	public IDeclaredSymbol Symbol { get; }
	#endregion

	#region Constructors
	public DeclaredSymbolAnnotation(IDeclaredSymbol symbol)
	{
		Symbol = symbol;
	}
	#endregion
}

public static class DeclaredSymbolAnnotationExtensions
{
	extension(IAnnotatedSyntaxNode node)
	{
		#region Methods
		public void AddSymbolDeclaration(IDeclaredSymbol symbol)
		{
			DeclaredSymbolAnnotation annotation = new(symbol);
			node.Annotations.Add(annotation);
		}
		public IDeclaredSymbol GetDeclaredSymbol() => node.Annotations.Get<DeclaredSymbolAnnotation>().Symbol;
		#endregion
	}
	extension(ISyntaxNode node)
	{
		#region Methods
		public IDeclaredSymbol? TryGetDeclaredSymbol()
		{
			if (TryGetDeclaredSymbol(node, out IDeclaredSymbol? symbol))
				return symbol;

			return default;
		}
		public bool TryGetDeclaredSymbol([NotNullWhen(true)] out IDeclaredSymbol? symbol)
		{
			if (node.MostDetailed is not IAnnotatedSyntaxNode annotated)
			{
				symbol = default;
				return false;
			}

			if (annotated.Annotations.TryGet(out DeclaredSymbolAnnotation? annotation))
			{
				symbol = annotation.Symbol;
				return true;
			}

			symbol = default;
			return false;
		}
		#endregion
	}
}
