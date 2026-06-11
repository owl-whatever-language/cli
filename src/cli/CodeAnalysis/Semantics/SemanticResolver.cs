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

			// Todo(Nightowl): Report type mismatch;

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
			ISymbol? symbol = FindSymbol(expression.Name.Value as string);
			ITypeInfo? type = GetType(symbol);

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
			ITypeInfo? resultType = null; // Nothing for now since function signatures are not a thing yet.

			// Todo(Nightowl): Report if the expression's type is not a function;

			return new(@abstract, resultType, expression, values, function);
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

