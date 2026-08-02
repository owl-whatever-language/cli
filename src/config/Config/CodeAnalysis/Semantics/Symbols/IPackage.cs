namespace OwlDomain.Owl.Config.CodeAnalysis.Semantics.Symbols;

public interface IPackage : ISymbol
{
	#region Properties
	string PackageId { get; }
	#endregion
}

public interface IDeclaredPackage : IDeclaredSymbol<IConcretePackageDocumentUnitSyntax>, IPackage
{
	#region Properties
	new string PackageId { get; set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string IPackage.PackageId => PackageId;
	#endregion
}
