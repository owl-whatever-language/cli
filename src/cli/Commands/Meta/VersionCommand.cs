namespace OwlDomain.Owl.CLI.Commands.Meta;

[DisplayName("version")]
[Description("Provides more detailed version information than --version")]
public sealed class VersionCommand : Command
{
	#region Methods
	protected override int Execute(CommandContext context, CancellationToken cancellationToken)
	{
		AnsiConsole.MarkupLineInterpolated($"[gray]Version[/]: {GitInfo.Version ?? "<missing>"}");

		if (GitInfo.IsAvailable is false)
		{
			AnsiConsole.WriteLine();
			AnsiConsole.MarkupLine("[italic bold]More detailed git information is unavailable.[/]");
			return 0;
		}

		if (GitInfo.Branch.IsWhiteSpace() is false)
			AnsiConsole.MarkupLineInterpolated($"[gray]Branch[/]: {GitInfo.Branch}");

		if (GitInfo.HashShort.IsWhiteSpace() is false)
		{
			if (GitInfo.HasChanges)
				AnsiConsole.MarkupLineInterpolated($"[gray]Hash[/]: {GitInfo.HashShort} [italic gray](with local changes)[/]");
			else
				AnsiConsole.MarkupLineInterpolated($"[gray]Hash[/]: {GitInfo.HashShort}");
		}

		if (GitInfo.Date is not null)
		{
			string relative = GetRelativeDate(GitInfo.Date.Value);
			AnsiConsole.MarkupLineInterpolated($"[gray]Date[/]: {GitInfo.Date.Value.ToString()} [italic gray]({relative})[/]");
		}

		bool hasSubject = GitInfo.Subject?.IsWhiteSpace() is false;
		bool hasBody = GitInfo.Body?.IsWhiteSpace() is false;

		if (hasSubject || hasBody)
		{
			List<Text> parts = [];

			if (hasSubject)
			{
				Debug.Assert(GitInfo.Subject is not null);
				parts.Add(new Text(GitInfo.Subject));
			}

			if (hasSubject && hasBody)
				parts.Add(Text.Empty);

			if (hasBody)
			{
				Debug.Assert(GitInfo.Body is not null);
				parts.Add(new Text(GitInfo.Body));
			}

			Panel panel = new Panel(new Rows(parts)).Header("Message");
			AnsiConsole.WriteLine();
			AnsiConsole.Write(panel);
		}

		return 0;
	}
	#endregion

	#region Helpers
	private static string GetRelativeDate(DateTimeOffset date)
	{
		TimeSpan since = DateTimeOffset.Now.Subtract(date);
		if (since.TotalDays > 28)
			return "a while ago";

		if (since.TotalDays >= 2)
			return $"{since.TotalDays:n0} days ago";

		if (since.TotalHours >= 2)
			return $"{since.TotalHours:n0} hours ago";

		return "recently";
	}
	#endregion
}
