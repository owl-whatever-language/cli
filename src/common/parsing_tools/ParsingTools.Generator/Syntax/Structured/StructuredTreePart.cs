namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

internal interface IStructuredTreePart
{
	#region Properties
	StructuredTreeInfo Tree { get; }
	#endregion
}

internal abstract class BaseStructuredTreePart : IStructuredTreePart
{
	#region Properties
	public StructuredTreeInfo Tree
	{
		get => field ?? throw new InvalidOperationException("The tree hasn't been set yet.");
		set;
	}
	#endregion

	#region Constructors
	protected BaseStructuredTreePart() { }
	protected BaseStructuredTreePart(StructuredTreeInfo tree) => Tree = tree;
	#endregion
}

internal static class IStructuredTreePartExtensions
{
	extension(IStructuredTreePart part)
	{
		#region Properties
		public Name Kind => part.Tree.Kind;
		#endregion
	}
}
