namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Symbols;

public sealed class TypeSymbol : BaseSymbol
{
	#region Properties
	[DisallowNull]
	public ITypeInfo? Type
	{
		get;
		set
		{
			if (field is not null)
				ThrowHelper.ThrowInvalidOperationException("The type was already set.");

			field = value;
		}
	}
	#endregion

	#region Constructors
	public TypeSymbol(string? name) : base(name)
	{
	}
	public TypeSymbol(ITypeInfo type) : this(type.Name)
	{
		Type = type;
	}
	#endregion
}
