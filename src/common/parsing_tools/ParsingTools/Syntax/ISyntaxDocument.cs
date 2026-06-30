namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxDocument : ISyntaxNode
{
	#region Properties
	[DisallowNull]
	ISyntaxTree? Tree { get; set; }
	#endregion
}
