namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Loops;

public interface ILoopLabel : ISymbol
{
}

public interface IDeclaredLoopLabel : IMutableDeclaredSymbol<IConcreteLoopLabelClauseSyntax>, ILoopLabel
{
}

public sealed class DeclaredLoopLabel : BaseDeclaredSymbol<IConcreteLoopLabelClauseSyntax>, IDeclaredLoopLabel
{
	#region Properties
	public override string? Name => Declaration.Name.Value as string;
	public override ClassificationKind Classification => ClassificationKind.Label;
	#endregion

	#region Constructors
	public DeclaredLoopLabel(IConcreteLoopLabelClauseSyntax declaration) : base(declaration)
	{
	}
	#endregion

	#region Methods
	public override TextFragmentCollection GetDebugText()
	{
		TextFragmentCollection fragments = [];

		fragments.Add(Name ?? "???", ClassificationKind.Label);

		return fragments;
	}
	#endregion
}
