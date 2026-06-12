namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics;

public sealed class SemanticResolutionResult : BaseSemanticResolutionResult<SemanticSyntaxTree>
{
	#region Constructors
	public SemanticResolutionResult(IDiagnosticBag diagnostics, TimeSpan duration, SemanticSyntaxTree tree) : base(diagnostics, duration, tree) { }
	#endregion
}

public sealed class SemanticResolutionInput : BaseSemanticResolutionInput
{
	#region Constructors
	public SemanticResolutionInput(ISymbolScope symbols, IReadOnlyCollection<ISymbolTarget> targets) : base(symbols, targets) { }
	#endregion
}

public sealed class SemanticResolver : BaseSemanticResolver<SemanticResolutionInput, ConcreteSyntaxTree, SemanticSyntaxTree, SemanticResolutionResult>
{
	#region Nested types
	private sealed class Instance : ResolverInstance
	{
		#region Constructors
		public Instance(ISemanticResolver resolver, SemanticResolutionInput additionalInputs, ConcreteSyntaxTree tree) : base(resolver, additionalInputs, tree) { }
		#endregion

		#region Methods
		protected override SemanticResolutionResult CreateResult(TimeSpan duration, SemanticSyntaxTree semantic) => new(Diagnostics, duration, semantic);
		protected override SemanticSyntaxTree Resolve(ConcreteSyntaxTree tree)
		{
			SemanticDocumentSyntax document = Resolve(tree.Document);
			return new(tree.Source, document);
		}
		private SemanticDocumentSyntax Resolve(ConcreteDocumentSyntax document)
		{
			ISemanticSyntaxList<ISemanticStatement> statements = Resolve(document.Statements);
			ISemanticSyntaxToken endOfInput = Resolve(document.EndOfInput);

			return new(statements, endOfInput);
		}
		private ISemanticSyntaxToken Resolve(IConcreteSyntaxToken token, ISymbol? symbol = null)
		{
			if (symbol is null)
				return new SemanticSyntaxToken(token, symbol);

			List<ClassificationKind> classifications = [.. token.Classification];

			ISymbolTarget target = symbol.Target;

			if (target is ILocalVariableTarget)
				classifications.Add(ClassificationKind.Variable);

			if (target is IFunctionInfo)
				classifications.Add(ClassificationKind.Function);

			if (target is ITypeInfo)
				classifications.Add(ClassificationKind.Type);

			return new SemanticSyntaxToken(token, symbol, new ClassificationList(classifications));
		}
		#endregion

		#region Statement methods
		private ISemanticSyntaxList<ISemanticStatement> Resolve(IConcreteSyntaxList<IConcreteStatement> statements)
		{
			ISemanticStatement[] result = new ISemanticStatement[statements.Values.Count];

			for (int i = 0; i < statements.Values.Count; i++)
				result[i] = Resolve(statements.Values[i]);

			return new SemanticSyntaxList<ISemanticStatement>(result);
		}
		private ISemanticStatement Resolve(IConcreteStatement concrete)
		{
			return concrete switch
			{
				ConcreteExpressionStatement statement => Resolve(statement),
				ConcreteVariableDeclarationStatement statement => Resolve(statement),

				_ => ThrowHelper.ThrowArgumentException<ISemanticStatement>(nameof(concrete), $"Unable to resolve the concrete statement ({concrete.GetType()}).")
			};
		}
		private SemanticExpressionStatement Resolve(ConcreteExpressionStatement statement)
		{
			ISemanticExpression expression = Resolve(statement.Expression);
			ISemanticSyntaxToken terminator = Resolve(statement.Terminator);

			return new(expression, terminator);
		}
		private SemanticVariableDeclarationStatement Resolve(ConcreteVariableDeclarationStatement statement)
		{
			LocalVariableTarget variable = GetSymbol<LocalVariableTarget>(statement);
			variable.Type = TryGetSymbol<ITypeInfo>(statement.TypeName.Value as string, "type", statement.TypeName.Position);
			variable.Lock();

			ISemanticSyntaxToken typeName = Resolve(statement.TypeName, variable.Type?.Symbol);
			ISemanticSyntaxToken name = Resolve(statement.Name, variable.Symbol);
			ISemanticSyntaxToken assignment = Resolve(statement.Assignment);
			ISemanticExpression value = Resolve(statement.Value);
			ISemanticSyntaxToken terminator = Resolve(statement.Terminator);

			if (value.Type is not null && variable.Type is not null && (value.Type.CanBeAssignedTo(variable.Type) is false))
				ReportInvalidTargetType(value.Type, variable.Type, statement.TypeName.Position);

			return new(typeName, name, assignment, value, terminator);
		}
		#endregion

