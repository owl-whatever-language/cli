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
