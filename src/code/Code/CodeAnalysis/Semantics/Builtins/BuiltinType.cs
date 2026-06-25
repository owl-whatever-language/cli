namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Builtins;

public class BuiltinType : INamedType
{
	#region Properties
	public string Name { get; }
	#endregion

	#region Constructors
	public BuiltinType(string name) => Name = name;
	#endregion

	#region Methods
	public bool CanAssignTo(IType target) => Equals(target) || target == SpecialTypes.Void;
	public bool Equals(IType? other) => ReferenceEquals(this, other);
	#endregion
}
