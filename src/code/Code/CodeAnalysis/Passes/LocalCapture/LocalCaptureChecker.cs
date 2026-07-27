namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.LocalCapture;

public sealed class LocalCaptureChecker : AnalysisPass.PerTree, IDiagnosticProvider
{
	#region Nested types
	private sealed class Instance : BaseAnnotatedVisitor
	{
		#region Properties
		private LocalCaptureChecker Analyser { get; }
		public DiagnosticBag Diagnostics { get; } = [];
		#endregion

		#region Constructors
		public Instance(LocalCaptureChecker analyser)
		{
			Analyser = analyser;
		}
		#endregion

		#region Methods
		protected override bool Visit(IAnnotatedGetExpressionSyntax node)
		{
			if (node.Symbol is IDeclaredFunction function)
			{
				IAnnotatedFunctionDeclarationStatementSyntax declaration = (IAnnotatedFunctionDeclarationStatementSyntax)function.Declaration;
				Visit(declaration);

				CheckUse(node, declaration);
			}

			return false;
		}

		private void CheckUse(IAnnotatedGetExpressionSyntax get, IAnnotatedFunctionDeclarationStatementSyntax function)
		{
			IReadOnlyCollection<IUsedVariableInfo> GetInvalid()
			{
				List<IUsedVariableInfo> result = [];

				foreach (var variable in function.GetLocalCapture().Variables)
				{
					// Note(Nightowl): It shouldn't be possible to have non-declared ones;
					IDeclaredLocalVariable declared = (IDeclaredLocalVariable)variable.Variable;

					IndexedLinePosition use = get.Name.Position.Start;
					IndexedLinePosition declaration = declared.Declaration.Name.Position.Start;

					if (declared.Declaration.Position.Start >= get.Position.Start)
						result.Add(variable);
				}

				return result;
			}

			var invalid = GetInvalid();
			if (invalid.Count is 0)
				return;

			bool isCall = get.Parent is IConcreteFunctionCallExpressionSyntax;

			Diagnostic diagnostic = Diagnostics
				.BuildError(Analyser, "variable_used_before_declaration")
				.Add(get.Name, lines =>
				{
					string call = isCall ? "call" : "access";
					string called = isCall ? "called" : "accessed";

					lines.AddLine($"The {called} function uses some variables that haven't been declared before this {call}.");
				});

			foreach (IUsedVariableInfo variable in invalid)
			{
				IDeclaredLocalVariable declared = (IDeclaredLocalVariable)variable.Variable;

				string? name = declared.Declaration.Name.Value as string;
				Debug.Assert(name is not null, "A variable without a name can't be referenced.");
				TextFragment nameFragment = new(name, ClassificationKind.Variable);

				diagnostic.Add(declared.Declaration.Name, lines => lines.AddLine("This is where '", nameFragment, "' is declared, after the function is used."));

				foreach (var use in variable.Uses)
					diagnostic.Add(use.Name, lines => lines.AddLine("This is where '", nameFragment, "' is used in the function."));
			}
		}
		#endregion
	}
	#endregion

	#region Properties
	public string Name => "local_capture_checker";
	public override string Kind => "local_capture";
	#endregion

	#region Methods
	protected override IDiagnosticBag Run(IAnalysisContext context, IAnnotatedSyntaxTree tree)
	{
		Instance instance = new(this);
		instance.Visit(tree);

		return instance.Diagnostics;
	}
	#endregion
}
