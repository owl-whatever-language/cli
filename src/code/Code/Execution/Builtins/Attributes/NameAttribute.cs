namespace OwlDomain.Owl.Code.Execution.Builtins.Attributes;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
internal sealed class NameAttribute(string name) : Attribute
{
	#region Properties
	public string Name { get; } = name;
	#endregion
}
