namespace OwlDomain.OWL.Code.Semantics.Types;

public static class SpecialTypes
{
	#region Properties
	public static INamedTypeInfo Void { get; } = new NamedTypeInfo("void").WithSymbol("void").Locked();
	public static INamedTypeInfo Text { get; } = new NamedTypeInfo("text").WithSymbol("text").Locked();
	#endregion
}
