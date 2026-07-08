namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.EntryPoint;

public sealed class EntryPointAnalyser : AnalysisPass.PerCompilation, IDiagnosticProvider
{
	#region Properties
	public string Name => "entry_point_analyser";
	public override string Kind => "entry_point";
	#endregion

	#region Methods
	protected override IDiagnosticBag RunCore(IAnalysisContext context)
	{
		DiagnosticBag diagnostics = [];

		IReadOnlyCollection<IAnnotatedSyntaxTree> trees = context.Annotated;
		IReadOnlyCollection<IAnnotatedSyntaxTree> withExecutable = trees.Where(t => t.HasExecutableStatements).ToArray();

		if (withExecutable.Count is 0)
		{
			diagnostics.Add(new Diagnostic()
			{
				Provider = this,
				Id = "nothing_to_execute",
				Kind = DiagnosticKind.Suggestion,

				Location = CompilationDiagnosticLocation.Instance,
				Message = "None of the files in the compilation contained executable statements, and so none of the files counted as entry point.",
			});

			return diagnostics;
		}

		if (withExecutable.Count is 1)
			return diagnostics;

		foreach (IAnnotatedSyntaxTree tree in withExecutable)
		{
			IAnnotatedStatementSyntax first = tree.Document.Statements.First(s => s.IsExecutable);
			diagnostics.AddError(
				this,
				"multiple_entry_points",
				tree.Source,
				new(first.Position.Start, first.Position.Start),
				$"The file '{tree.Source.SimpleName}' is one of the multiple entry points in the compilation, but only having one is allowed.");
		}

		return diagnostics;
	}
	#endregion
}
