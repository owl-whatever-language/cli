namespace OwlDomain.Owl.Code.CodeAnalysis.Syntax.Final;

partial interface IFinalSyntaxNode
{
	#region Properties
	ICodeAnnotations Annotations { get; }
	#endregion
}

partial class BaseFinalSyntaxNode
{
	#region Properties
	public ICodeAnnotations Annotations { get; } = new CodeAnnotations();
	#endregion
}

partial class FinalToken
{
	#region Properties
	public ICodeAnnotations Annotations { get; } = new CodeAnnotations();
	#endregion
}
