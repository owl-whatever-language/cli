namespace OwlDomain.Owl.Config.CodeAnalysis.Classification;

public static class ClassificationExtensions
{
	extension(ClassificationKind)
	{
		#region Properties
		public static ClassificationKind Key => ClassificationKind.Identifier + "key";
		public static ClassificationKind Value => ClassificationKind.Identifier + "value";
		#endregion
	}
}
