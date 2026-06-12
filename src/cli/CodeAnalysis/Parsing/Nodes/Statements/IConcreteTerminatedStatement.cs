namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Nodes.Statements;

public interface IConcreteTerminatedStatement : IConcreteStatement
{
	#region Properties
	IConcreteSyntaxToken Terminator { get; }
	#endregion
}
