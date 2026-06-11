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
	public SemanticResolutionInput(ISymbolScope symbols) : base(symbols) { }
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
			LocalVariableSymbol? symbol = FindSymbol<LocalVariableSymbol>(statement.Name.Value as string);
			symbol?.Type = GetType(FindSymbol(symbol.Declaration?.TypeName.Value as string));

			Debug.Assert(symbol is not null, "If the declaration exists then the symbol should've been found.");

			ITypeInfo? type = symbol.Type;
			ISemanticExpression value = Resolve(statement.Value);

			if (value.Type is not null && type is not null && (value.Type.CanBeAssignedTo(type) is false))
				ReportInvalidTargetType(value.Type, type, statement.TypeName.Position);

			return new(statement, type, symbol, value);
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
			string? name = expression.Name.Value as string;
			ISymbol? symbol = FindSymbol(name);
			ITypeInfo? type = GetType(symbol);

			if (symbol is null)
				ReportUnknownValueAccess(name, expression.Name.Position);

			return new(expression, symbol, type);
		}
		private SemanticLiteralExpression Resolve(AbstractLiteralExpression expression)
		{
			ITypeInfo? type = expression.Value switch
			{
				string => FindSymbol<TypeSymbol>("text")?.Type,
				_ => null,
			};

			return new(expression, type, expression.Value);
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

		#region Diagnostic methods
		private void ReportUnknownValueAccess(string? name, IndexedPositionRange position)
		{
			Diagnostics.Add(new Diagnostic()
			{
				Provider = DiagnosticProvider,
				Kind = DiagnosticKind.Error,
				Id = "unknown_value_access",

				Location = new DiagnosticSourceLocation(Tree.Source, position),
				Message = $"The value '{name}' doesn't exist.",
			});
		}
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
		#endregion

		#region Helpers
		private ITypeInfo? GetType(ISymbol? symbol)
		{
			return symbol switch
			{
				LocalVariableSymbol variable => variable.Type,
				TypeSymbol type => type.Type,
				FunctionSymbol function => function.Function?.AsType,

				_ => null,
			};
		}
		private T? FindSymbol<T>(string? name)
			where T : notnull, ISymbol
		{
			if (name is null)
				return default;

			if (Symbols.TryGet(name, out IReadOnlyList<ISymbol>? symbols))
				return symbols.OfType<T>().FirstOrDefault();

			return default;
		}
		private ISymbol? FindSymbol(string? name)
		{
			if (name is null)
				return null;

			if (Symbols.TryGet(name, out IReadOnlyList<ISymbol>? symbols))
				return symbols[0];

			return null;
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