		#region Expression methods
		private ISemanticExpression Resolve(IConcreteExpression concrete)
		{
			return concrete switch
			{
				ConcreteAccessExpression expression => Resolve(expression),
				ConcreteLiteralExpression expression => Resolve(expression),
				ConcreteInvocationExpression expression => Resolve(expression),

				_ => ThrowHelper.ThrowArgumentException<ISemanticExpression>(nameof(concrete), $"Unable to resolve the concrete expression ({concrete.GetType()}).")
			};
		}
		private SemanticAccessExpression Resolve(ConcreteAccessExpression expression)
		{
			ISymbolTarget? target = TryGetSymbol(expression.Name.Value as string, expression.Name.Position);
			ISemanticSyntaxToken name = Resolve(expression.Name, target?.Symbol);

			return new(name);
		}
		private SemanticLiteralExpression Resolve(ConcreteLiteralExpression expression)
		{
			ISemanticSyntaxToken literal = Resolve(expression.Literal);

			return new(literal);
		}
		private SemanticInvocationExpression Resolve(ConcreteInvocationExpression concrete)
		{
			ISemanticExpression expression = Resolve(concrete.Expression);
			ISemanticSyntaxToken openingBracket = Resolve(concrete.OpeningBracket);
			ISemanticSeparatedSyntaxList<ISemanticExpression, ISemanticSyntaxToken> values = Resolve(concrete.Values);
			ISemanticSyntaxToken closingBracket = Resolve(concrete.ClosingBracket);

			SemanticInvocationExpression semantic = new(expression, openingBracket, values, closingBracket);

			int? expectedCount = semantic.Function?.Signature?.Parameters.Count;
			int actualCount = values.Values.Count;

			if (expression.Type is not FunctionType)
				ReportInvalidInvocationTarget(concrete.OpeningBracket.Position);
			else if (expectedCount is not null)
			{
				if (expectedCount != actualCount)
					ReportArgumentCountMismatch(expectedCount.Value, actualCount, concrete.OpeningBracket.Position);

				Debug.Assert(semantic.Function?.Signature is not null);
				for (int i = 0; i < Math.Min(expectedCount.Value, actualCount); i++)
				{
					FunctionParameterSignature parameter = semantic.Function.Signature.Parameters[i];
					ISemanticExpression argument = values.Values[i];

					if (argument.Type?.CanBeAssignedTo(parameter.Type) is false)
						ReportInvalidTargetType(argument.Type, parameter.Type, argument.Position);
				}
			}

			return semantic;
		}
		private ISemanticSeparatedSyntaxList<ISemanticExpression, ISemanticSyntaxToken> Resolve(IConcreteSeparatedSyntaxList<IConcreteExpression, IConcreteSyntaxToken> expressions)
		{
			ISemanticSyntaxNode[] nodes = new ISemanticSyntaxNode[expressions.Nodes.Count];
			ISemanticExpression[] values = new ISemanticExpression[expressions.Values.Count];
			ISemanticSyntaxToken[] separators = new ISemanticSyntaxToken[expressions.Separators.Count];

			int resultIndex = 0;
			int separatorsIndex = 0;

			for (int i = 0; i < expressions.Nodes.Count; i++)
			{
				IConcreteSyntaxNode concrete = expressions.Nodes[i];
				if (concrete is IConcreteSyntaxToken token)
				{
					ISemanticSyntaxToken semantic = Resolve(token);
					nodes[i] = semantic;
					separators[separatorsIndex++] = semantic;
				}
				else if (concrete is IConcreteExpression expression)
				{
					ISemanticExpression semantic = Resolve(expression);
					nodes[i] = semantic;
					values[resultIndex++] = semantic;
				}
				else
					ThrowHelper.ThrowInvalidOperationException($"Unexpected node type ({concrete.GetType()}) in the separated list.");
			}

			return new SemanticSeparatedSyntaxList<ISemanticExpression>(nodes, values, separators);
		}
		#endregion

