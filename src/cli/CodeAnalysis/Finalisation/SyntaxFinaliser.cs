namespace OwlDomain.Owl.CLI.CodeAnalysis.Finalisation;

public sealed class SyntaxFinalisationResult : BaseSyntaxFinalisationResult<FinalSyntaxTree>
{
	#region Constructors
	public SyntaxFinalisationResult(IDiagnosticBag diagnostics, TimeSpan duration, FinalSyntaxTree tree)
	: base(diagnostics, duration, tree)
	{ }
	#endregion
}

public sealed class SyntaxFinaliser : BaseSyntaxFinaliser<FinalSyntaxTree, SemanticSyntaxTree, SyntaxFinalisationResult>
{
	#region Nested types
	private sealed class Instance : FinaliserInstance
	{
		#region Constructors
		public Instance(ISyntaxFinaliser finaliser, SemanticSyntaxTree semantic) : base(finaliser, semantic) { }
		#endregion

		#region Methods
		protected override SyntaxFinalisationResult CreateResult(TimeSpan duration, FinalSyntaxTree tree) => new(Diagnostics, duration, tree);
		protected override FinalSyntaxTree Convert(SemanticSyntaxTree semantic)
		{
			FinalDocumentSyntax document = Convert(semantic.Document);
			return new(semantic.Source, document);
		}
		private FinalDocumentSyntax Convert(SemanticDocumentSyntax semantic)
		{
			IFinalSyntaxList<IFinalStatement> statements = Convert(semantic.Statements);
			IFinalSyntaxToken endOfInput = Convert(semantic.EndOfInput);

			return new(statements, endOfInput);
		}
		private IFinalSyntaxToken Convert(ISemanticSyntaxToken semantic) => new FinalSyntaxToken(semantic);
		#endregion

		#region Statement methods
		private IFinalSyntaxList<IFinalStatement> Convert(ISemanticSyntaxList<ISemanticStatement> semantic)
		{
			IFinalStatement[] statements = new IFinalStatement[semantic.Values.Count];

			for (int i = 0; i < statements.Length; i++)
				statements[i] = Convert(semantic.Values[i]);

			return new FinalSyntaxList<IFinalStatement>(statements);
		}
		private IFinalStatement Convert(ISemanticStatement semantic)
		{
			return semantic switch
			{
				SemanticVariableDeclarationStatement statement => Convert(statement),
				SemanticExpressionStatement statement => Convert(statement),

				_ => ThrowHelper.ThrowArgumentException<IFinalStatement>(nameof(semantic), $"Unable to resolve the semantic statement ({semantic.GetType()}).")
			};
		}
		private FinalVariableDeclarationStatement Convert(SemanticVariableDeclarationStatement semantic)
		{
			IFinalSyntaxToken typeName = Convert(semantic.TypeName);
			IFinalSyntaxToken name = Convert(semantic.Name);
			IFinalSyntaxToken assignment = Convert(semantic.Assignment);
			IFinalExpression value = Convert(semantic.Value);
			IFinalSyntaxToken terminator = Convert(semantic.Terminator);

			return new(typeName, name, assignment, value, terminator);
		}
		private FinalExpressionStatement Convert(SemanticExpressionStatement semantic)
		{
			IFinalExpression expression = Convert(semantic.Expression);
			IFinalSyntaxToken terminator = Convert(semantic.Terminator);

			return new(expression, terminator);
		}
		#endregion

		#region Expression methods
		private IFinalExpression Convert(ISemanticExpression semantic)
		{
			return semantic switch
			{
				SemanticAccessExpression expression => Convert(expression),
				SemanticLiteralExpression expression => Convert(expression),
				SemanticInvocationExpression expression => Convert(expression),

				_ => ThrowHelper.ThrowArgumentException<IFinalExpression>(nameof(semantic), $"Unable to resolve the semantic expression ({semantic.GetType()}).")
			};
		}
		private FinalAccessExpression Convert(SemanticAccessExpression semantic)
		{
			IFinalSyntaxToken name = Convert(semantic.Name);

			return new(name);
		}
		private FinalLiteralExpression Convert(SemanticLiteralExpression semantic)
		{
			IFinalSyntaxToken literal = Convert(semantic.Literal);

			return new(literal);
		}
		private FinalInvocationExpression Convert(SemanticInvocationExpression semantic)
		{
			IFinalExpression expression = Convert(semantic.Expression);
			IFinalSyntaxToken openingBracket = Convert(semantic.OpeningBracket);
			IFinalSeparatedSyntaxList<IFinalExpression, IFinalSyntaxToken> values = Convert(semantic.Values);
			IFinalSyntaxToken closingBracket = Convert(semantic.ClosingBracket);

			return new(expression, openingBracket, values, closingBracket);
		}
		private IFinalSeparatedSyntaxList<IFinalExpression, IFinalSyntaxToken> Convert(ISemanticSeparatedSyntaxList<ISemanticExpression, ISemanticSyntaxToken> expressions)
		{
			IFinalSyntaxNode[] nodes = new IFinalSyntaxNode[expressions.Nodes.Count];
			IFinalExpression[] values = new IFinalExpression[expressions.Values.Count];
			IFinalSyntaxToken[] separators = new IFinalSyntaxToken[expressions.Separators.Count];

			int resultIndex = 0;
			int separatorsIndex = 0;

			for (int i = 0; i < expressions.Nodes.Count; i++)
			{
				ISemanticSyntaxNode semantic = expressions.Nodes[i];
				if (semantic is ISemanticSyntaxToken token)
				{
					IFinalSyntaxToken final = Convert(token);
					nodes[i] = final;
					separators[separatorsIndex++] = final;
				}
				else if (semantic is ISemanticExpression expression)
				{
					IFinalExpression final = Convert(expression);
					nodes[i] = final;
					values[resultIndex++] = final;
				}
				else
					ThrowHelper.ThrowInvalidOperationException($"Unexpected node type ({semantic.GetType()}) in the separated list.");
			}

			return new FinalSeparatedSyntaxList<IFinalExpression, IFinalSyntaxToken>(nodes, values, separators);
		}
		#endregion
	}
	#endregion

	#region Methods
	protected override FinaliserInstance CreateInstance(SemanticSyntaxTree semantic) => new Instance(this, semantic);
	#endregion
}
