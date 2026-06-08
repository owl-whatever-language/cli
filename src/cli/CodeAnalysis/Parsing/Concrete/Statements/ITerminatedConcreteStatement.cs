namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Concrete.Statements;

public interface ITerminatedConcreteStatement : IConcreteStatement
{
	#region Properties
	ITokenNode Terminator { get; }
	#endregion
}
