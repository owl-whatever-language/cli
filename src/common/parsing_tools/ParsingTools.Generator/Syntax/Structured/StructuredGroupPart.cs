namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

internal interface IStructuredGroupPart : IStructuredTreePart
{
	#region Properties
	StructuredGroupInfo? Group { get; }
	#endregion
}

internal abstract class BaseStructuredGroupPart : BaseStructuredTreePart, IStructuredGroupPart
{
	#region Properties
	public StructuredGroupInfo? Group { get; }
	#endregion

	#region Constructors
	protected BaseStructuredGroupPart(StructuredTreeInfo tree, StructuredGroupInfo? group) : base(tree) => Group = group;
	#endregion
}

internal static class IStructuredGroupPartExtensions
{
	extension(IStructuredGroupPart part)
	{
		#region Properties
		public Name Kind => part.Tree.Kind;
		#endregion
	}
}
