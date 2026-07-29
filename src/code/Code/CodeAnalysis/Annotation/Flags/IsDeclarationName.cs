namespace OwlDomain.Owl.Code.CodeAnalysis.Annotation.Flags;

partial class AnnotationFlag
{
	#region Constants
	public const string IsDeclarationName = "is_declaration_name";
	#endregion
}

static partial class FlagGroupAnnotationExtensions
{
	extension(ISyntaxToken token)
	{
		#region Methods
		public bool IsDeclarationName()
		{
			if (token is IAnnotatedToken annotated)
				return annotated.GetFlag(AnnotationFlag.IsDeclarationName, false);
			else if (token.ShadowedBy is not null)
				return IsDeclarationName(token.ShadowedBy);

			return false;
		}
		#endregion
	}
	extension(IAnnotatedToken token)
	{
		#region Methods
		public void MarkAsDeclarationName() => token.SetFlag(AnnotationFlag.IsDeclarationName);
		#endregion
	}
}