		#region Diagnostic methods
		private void ReportInvalidTargetType(ITypeInfo source, ITypeInfo target, IndexedPositionRange position)
		{
			Diagnostics.Add(new Diagnostic()
			{
				Provider = DiagnosticProvider,
				Kind = DiagnosticKind.Error,
				Id = "invalid_target_type",

				Location = new DiagnosticSourceLocation(Tree.Source, position),
				Message = $"The type '{source}' cannot be converted to the target type '{target}'.",
			});
		}
		private void ReportInvalidInvocationTarget(IndexedPositionRange position)
		{
			Diagnostics.Add(new Diagnostic()
			{
				Provider = DiagnosticProvider,
				Kind = DiagnosticKind.Error,
				Id = "invalid_invocation_target",

				Location = new DiagnosticSourceLocation(Tree.Source, position),
				Message = $"The value is not a type that can be called.",
			});
		}
		private void ReportArgumentCountMismatch(int expected, int actual, IndexedPositionRange position)
		{
			Diagnostics.Add(new Diagnostic()
			{
				Provider = DiagnosticProvider,
				Kind = DiagnosticKind.Error,
				Id = "argument_count_mismatch",

				Location = new DiagnosticSourceLocation(Tree.Source, position),
				Message = $"The function expected {expected:n0} arguments but it received {actual:n0}.",
			});
		}
		protected override void ReportAmbiguousSymbolTargets(IReadOnlyCollection<ISymbolTarget> targets, IndexedPositionRange position)
		{
			string targetsText = string.Join(", ", targets.Select(t => $"\"{t}\""));

			Diagnostics.Add(new Diagnostic()
			{
				Provider = DiagnosticProvider,
				Kind = DiagnosticKind.Error,
				Id = "",

				Location = new DiagnosticSourceLocation(Tree.Source, position),
				Message = $"Couldn't differentiate between: {targetsText}.",
			});
		}
		protected override void ReportMissingSymbol(string name, string expectedKind, IndexedPositionRange position)
		{
			Diagnostics.Add(new Diagnostic()
			{
				Provider = DiagnosticProvider,
				Kind = DiagnosticKind.Error,
				Id = "missing_symbol",

				Location = new DiagnosticSourceLocation(Tree.Source, position),
				Message = $"There is no {expectedKind} with the name '{name}'.",
			});
		}
		protected override void ReportInvalidSymbolKind(string name, IReadOnlyCollection<ISymbolTarget> targets, string expectedKind, IndexedPositionRange position)
		{
			Guard.IsNotEmpty(targets);
			string[] otherKinds = targets.Select(t => t.Kind).Distinct().ToArray();

			Diagnostics.Add(new Diagnostic()
			{
				Provider = DiagnosticProvider,
				Kind = DiagnosticKind.Error,
				Id = "",

				Location = new DiagnosticSourceLocation(Tree.Source, position),
				Message = $"There is no {expectedKind} with the name '{name}', but could find: {string.Join(", ", otherKinds)}.",
			});
		}
		#endregion
	}
	#endregion

	#region Methods
	protected override ResolverInstance CreateInstance(SemanticResolutionInput additionalInputs, ConcreteSyntaxTree tree)
	{
		return new Instance(this, additionalInputs, tree);
	}
	#endregion
}

