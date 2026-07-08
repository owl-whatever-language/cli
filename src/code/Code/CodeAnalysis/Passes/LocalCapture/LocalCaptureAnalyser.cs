namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.LocalCapture;

public sealed class LocalCaptureAnalyser : AnalysisPass.PerTree, IDiagnosticProvider
{
	#region Nested types
	private sealed class Instance : BaseAnnotatedVisitor
	{
		#region Fields
		private readonly HashSet<IAnnotatedFunctionDeclarationStatementSyntax> _seen = [];
		private readonly Stack<HashSet<ILocalVariable>> _used = [];
		private readonly Stack<HashSet<ILocalVariable>> _declared = [];
		#endregion

		#region Properties
		private LocalCaptureAnalyser Analyser { get; }
		private ISourceFile Source { get; }
		public DiagnosticBag Diagnostics { get; } = [];
		#endregion

		#region Constructors
		public Instance(LocalCaptureAnalyser analyser, ISourceFile source)
		{
			Analyser = analyser;
			Source = source;
		}
		#endregion

		#region Methods
		protected override bool Visit(IAnnotatedFunctionDeclarationStatementSyntax node)
		{
			if (node.IsLocal is false || _seen.Contains(node))
				return false;

			_seen.Add(node);

			HashSet<ILocalVariable> used = [];
			HashSet<ILocalVariable> declared = [];

			_used.Push(used);
			_declared.Push(declared);

			VisitChildren(node);

			// Note(Nightowl): We only care about the external variables that were used;
			foreach (ILocalVariable variable in declared)
				used.Remove(variable);

			node.AddLocalCapture(used);

			_used.Pop();
			_declared.Pop();

			return false;
		}

		protected override bool Visit(IAnnotatedGetExpressionSyntax node)
		{
			if (node.Symbol is ILocalVariable variable)
			{
				foreach (HashSet<ILocalVariable> set in _used)
					set.Add(variable);
			}

			if (node.Symbol is IDeclaredFunction function)
			{
				IAnnotatedFunctionDeclarationStatementSyntax declaration = (IAnnotatedFunctionDeclarationStatementSyntax)function.Declaration;
				Visit(declaration);

				CheckUse(node, declaration);
			}

			return false;
		}
		protected override bool Visit(IAnnotatedVariableDeclarationStatementSyntax node)
		{
			if (_declared.TryPeek(out HashSet<ILocalVariable>? declared))
				declared.Add(node.Variable);

			return true;
		}
		private void CheckUse(IAnnotatedGetExpressionSyntax get, IAnnotatedFunctionDeclarationStatementSyntax function)
		{
			foreach (ILocalVariable variable in function.GetLocalCapture().Variables)
			{
				// Note(Nightowl): It shouldn't be possible to have non-declared ones;
				IDeclaredLocalVariable declared = (IDeclaredLocalVariable)variable;

				IndexedLinePosition use = get.Name.Position.Start;
				IndexedLinePosition declaration = declared.Declaration.Name.Position.Start;

				if (declared.Declaration.Position.Start >= get.Position.Start)
				{
					Diagnostics.AddError(
						Analyser,
						"function_undeclared_variable_use",
						Source,
						get.Name.Position,
						$"This function uses the variable '{variable.Name}' (declared on {declaration}) which is declared after this function is used.");

					Diagnostics.AddError(
						Analyser,
						"variable_defined_after_use",
						Source,
						declared.Declaration.Name.Position,
						$"This variable is declared after the function '{function.Function.Name}' (which uses this variable) is used on {use}.");
				}
			}
		}
		#endregion
	}
	#endregion

	#region Properties
	public string Name => "local_capture_analyser";
	public override string Kind => "local_capture";
	#endregion

	#region Methods
	protected override IDiagnosticBag Run(IAnalysisContext context, IAnnotatedSyntaxTree tree)
	{
		Instance instance = new(this, tree.Source);
		instance.Visit(tree);

		return instance.Diagnostics;
	}
	#endregion
}
