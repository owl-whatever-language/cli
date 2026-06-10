namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Symbols;

public class LocalVariableSymbol : BaseSymbol<AbstractVariableDeclarationStatement>
{
	#region Properties
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
	public LocalVariableSymbol(string? name, AbstractVariableDeclarationStatement? declaration) : base(name, declaration)
	{
	}
	#endregion
}
