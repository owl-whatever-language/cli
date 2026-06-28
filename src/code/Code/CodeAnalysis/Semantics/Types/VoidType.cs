namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;

public sealed class VoidType : INamedType
{
	#region Properties
	public static VoidType Instance { get; } = new();
	public string? Name => "void";
	string ISymbol.Name => Name ?? SymbolHelpers.ThrowSymbolWithoutNameException<string>();
	#endregion

	#region Constructors
	private VoidType() { }
	#endregion

	#region Methods
	public bool CanAssignTo(IType target) => false;
	public bool Equals([NotNullWhen(true)] IType? other) => ReferenceEquals(this, other);
	public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);
	public override int GetHashCode() => base.GetHashCode(); // Note(Nightowl): We want reference equality so this is ok;
	public override string ToString() => "void";
	TextFragmentCollection IDebugTreePrintable.GetFragments() => [new("void", ClassificationKind.Type)];
	#endregion
}
