namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract;

public sealed class AstCreationResult : BaseAstCreationResult<AbstractSyntaxTree, ConcreteSyntaxTree>
{
	#region Constructors
	public AstCreationResult(IDiagnosticBag diagnostics, TimeSpan duration, AbstractSyntaxTree tree) : base(diagnostics, duration, tree)
	{
	}
	#endregion
}

public sealed class AstCreator : BaseAstCreator<AbstractSyntaxTree, ConcreteSyntaxTree, AstCreationResult>
{
	#region Nested types
	private sealed class Instance : CreatorInstance
	{
		#region Constructors
		public Instance(IAstCreator converter, ConcreteSyntaxTree concrete) : base(converter, concrete)
		{
		}
		#endregion

		#region Methods
		protected override AstCreationResult CreateResult(TimeSpan duration, AbstractSyntaxTree tree) => new(Diagnostics, duration, tree);
		protected override AbstractSyntaxTree Convert(ConcreteSyntaxTree concrete)
		{
			AbstractDocumentSyntax root = Convert(concrete.Document);
			return new(concrete.Source, concrete, root);
		}

		private AbstractDocumentSyntax Convert(ConcreteDocumentSyntax concrete)
		{
			IReadOnlyList<IAbstractStatement> statements = Convert(concrete.Statements);
			return new(concrete, statements);
		}
		#endregion

		#region Statement methods
		private IReadOnlyList<IAbstractStatement> Convert(IReadOnlyList<IConcreteStatement> concrete)
		{
			IAbstractStatement[] statements = new IAbstractStatement[concrete.Count];

			for (int i = 0; i < concrete.Count; i++)
				statements[i] = Convert(concrete[i]);

			return statements;
		}
		private IAbstractStatement Convert(IConcreteStatement concrete)
		{
			return concrete switch
			{
				ConcreteExpressionStatement statement => Convert(statement),
				ConcreteVariableDeclarationStatement statement => Convert(statement),

				_ => ThrowHelper.ThrowInvalidOperationException<IAbstractStatement>($"Could not convert the concrete statement ({concrete.GetType()}).")
			};
		}
		private AbstractVariableDeclarationStatement Convert(ConcreteVariableDeclarationStatement concrete)
		{
			IAbstractExpression value = Convert(concrete.Value);
			return new(concrete, value);
		}
		private AbstractExpressionStatement Convert(ConcreteExpressionStatement concrete)
		{
			IAbstractExpression expression = Convert(concrete.Expression);
			return new(concrete, expression);
		}
		#endregion

		#region Expression methods
		private IAbstractExpression Convert(IConcreteExpression concrete)
		{
			return concrete switch
			{
				ConcreteLiteralExpression expression => Convert(expression),
				ConcreteAccessExpression expression => Convert(expression),
				ConcreteInvocationExpression expression => Convert(expression),

				_ => ThrowHelper.ThrowInvalidOperationException<IAbstractExpression>($"Could not convert the concrete expression ({concrete.GetType()}).")
			};
		}
		private AbstractLiteralExpression Convert(ConcreteLiteralExpression concrete) => new(concrete);
		private AbstractAccessExpression Convert(ConcreteAccessExpression concrete) => new(concrete);
		private AbstractInvocationExpression Convert(ConcreteInvocationExpression concrete)
		{
			IAbstractExpression expression = Convert(concrete.Expression);
			IAbstractExpression value = Convert(concrete.Value);

			return new(concrete, expression, value);
		}
		#endregion
	}
	#endregion

	#region Methods
	protected override CreatorInstance CreateInstance(ConcreteSyntaxTree concrete) => new Instance(this, concrete);
	#endregion
}

