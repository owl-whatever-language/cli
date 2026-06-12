namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Statements;

public interface ISemanticTerminatedStatement : ISemanticStatement
{
	#region Properties
	ISemanticSyntaxToken Terminator { get; }
	#endregion
}
