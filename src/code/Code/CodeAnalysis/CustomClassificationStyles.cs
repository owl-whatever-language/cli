namespace OwlDomain.Owl.Code.CodeAnalysis;

public static class CustomClassificationStyles
{
	extension(ClassificationKind)
	{
		#region Properties
		public static ClassificationKind TypeMember => ClassificationKind.Identifier + "type_member";
		public static ClassificationKind TypeProperty => get_TypeMember() + "property";
		public static ClassificationKind TypeMethod => get_TypeMember() + "method";
		#endregion
	}
}
