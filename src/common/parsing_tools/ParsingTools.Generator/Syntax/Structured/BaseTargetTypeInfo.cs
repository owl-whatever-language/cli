
namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

internal interface ITargetTypeInfo
{
	#region Properties
	string Name { get; }
	IReadOnlyList<string> InheritFrom { get; }
	#endregion
}

internal class BaseTargetTypeInfo : ITargetTypeInfo
{
	#region Properties
	public string Name { get; }
	public IReadOnlyList<string> InheritFrom { get; }
	#endregion

	#region Constructors
	protected BaseTargetTypeInfo(string name, IEnumerable<string?> inheritFrom)
	{
		Name = name;
		InheritFrom = inheritFrom.Where(n => n is not null).ToArray()!;
	}
	#endregion
}
