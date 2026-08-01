namespace OwlDomain.Owl.Config.CodeAnalysis.Parsing;

public static class SyntaxKindExtensions
{
	#region Fields
	private static readonly IReadOnlyCollection<SyntaxKind> AllKeywords =
	[
	];
	#endregion

	extension(SyntaxKind)
	{
		#region Properties
		public static IReadOnlyCollection<SyntaxKind> AllKeywords => AllKeywords;
		#endregion
	}
}
