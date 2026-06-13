namespace OwlDomain.Owl.CLI.Commands;

public interface IPerformanceReportSettings
{
	#region Properties
	[CommandOption("--report-performance")]
	[Description("Whether to create a report of the compiler's performance during the requested operation.")]
	[DefaultValue(false)]
	bool ReportPerformance { get; init; }
	#endregion
}
