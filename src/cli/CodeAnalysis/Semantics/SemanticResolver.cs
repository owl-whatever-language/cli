namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics;

public sealed class SemanticResolutionResult : BaseSemanticResolutionResult<SemanticSyntaxTree, AbstractSyntaxTree>
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

public sealed class SemanticResolver : BaseSemanticResolver<SemanticResolutionInput, AbstractSyntaxTree, SemanticSyntaxTree, SemanticResolutionResult>
{
	#region Nested types
	private sealed class Instance : ResolverInstance
	{
		#region Constructors
		public Instance(ISemanticResolver resolver, SemanticResolutionInput additionalInputs, AbstractSyntaxTree tree) : base(resolver, additionalInputs, tree) { }
		#endregion

		#region Methods
		protected override SemanticResolutionResult CreateResult(TimeSpan duration, SemanticSyntaxTree semantic) => new(Diagnostics, duration, semantic);
		protected override SemanticSyntaxTree Resolve(AbstractSyntaxTree tree)
		{
			SemanticDocumentSyntax document = Resolve(tree.Document);
			return new(tree.Source, tree, document);
		}
		private SemanticDocumentSyntax Resolve(AbstractDocumentSyntax document)
		{
			ISemanticSyntaxList<ISemanticStatement> statements = Resolve(document.Statements);
			return new(document, statements);
		}
		#endregion

		#region Statement methods
		private ISemanticSyntaxList<ISemanticStatement> Resolve(IAbstractSyntaxList<IAbstractStatement> statements)
		{
			ISemanticStatement[] result = new ISemanticStatement[statements.Values.Count];

			for (int i = 0; i < statements.Values.Count; i++)
				result[i] = Resolve(statements.Values[i]);

			return new SemanticSyntaxList<ISemanticStatement, IAbstractStatement>(statements, result);
		}
		private ISemanticStatement Resolve(IAbstractStatement @abstract)
		{
			return @abstract switch
			{
				AbstractExpressionStatement statement => Resolve(statement),
				AbstractVariableDeclarationStatement statement => Resolve(statement),

				_ => ThrowHelper.ThrowArgumentException<ISemanticStatement>(nameof(@abstract), $"Unable to resolve the abstract statement ({@abstract.GetType()}).")
			};
		}
		private SemanticExpressionStatement Resolve(AbstractExpressionStatement statement)
		{
			ISemanticExpression expression = Resolve(statement.Expression);
			return new(statement, expression);
		}
		private SemanticVariableDeclarationStatement Resolve(AbstractVariableDeclarationStatement statement)
		{
			LocalVariableTarget variable = GetSymbol<LocalVariableTarget>(statement);
			variable.Type = TryGetSymbol<ITypeInfo>(statement.TypeName.Value as string, "type", statement.TypeName.Position);
			variable.Lock();

			SemanticSyntaxToken typeName = new(statement.TypeName, variable.Type?.Symbol);
			SemanticSyntaxToken name = new(statement.Name, variable.Symbol);

			ISemanticExpression value = Resolve(statement.Value);

			if (value.Type is not null && variable.Type is not null && (value.Type.CanBeAssignedTo(variable.Type) is false))
				ReportInvalidTargetType(value.Type, variable.Type, statement.TypeName.Position);

			return new(statement, typeName, name, value, variable);
		}
		#endregion

		#region Expression methods
		private ISemanticExpression Resolve(IAbstractExpression @abstract)
		{
			return @abstract switch
			{
				AbstractAccessExpression expression => Resolve(expression),
				AbstractLiteralExpression expression => Resolve(expression),
				AbstractInvocationExpression expression => Resolve(expression),

				_ => ThrowHelper.ThrowArgumentException<ISemanticExpression>(nameof(@abstract), $"Unable to resolve the abstract expression ({@abstract.GetType()}).")
			};
		}
		private SemanticAccessExpression Resolve(AbstractAccessExpression expression)
		{
			ISymbolTarget? target = TryGetSymbol(expression.Name.Value as string, expression.Name.Position);
			SemanticSyntaxToken name = new(expression.Name, target?.Symbol);
			ITypeInfo? type = ExtractType(target);

			return new(expression, name, type);
		}
		private SemanticLiteralExpression Resolve(AbstractLiteralExpression expression)
		{
			ITypeInfo? type = expression.Literal.Value switch
			{
				string => SpecialTypes.Text,
				_ => null,
			};

			return new(expression, type, expression.Literal.Value);
		}
		private SemanticInvocationExpression Resolve(AbstractInvocationExpression @abstract)
		{
			ISemanticExpression expression = Resolve(@abstract.Expression);
			ISemanticSyntaxList<ISemanticExpression> values = Resolve(@abstract.Values);
			IFunctionInfo? function = (expression.Type as FunctionType)?.Function;

			int? expectedCount = function?.Signature?.Parameters.Count;
			int actualCount = values.Values.Count;

			if (expression.Type is not FunctionType)
				ReportInvalidInvocationTarget(@abstract.Concrete.OpeningBracket.Position);
			else if (expectedCount is not null)
			{
				if (expectedCount != actualCount)
					ReportArgumentCountMismatch(expectedCount.Value, actualCount, @abstract.Concrete.OpeningBracket.Position);

				Debug.Assert(function?.Signature is not null);
				for (int i = 0; i < Math.Min(expectedCount.Value, actualCount); i++)
				{
					FunctionParameterSignature parameter = function.Signature.Parameters[i];
					ISemanticExpression argument = values.Values[i];

					if (argument.Type?.CanBeAssignedTo(parameter.Type) is false)
						ReportInvalidTargetType(argument.Type, parameter.Type, argument.Position);
				}
			}

			return new(@abstract, function?.Signature?.Return.Type, expression, values, function);
		}
		private ISemanticSyntaxList<ISemanticExpression> Resolve(IAbstractSyntaxList<IAbstractExpression> expressions)
		{
			ISemanticExpression[] result = new ISemanticExpression[expressions.Values.Count];

			for (int i = 0; i < expressions.Values.Count; i++)
				result[i] = Resolve(expressions.Values[i]);

			return new SemanticSyntaxList<ISemanticExpression, IAbstractExpression>(expressions, result);
		}
		#endregion

		#region Helpers
		private ITypeInfo? ExtractType(ISymbolTarget? target)
		{
			return target switch
			{
				ITypeInfo type => type,
				IFunctionInfo function => function.AsType,
				ILocalVariableTarget local => local.Type,

				_ => null,
			};
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
	protected override ResolverInstance CreateInstance(SemanticResolutionInput additionalInputs, AbstractSyntaxTree tree)
	{
		return new Instance(this, additionalInputs, tree);
	}
	#endregion
}

