using System.CommandLine.Help;
using OwlDomain.Owl.CLI.Actions.List;
using OwlDomain.Owl.CLI.Actions.Meta;
using OwlDomain.Owl.CLI.Actions.Run;

RootCommand root = new("the OWL toolkit command line interface.");

root.CustomiseOption<VersionOption>(o =>
{
	o.Aliases.Add("-v");
	o.Description += ".";
	o.Action = new CustomVersionAction();
});

root.CustomiseOption<HelpOption>(o =>
{
	o.Description += ".";
	if (o.Action is HelpAction defaultHelp)
		o.Action = new CustomHelpAction(defaultHelp);
});

root.SetAction(result =>
{
	return root.Parse([.. args, "--help"]).Invoke();
});

root.Add(new GeneralRunCommand()
{
	new RunExample()
});

root.AddGroup(args, new Command("list", "General verb action for listing things, such as examples.")
{
	new ListExamples()
});

ParseResult result = root.Parse(args);
return result.Invoke();
