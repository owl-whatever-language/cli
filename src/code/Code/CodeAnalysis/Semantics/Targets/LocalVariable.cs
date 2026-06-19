namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Targets;

public interface ILocalVariable : INamedSymbolTarget
{
	#region Properties
	ITypeInfo? Type { get; set; }
	#endregion
}

public class LocalVariable : BaseNamedSymbolTarget, ILocalVariable
{
	#region Properties
	public override string Kind => "variable";
	public ITypeInfo? Type
	{
		get;
		set => Set(ref field, value);
	}
	#endregion

	#region Constructors
	public LocalVariable(string? name) : base(name) { }
	#endregion
}
