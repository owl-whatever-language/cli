namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;

public static class SpecialTypes
{
	#region Properties
	public static INamedTypeInfo Void { get; } = new NamedTypeInfo("void").WithSymbol("void").Lock();
	public static INamedTypeInfo Text { get; } = new NamedTypeInfo("text").WithSymbol("text").Lock();
	#endregion

	#region Methods
	public static IEnumerable<INamedTypeInfo> GetAll() => [Void, Text];
	#endregion
}
