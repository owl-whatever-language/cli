namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.LocalCapture;

public class LocalCaptureAnnotator : AnalysisPass.PerTree
{
	#region Nested types
	private sealed class Instance : BaseAnnotatedVisitor
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
	#endregion

	#region Properties
	public override string Kind => "local_capture_annotator";
	#endregion

	#region Methods
	protected override IDiagnosticBag Run(IAnalysisContext context, IAnnotatedSyntaxTree tree)
	{
		Instance instance = new();
		instance.Visit(tree);

		return DiagnosticBag.Empty;
	}
	#endregion
}
