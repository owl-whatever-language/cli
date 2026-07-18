namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;

public abstract class SpecialType : INamedType
{
	#region Properties
	public string Id { get; } = SymbolHelpers.GetNewId();
	public abstract string Name { get; }
	protected virtual ClassificationKind Classification => ClassificationKind.Type;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string ISymbol.Name => Name ?? SymbolHelpers.ThrowSymbolWithoutNameException<string>();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyCollection<ITypeMember> IType.Members => [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyCollection<ITypeProperty> IType.Properties => [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyCollection<ITypeMethod> IType.Methods => [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyCollection<IBinaryOperator> IType.BinaryOperators => [];
	#endregion

	#region Constructors
	protected SpecialType() { }
	#endregion

	#region Methods
	public virtual bool CanAssignTo(IType target) => false;
	public virtual bool Equals([NotNullWhen(true)] IType? other) => ReferenceEquals(this, other);
	public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);
	public override int GetHashCode() => base.GetHashCode(); // Note(Nightowl): We want reference equality so this is ok;
	public override string ToString() => Name;
	public TextFragmentCollection GetDebugText() => [new(Name, Classification)];
	#endregion
}

public abstract class SpecialType<T> : SpecialType
	where T : notnull, SpecialType<T>, new()
{
	#region Properties
	public static T Instance { get; } = new();
	#endregion
}

public sealed class VoidType : SpecialType<VoidType>
{
	public override string Name => "void";
}

public sealed class UnknownType : SpecialType<UnknownType>
{
	#region Properties
	protected override ClassificationKind Classification => ClassificationKind.Error;
	public override string Name => "unknown";
	#endregion

	#region Methods
	public override bool Equals([NotNullWhen(true)] IType? other) => false;
	public override bool Equals([NotNullWhen(true)] object? obj) => false;
	public override int GetHashCode() => base.GetHashCode();
	#endregion
}

public sealed class ErrorType : SpecialType<ErrorType>
{
	#region Properties
	protected override ClassificationKind Classification => ClassificationKind.Error;
	public override string Name => "error";
	#endregion

	#region Methods
	public override bool Equals([NotNullWhen(true)] IType? other) => false;
	public override bool Equals([NotNullWhen(true)] object? obj) => false;
	public override int GetHashCode() => base.GetHashCode();
	#endregion
}

public static class SpecialTypes
{
	#region Properties
	public static ErrorType Error => ErrorType.Instance;
	public static UnknownType Unknown => UnknownType.Instance;
	public static VoidType Void => VoidType.Instance;
	#endregion
}
