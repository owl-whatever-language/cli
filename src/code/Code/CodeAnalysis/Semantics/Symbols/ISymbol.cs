namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public interface ISymbol : IDebugTreePrintable
{
	#region Properties
	/// <summary>The name of the symbol.</summary>
	/// <exception cref="InvalidOperationException">
	/// 	Thrown if the symbol didn't have a name.
	/// 	Symbols like this should be impossible to reference through the name, and should not be put into situations where the name
	/// 	might be accessed generally like this. Getting this exception is likely a result of a bug in OWL.
	/// </exception>
	string Name { get; }
	#endregion
}

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

	extension(ISymbol symbol)
	{
		#region Properties
		public bool IsKnown => symbol != SpecialSymbols.NotFound;
		public bool IsNotKnown => symbol == SpecialSymbols.NotFound;
		public ClassificationKind? Classification
		{
			get
			{
				return symbol switch
				{
					IFunction => ClassificationKind.Function,
					IFunctionParameter => ClassificationKind.Parameter,
					ILocalVariable => ClassificationKind.Variable,

					null => null,
					_ => ThrowHelper.ThrowInvalidOperationException<ClassificationKind?>($"Unhandled symbol type ({symbol.GetType().Name}).")
				};
			}
		}
		#endregion
	}
	extension(IReadOnlyCollection<ISymbol> symbols)
	{
		#region Methods
		public ClassificationKind? GetSharedClassification() => symbols.Select(get_Classification).Distinct().SingleOrDefault();
		#endregion
	}

	extension<T>(IReadOnlyCollection<T> symbols) where T : notnull, ISymbol
	{
		#region Methods
		public ClassificationKind? GetSharedClassification() => symbols.Select(s => s.Classification).Distinct().SingleOrDefault();
		#endregion
	}
}

public static class SymbolHelpers
{
	#region Functions
	[DoesNotReturn]
	public static void ThrowSymbolWithoutNameException()
	{
		ThrowHelper.ThrowInvalidOperationException("The symbol didn't have a name, and so it shouldn't have been able to be referenced by its name, or put into a situation where the name is required. This is likely a bug in OWL.");
	}

	[DoesNotReturn, MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T ThrowSymbolWithoutNameException<T>()
	{
		ThrowSymbolWithoutNameException();
		return default;
	}
	#endregion
}
