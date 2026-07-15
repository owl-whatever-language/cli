using System.CommandLine.Help;
using OwlDomain.Owl.CLI.Actions.Meta;

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
	return root.Parse("--help").Invoke();
});

ParseResult result = root.Parse(args);
return result.Invoke();
