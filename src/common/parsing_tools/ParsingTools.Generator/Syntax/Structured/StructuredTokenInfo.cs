namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

internal sealed class StructuredTokenInfo : BaseStructuredTreePart, IStructuredShadowedInfo<StructuredTokenInfo>
{
	#region Properties
	public string Path => $"{Tree.Directory}/{Class.Name}.g.cs";
	public StructuredInterfaceInfo Interface { get; }
	public StructuredClassInfo Class { get; }
	public IReadOnlyList<StructuredMemberInfo> Members { get; }
	public StructuredTokenInfo? Shadows { get; }
	public StructuredTokenInfo? ShadowedBy { get; private set; }
	public IEnumerable<StructuredMemberInfo> InterfaceMembers => Members;
	public IEnumerable<StructuredMemberInfo> ClassMembers => InterfaceMembers.Concat(Shadows?.ClassMembers ?? []);
	#endregion

	#region Constructors
	public StructuredTokenInfo(
		StructuredInterfaceInfo @interface,
		StructuredClassInfo @class,
		IReadOnlyList<StructuredMemberInfo> members,
		StructuredTokenInfo? shadows)
	{
		Interface = @interface;
		Class = @class;

		Members = members;

		Shadows = shadows;
		shadows?.ShadowedBy = this;
	}
	#endregion
}
