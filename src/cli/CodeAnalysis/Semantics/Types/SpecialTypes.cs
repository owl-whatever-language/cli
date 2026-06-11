namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Types;

public static class SpecialTypes
{
	#region Properties
	public static ITypeInfo Void { get; } = new ImmutableTypeInfo("void");
	#endregion
}
