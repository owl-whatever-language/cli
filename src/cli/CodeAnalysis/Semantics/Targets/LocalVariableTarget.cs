namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Targets;

public interface ILocalVariableTarget : ISymbolTarget, ITypeTarget
{
	#region Properties
	public string? Name { get; }
	#endregion
}

public sealed class LocalVariableTarget : BaseSymbolTarget<ConcreteVariableDeclarationStatement>, ILocalVariableTarget
{
	#region Properties
	public override string Kind => "local variable";
	public string? Name
	{
		get;
		set => Set(ref field, value);
	}
	public ITypeInfo? Type
	{
		get;
		set => Set(ref field, value);
	}
	#endregion

	#region Constructors
	public LocalVariableTarget(string? name, ITypeInfo? type = null)
	{
		Name = name;
		Type = type;
	}
	#endregion

	#region Methods
	public override string ToString() => $"{Type?.ToString() ?? "???"} {Name ?? "???"}";
	#endregion
}
