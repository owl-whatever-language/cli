using System.CommandLine.Help;

namespace OwlDomain.Owl.CLI.Actions.Meta;

public class CustomHelpAction : SynchronousCommandLineAction
{
	#region Fields
	private readonly HelpAction _defaultHelp;
	#endregion

	#region Constructors
	public CustomHelpAction(HelpAction defaultHelp) => _defaultHelp = defaultHelp;
	#endregion

	#region Methods
	public override int Invoke(ParseResult parseResult)
	{
		FigletText text = new("OWL");

		AnsiConsole.Write(text);
		AnsiConsole.WriteLine();

		int result = _defaultHelp.Invoke(parseResult);

		return result;
	}
	#endregion
}
