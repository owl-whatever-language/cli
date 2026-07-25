namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public static class SymbolExtensions
{
	extension(ISyntaxPart? part)
	{
		#region Properties
		public ISymbol? Symbol => (part as IDeclaredToken)?.Symbol;
		#endregion
	}
	extension(TextFragment fragment)
	{
		#region Properties
		public ISymbol? Symbol => fragment.Syntax.Symbol;
		#endregion
	}
	extension(ISymbolScope scope)
	{
		#region Properties
		public ICoreSymbolScope Core
		{
			get
			{
				ISymbolScope root = scope.Root;
				if (root is ICoreSymbolScope core)
					return core;

				ThrowHelper.ThrowInvalidOperationException($"The root scope ({root.Name}) was not the expected core scope.");
				return default;
			}
		}
		#endregion
	}
}

