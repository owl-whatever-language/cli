namespace OwlDomain.Owl.Code.CodeAnalysis.Diagnostics;

public static class OwlDiagnosticExtensions
{
	extension(IDiagnostic diagnostic)
	{
		#region Relevant node methods
		public ISyntaxNode? TryGetRelevantNode(AnalysisContext context)
		{
			return TryGetRelevantNode(diagnostic, context.Trees);
		}
		public ISyntaxNode? TryGetRelevantNode(IEnumerable<ISyntaxTree> trees)
		{
			PositionRange position = diagnostic.Position;
			ISyntaxTree? tree = trees.FirstOrDefault(p => p.Source == diagnostic.Source);

			if (position == default || tree is null)
				return null;

			ISyntaxPart? part = tree.Document.Search<ISyntaxPart>(p => p.Position.WithoutIndex.Contains(position.Start));
			if (part is null)
				return null;

			ISyntaxNode relevantNode =
				part.GetParent<IConcreteFunctionDeclarationStatementSyntax>() ??
				part.GetParent<IConcreteStatementSyntax>() ??
				part.GetParent<IConcreteExpressionSyntax>() ??
				(ISyntaxNode)part;

			return relevantNode;
		}
		#endregion
	}

	extension(IReadOnlyCollection<IDiagnostic> diagnostics)
	{
		#region Methods
		public IReadOnlyDictionary<ISyntaxNode, IDiagnosticBag> ByRelevantNode(AnalysisContext context)
		{
			return ByRelevantNode(diagnostics, context.Trees.ToArray());
		}
		public IReadOnlyDictionary<ISyntaxNode, IDiagnosticBag> ByRelevantNode(IReadOnlyCollection<ISyntaxTree> trees)
		{
			Dictionary<ISyntaxNode, DiagnosticBag> groups = [];

			foreach (IDiagnostic diagnostic in diagnostics)
			{
				ISyntaxNode? relevant = diagnostic.TryGetRelevantNode(trees);
				if (relevant is null)
					continue;

				if (groups.TryGetValue(relevant, out DiagnosticBag? bag) is false)
				{
					bag = [];
					groups.Add(relevant, bag);
				}

				bag.Add(diagnostic);
			}

			return groups.ToDictionary(pair => pair.Key, pair => (IDiagnosticBag)pair.Value);
		}
		#endregion
	}
	extension(IReadOnlyDictionary<ISyntaxNode, IDiagnosticBag> groups)
	{
		#region Methods
		public IReadOnlyDictionary<ISyntaxNode, IDiagnosticBag> Fold()
		{
			Dictionary<ISyntaxNode, DiagnosticBag> folded = [];

			// Seed
			foreach (KeyValuePair<ISyntaxNode, IDiagnosticBag> pair in groups)
				folded.Add(pair.Key, [.. pair.Value]);

			foreach (KeyValuePair<ISyntaxNode, DiagnosticBag> pair in folded)
			{
				ISyntaxNode? parent = pair.Key.Parent;
				while (parent is not null)
				{
					if (pair.Value.Count is 0)
						break;

					if (folded.TryGetValue(parent, out DiagnosticBag? other))
					{
						other.AddRange(pair.Value);
						pair.Value.Clear();
					}

					parent = parent.Parent;
				}
			}

			return folded
				.Where(p => p.Value.Count > 0)
				.ToDictionary(pair => pair.Key, pair => (IDiagnosticBag)pair.Value);
		}
		#endregion
	}
}
