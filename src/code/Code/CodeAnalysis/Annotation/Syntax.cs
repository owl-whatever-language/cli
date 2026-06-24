namespace OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated;

partial interface IAnnotatedSyntaxNode
{
	#region Properties
	ICodeAnnotations Annotations { get; }
	#endregion
}

partial class BaseAnnotatedSyntaxNode
{
	#region Properties
	public ICodeAnnotations Annotations { get; } = new CodeAnnotations();
	#endregion
}

partial class AnnotatedToken
{
	#region Properties
	public ICodeAnnotations Annotations { get; } = new CodeAnnotations();
	#endregion
}
