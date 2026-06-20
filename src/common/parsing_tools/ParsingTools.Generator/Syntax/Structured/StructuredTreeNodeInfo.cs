namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

internal class StructuredTreeNodeInfo : BaseStructuredTreePart, IStructuredShadowedInfo<StructuredTreeNodeInfo>
{
	#region Properties
	public string Path => $"{Tree.Directory}/{Class.Name}.g.cs";
	public StructuredInterfaceInfo Interface { get; }
	public StructuredClassInfo Class { get; }
	public IReadOnlyList<StructuredMemberInfo> Members { get; }
	public StructuredTreeNodeInfo? Shadows { get; }
	public StructuredTreeNodeInfo? ShadowedBy { get; private set; }
	#endregion

	#region Constructors
	public StructuredTreeNodeInfo(
		StructuredInterfaceInfo @interface,
		StructuredClassInfo @class,
		IReadOnlyList<StructuredMemberInfo> members,
		StructuredTreeNodeInfo? shadows)
	{
		Interface = @interface;
		Class = @class;

		Members = members;

		Shadows = shadows;
		shadows?.ShadowedBy = this;
	}
	#endregion
}
