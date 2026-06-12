namespace OwlDomain.Owl.CLI.CodeAnalysis.Finalisation.Nodes.Statements;

public interface IFinalTerminatedStatement : IFinalStatement
{
	#region Properties
	IFinalSyntaxToken Terminator { get; }
	#endregion
}
