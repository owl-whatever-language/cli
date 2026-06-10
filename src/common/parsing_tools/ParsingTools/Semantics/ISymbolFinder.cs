namespace OwlDomain.ParsingTools.Semantics;

/// <summary>
/// 	Represents the finder for symbols.
/// </summary>
public interface ISymbolFinder : IDiagnosticProvider
{
}

/// <summary>
/// 	Represents the finder for symbols.
/// </summary>
/// <typeparam name="TAbstract">The type of the abstract syntax trees (ASTs) that will be explored for symbols.</typeparam>
public interface ISymbolFinder<TAbstract> : ISymbolFinder
	where TAbstract : notnull, IAbstractSyntaxTree
{
	#region Methods
	/// <summary>Explores the given abstract syntax <paramref name="trees"/> (ASTs) to discover the defined symbols.</summary>
	/// <param name="baseScope">The base scope to use for the built-in and externally defined symbols.</param>
	/// <param name="trees">The abstract syntax trees (ASTs) to explore for the defined symbols.</param>
	/// <returns>The result of the symbol discovery.</returns>
	ISymbolDiscoveryResult Explore(ISymbolScope baseScope, IReadOnlyCollection<TAbstract> trees);
	#endregion
}

/// <summary>
/// 	Represents the base implementation for the finder for symbols.
/// </summary>
/// <typeparam name="TAbstract">The type of the abstract syntax trees (ASTs) that will be explored for symbols.</typeparam>
public abstract class BaseSymbolFinder<TAbstract> : ISymbolFinder<TAbstract>
	where TAbstract : notnull, IAbstractSyntaxTree
{
	#region Nested types
	/// <summary>
	/// 	Represents the instance that can perform a single symbol discovery operation.
	/// </summary>
	protected abstract class FinderInstance : StageInstance
	{
		#region Nested types
		/// <summary>Represents the scope during which the new symbol scope is available.</summary>
		/// <param name="finder">The finder instance.</param>
		protected readonly struct ScopeScope(FinderInstance finder) : IDisposable
		{
			#region Methods
			/// <inheritdoc/>
			public void Dispose() => finder.ExitScope();
			#endregion
		}
		#endregion

		#region Properties
		/// <summary>The base scope to use for built-in / externally defined symbols.</summary>
		protected ISymbolScope BaseScope { get; }

		/// <summary>The original scope that represents the result of the symbol discovery.</summary>
		protected ISymbolScope ResultScope { get; }

		/// <summary>The abstract syntax trees (ASTs) to explore for the defined symbols.</summary>
		protected IReadOnlyCollection<TAbstract> Trees { get; }

		/// <summary>The current scope for symbols.</summary>
		protected SymbolScope Scope { get; private set; }
		#endregion

		#region Constructors
		/// <summary>Populates the <see cref="FinderInstance"/> properties.</summary>
		/// <param name="symbolFinder">The symbol finder that created this instance.</param>
		/// <param name="baseScope">The base scope to use for built-in / externally defined symbols.</param>
		/// <param name="trees">The abstract syntax trees (ASTs) to explore for the defined symbols.</param>
		protected FinderInstance(
			ISymbolFinder symbolFinder,
			ISymbolScope baseScope,
			IReadOnlyCollection<TAbstract> trees)
			: base(symbolFinder)
		{
			BaseScope = baseScope;
			Trees = trees;

			Scope = new("global", baseScope);
			ResultScope = Scope;
		}
		#endregion

		#region Methods
		/// <summary>Explores the provided abstract syntax trees (ASTs) for the defined symbols.</summary>
		/// <returns>The result of the symbol discovery.</returns>
		public ISymbolDiscoveryResult Explore()
		{
			Stopwatch watch = Stopwatch.StartNew();

			Explore(Trees);

			return new SymbolDiscoveryResult(Diagnostics, watch.Elapsed, ResultScope);
		}

		/// <summary>Explores the given abstract syntax <paramref name="trees"/> (ASTs) for the defined symbols.</summary>
		/// <param name="trees">The abstract syntax trees (ASTs) to explore.</param>
		protected virtual void Explore(IReadOnlyCollection<TAbstract> trees)
		{
			foreach (TAbstract tree in trees)
				Explore(tree);
		}

		/// <summary>Explores the given abstract syntax <paramref name="tree"/> (AST) for the defined symbols.</summary>
		/// <param name="tree">The abstract syntax tree (AST) to explore.</param>
		protected abstract void Explore(TAbstract tree);


		/// <summary>Creates and enters a new scope.</summary>
		/// <param name="name">The name of the new scope, to help with debugging.</param>
		protected void EnterNewScope(string name) => Scope = new SymbolScope(name, Scope);

		/// <summary>Exits the current scope and returns to its parent.</summary>
		protected void ExitScope()
		{
			if (Scope == ResultScope)
				ThrowHelper.ThrowInvalidOperationException("Cannot exit the result scope.");

			Debug.Assert(Scope.Parent is not null);
			Scope = (SymbolScope)Scope.Parent;
		}

		/// <summary>Creates and enters a new scope until the returned value is disposed.</summary>
		/// <param name="name">The name of the new scope, to help with debugging.</param>
		/// <returns>The scope during which the new symbol scope will be available, dispose to exit the scope.</returns>
		protected ScopeScope NewScope(string name)
		{
			EnterNewScope(name);
			return new(this);
		}
		#endregion
	}
	#endregion

	#region Properties
	/// <inheritdoc/>
	public string Name => "symbol_finder";
	#endregion

	#region Methods
	/// <inheritdoc/>
	public ISymbolDiscoveryResult Explore(ISymbolScope baseScope, IReadOnlyCollection<TAbstract> trees)
	{
		FinderInstance finder = CreateInstance(baseScope, trees);

		return finder.Explore();
	}

	/// <summary>Creates a new instance of the finder.</summary>
	/// <param name="baseScope">The base scope to use for built-in / externally defined symbols.</param>
	/// <param name="trees">The abstract syntax trees (ASTs) to explore for the defined symbols.</param>
	/// <returns>The created symbol finder instance.</returns>
	protected abstract FinderInstance CreateInstance(ISymbolScope baseScope, IReadOnlyCollection<TAbstract> trees);
	#endregion
}
