namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.LocalCapture;

public sealed class LocalCaptureAnalyser : AnalysisPass.PerTree, IDiagnosticProvider
{
	#region Nested types
	private sealed class Instance : BaseAnnotatedVisitor
	{
		#region Fields
		private readonly HashSet<IAnnotatedFunctionDeclarationStatementSyntax> _seen = [];
		private readonly Stack<Dictionary<ILocalVariable, List<IAnnotatedGetExpressionSyntax>>> _used = [];
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

			Dictionary<ILocalVariable, List<IAnnotatedGetExpressionSyntax>> used = [];
			HashSet<ILocalVariable> declared = [];

			_used.Push(used);
			_declared.Push(declared);

			VisitChildren(node);

			// Note(Nightowl): We only care about the external variables that were used;
			foreach (ILocalVariable variable in declared)
				used.Remove(variable);

			Dictionary<ILocalVariable, IReadOnlyCollection<IAnnotatedGetExpressionSyntax>> read = [];
			foreach (var pair in used)
				read.Add(pair.Key, pair.Value);

			node.AddLocalCapture(read);

			_used.Pop();
			_declared.Pop();

			return false;
		}

		protected override bool Visit(IAnnotatedGetExpressionSyntax node)
		{
			if (node.Symbol is ILocalVariable variable)
			{
				foreach (var use in _used)
				{
					if (use.TryGetValue(variable, out List<IAnnotatedGetExpressionSyntax>? uses) is false)
					{
						uses = [];
						use.Add(variable, uses);
					}

					uses.Add(node);
				}
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
			IReadOnlyDictionary<IDeclaredLocalVariable, IReadOnlyCollection<IAnnotatedGetExpressionSyntax>> GetInvalid()
			{
				Dictionary<IDeclaredLocalVariable, IReadOnlyCollection<IAnnotatedGetExpressionSyntax>> result = [];

				foreach (var variable in function.GetLocalCapture().Variables)
				{
					// Note(Nightowl): It shouldn't be possible to have non-declared ones;
					IDeclaredLocalVariable declared = (IDeclaredLocalVariable)variable.Key;

					IndexedLinePosition use = get.Name.Position.Start;
					IndexedLinePosition declaration = declared.Declaration.Name.Position.Start;

					if (declared.Declaration.Position.Start >= get.Position.Start)
						result.Add(declared, variable.Value);
				}

				return result;
			}

			var invalid = GetInvalid();
			if (invalid.Count is 0)
				return;

			Diagnostic diagnostic = Diagnostics
				.BuildError(Analyser, "variable_used_before_declaration")
				.Add(get.Name, lines => lines.AddLine("Some variables that this function uses have not been defined yet."));

			foreach (var variable in invalid)
			{
				diagnostic.Add(variable.Key.Declaration.Name, lines => lines.AddLine("Declared here."));

				foreach (var use in variable.Value)
					diagnostic.Add(use.Name, lines => lines.AddLine("Used here."));
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
