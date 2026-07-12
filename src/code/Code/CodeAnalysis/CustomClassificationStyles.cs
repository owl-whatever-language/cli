namespace OwlDomain.Owl.Code.CodeAnalysis;

public static class CustomClassificationStyles
{
	extension(ClassificationKind)
	{
		#region Properties
		public static ClassificationKind TypeProperty => ClassificationKind.Identifier + "type_property";
		#endregion
	}
}
