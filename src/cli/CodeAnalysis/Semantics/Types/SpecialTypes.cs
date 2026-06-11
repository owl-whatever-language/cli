namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Types;

public static class SpecialTypes
{
	#region Properties
	public static ITypeInfo Void { get; } = new TypeInfo("void").WithSymbol("void").Locked();
	public static ITypeInfo Text { get; } = new TypeInfo("text").WithSymbol("text").Locked();
	#endregion
}
