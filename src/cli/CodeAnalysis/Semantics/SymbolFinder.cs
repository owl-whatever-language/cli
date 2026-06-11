namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics;

public sealed class SymbolFinder : BaseSymbolFinder<AbstractSyntaxTree>
{
	#region Nested types
	private sealed class Instance : FinderInstance
	{
		#region Constructors
		public Instance(
			ISymbolFinder symbolFinder,
			ISymbolScope baseScope,
			IReadOnlyCollection<AbstractSyntaxTree> trees)
			: base(symbolFinder, baseScope, trees)
		{
		}
		#endregion

		#region Methods
		protected override void Explore(AbstractSyntaxTree tree)
		{
			foreach (IAbstractStatement statement in tree.Document.Statements.Values)
				Explore(statement);
		}
		#endregion

		#region Statement methods
		private void Explore(IAbstractStatement statement)
		{
			if (statement is AbstractVariableDeclarationStatement variableDeclaration)
				Explore(variableDeclaration);
		}
		private void Explore(AbstractVariableDeclarationStatement statement)
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
	protected override FinderInstance CreateInstance(ISymbolScope baseScope, IReadOnlyCollection<AbstractSyntaxTree> trees)
	{
		return new Instance(this, baseScope, trees);
	}
	#endregion
}
