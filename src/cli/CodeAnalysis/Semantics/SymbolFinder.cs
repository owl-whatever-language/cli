namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics;

public sealed class SymbolFinder : BaseSymbolFinder<ConcreteSyntaxTree>
{
	#region Nested types
	private sealed class Instance : FinderInstance
	{
		#region Constructors
		public Instance(
			ISymbolFinder symbolFinder,
			ISymbolScope baseScope,
			IReadOnlyCollection<ConcreteSyntaxTree> trees)
			: base(symbolFinder, baseScope, trees)
		{
		}
		#endregion

		#region Methods
		protected override void Explore(ConcreteSyntaxTree tree)
		{
			foreach (IConcreteStatement statement in tree.Document.Statements.Values)
				Explore(statement);
		}
		#endregion

		#region Statement methods
		private void Explore(IConcreteStatement statement)
		{
			if (statement is ConcreteVariableDeclarationStatement variableDeclaration)
				Explore(variableDeclaration);
		}
		private void Explore(ConcreteVariableDeclarationStatement statement)
		{
			LocalVariableTarget target = new(statement.Name.Value as string);
			target.WithSymbol(statement.Name.Value as string, statement);

			Targets.Add(target);
			Scope.Add(target.Symbol);
		}
		#endregion
	}
	#endregion

	#region Methods
	protected override FinderInstance CreateInstance(ISymbolScope baseScope, IReadOnlyCollection<ConcreteSyntaxTree> trees)
	{
		return new Instance(this, baseScope, trees);
	}
	#endregion
}
