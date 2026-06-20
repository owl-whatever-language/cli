namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

internal sealed class StructuredInterfaceInfo : BaseTargetTypeInfo, IStructuredShadowedInfo<StructuredInterfaceInfo>
{
	#region Properties
	public StructuredInterfaceInfo? Shadows { get; }
	public StructuredInterfaceInfo? ShadowedBy { get; private set; }
	#endregion

	#region Constructors
	public StructuredInterfaceInfo(
		string name,
		IEnumerable<string?> inheritFrom,
		StructuredInterfaceInfo? shadows)
	: base(name, inheritFrom)
	{
		Shadows = shadows;
		shadows?.ShadowedBy = this;
	}
	#endregion
}
