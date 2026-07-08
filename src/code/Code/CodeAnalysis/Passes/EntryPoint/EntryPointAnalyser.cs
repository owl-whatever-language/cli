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
			diagnostics
				.BuildSuggestion(this, "nothing_to_execute")
				.Add(lines =>
				{
					lines.AddLine("None of the files in the compilation contained executable statements.");
					lines.AddLine("This means that none of the files counted as an entry point.");
					lines.AddLine("Which also means that nothing will happen by trying to run the program.");
				});

			return diagnostics;
		}

		if (withExecutable.Count is 1)
			return diagnostics;

		Diagnostic diagnostic = diagnostics
			.BuildError(this, "multiple_entry_point_files")
			.Add(lines =>
			{
				lines.AddLine("The compilation contained several files with executable statements.");
				lines.AddLine("This means that several files were counted as possible entry points to the program, however only one is allowed.");
			});

		foreach (IAnnotatedSyntaxTree tree in withExecutable)
		{
			IAnnotatedStatementSyntax first = tree.Document.Statements.First(s => s.IsExecutable);
			ISyntaxNode target = first.Flatten().FirstOrDefault() ?? (ISyntaxNode)first;

			diagnostic.Add(target, lines => lines.AddLine("This is an executable statement."));
		}

		return diagnostics;
	}
	#endregion
}
