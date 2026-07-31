namespace OwlDomain.Owl.Packages.CodeAnalysis.Classification;

public static class ClassificationExtensions
{
	extension(ClassificationKind)
	{
		#region Properties
		public static ClassificationKind Key => ClassificationKind.Identifier + "key";
		#endregion
	}
}
