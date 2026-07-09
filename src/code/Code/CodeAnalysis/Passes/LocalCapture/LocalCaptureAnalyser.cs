namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.LocalCapture;

public sealed class LocalCaptureAnalyser : AnalysisPass.PerTree, IDiagnosticProvider
{
	#region Nested types
	private sealed class Annotator : BaseAnnotatedVisitor
	{
		#region Nested types
		private sealed class UsedLookup
		{
			#region Fields
			private readonly Dictionary<ILocalVariable, UsedVariableInfo> _lookup = [];
			#endregion

			#region Methods
			public IReadOnlyCollection<UsedVariableInfo> GetAll() => _lookup.Values.ToArray();
			public void AddUse(ILocalVariable variable, IAnnotatedGetExpressionSyntax get)
			{
				if (_lookup.TryGetValue(variable, out UsedVariableInfo? info) is false)
				{
					info = new(variable);
					_lookup.Add(variable, info);
				}

				info.Uses.Add(get);
			}
			public void Remove(ILocalVariable variable) => _lookup.Remove(variable);
			#endregion
		}
		#endregion

		#region Fields
		private readonly HashSet<IAnnotatedFunctionDeclarationStatementSyntax> _seen = [];
		private readonly Stack<UsedLookup> _used = [];
		private readonly Stack<HashSet<ILocalVariable>> _declared = [];
		#endregion

		#region Methods
		protected override bool Visit(IAnnotatedFunctionDeclarationStatementSyntax node)
		{
			if (node.IsLocal is false || _seen.Contains(node))
				return false;

			_seen.Add(node);

			UsedLookup used = new();
			HashSet<ILocalVariable> declared = [];

			_used.Push(used);
			_declared.Push(declared);

			VisitChildren(node);

			// Note(Nightowl): We only care about the external variables that were used;
			foreach (ILocalVariable variable in declared)
			{
				foreach (UsedLookup lookup in _used)
					lookup.Remove(variable);
			}

			IReadOnlyCollection<IUsedVariableInfo> all = used.GetAll();
			node.AddLocalCapture(all);

			_used.Pop();
			_declared.Pop();

			return false;
		}
		protected override bool Visit(IAnnotatedGetExpressionSyntax node)
		{
			if (node.Symbol is ILocalVariable variable)
			{
				foreach (UsedLookup lookup in _used)
					lookup.AddUse(variable, node);
			}

			return false;
		}

		protected override bool Visit(IAnnotatedVariableDeclarationStatementSyntax node)
		{
			if (_declared.TryPeek(out HashSet<ILocalVariable>? declared))
				declared.Add(node.Variable);

			return true;
		}
		#endregion
	}
	private sealed class Checker : BaseAnnotatedVisitor
	{
		#region Properties
		private LocalCaptureAnalyser Analyser { get; }
		public DiagnosticBag Diagnostics { get; } = [];
		#endregion

		#region Constructors
		public Checker(LocalCaptureAnalyser analyser)
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
	public string Name => "local_capture_analyser";
	public override string Kind => "local_capture";
	#endregion

	#region Methods
	protected override IDiagnosticBag Run(IAnalysisContext context, IAnnotatedSyntaxTree tree)
	{
		Annotator annotator = new();
		annotator.Visit(tree);

		Checker checker = new(this);
		checker.Visit(tree);

		return checker.Diagnostics;
	}
	#endregion
}
