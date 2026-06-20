namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

public enum ClassType
{
	Regular,
	Sealed,
	Abstract,
}

internal sealed class StructuredClassInfo : BaseTargetTypeInfo, IStructuredShadowedInfo<StructuredClassInfo>
{
	#region Properties
	public ClassType Type { get; }
	public StructuredClassInfo? Shadows { get; }
	public StructuredClassInfo? ShadowedBy { get; private set; }
	#endregion

	#region Constructors
	public StructuredClassInfo(
		string name,
		ClassType type,
		IEnumerable<string?> inheritFrom,
		StructuredClassInfo? shadows)
	: base(name, inheritFrom)
	{
		Type = type;

		Shadows = shadows;
		shadows?.ShadowedBy = this;
	}
	#endregion
}
