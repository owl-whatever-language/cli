namespace OwlDomain.ParsingTools.Semantics.Discovery;

/// <summary>
/// 	Represents the finder for symbols.
/// </summary>
public interface ISymbolFinder : IDiagnosticProvider
{
}

/// <summary>
/// 	Represents the finder for symbols.
/// </summary>
/// <typeparam name="TConcrete">The type of the concrete syntax trees (CSTs) that will be explored for symbols.</typeparam>
public interface ISymbolFinder<in TConcrete> : ISymbolFinder
	where TConcrete : notnull, IConcreteSyntaxTree
{
	#region Methods
	/// <summary>Explores the given concrete syntax <paramref name="trees"/> (CSTs) to discover the defined symbols.</summary>
	/// <param name="baseScope">The base scope to use for the built-in and externally defined symbols.</param>
	/// <param name="trees">The concrete syntax trees (CSTs) to explore for the defined symbols.</param>
	/// <returns>The result of the symbol discovery.</returns>
	ISymbolDiscoveryResult Explore(ISymbolScope baseScope, IReadOnlyCollection<TConcrete> trees);
	#endregion
}

/// <summary>
/// 	Represents the base implementation for the finder for symbols.
/// </summary>
/// <typeparam name="TConcrete">The type of the concrete syntax trees (CSTs) that will be explored for symbols.</typeparam>
public abstract class BaseSymbolFinder<TConcrete> : ISymbolFinder<TConcrete>
	where TConcrete : notnull, IConcreteSyntaxTree
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

		/// <summary>The concrete syntax trees (CSTs) to explore for the defined symbols.</summary>
		protected IReadOnlyCollection<TConcrete> Trees { get; }

		/// <summary>The current scope for symbols.</summary>
		protected SymbolScope Scope { get; private set; }

		/// <summary>The collection of the newly created symbol targets.</summary>
		protected List<ISymbolTarget> Targets { get; } = [];
		#endregion

		#region Constructors
		/// <summary>Populates the <see cref="FinderInstance"/> properties.</summary>
		/// <param name="symbolFinder">The symbol finder that created this instance.</param>
		/// <param name="baseScope">The base scope to use for built-in / externally defined symbols.</param>
		/// <param name="trees">The concrete syntax trees (CSTs) to explore for the defined symbols.</param>
		protected FinderInstance(
			ISymbolFinder symbolFinder,
			ISymbolScope baseScope,
			IReadOnlyCollection<TConcrete> trees)
			: base(symbolFinder)
		{
			BaseScope = baseScope;
			Trees = trees;

			Scope = new("global", baseScope);
			ResultScope = Scope;
		}
		#endregion

		#region Methods
		/// <summary>Explores the provided concrete syntax trees (CSTs) for the defined symbols.</summary>
		/// <returns>The result of the symbol discovery.</returns>
		public ISymbolDiscoveryResult Explore()
		{
			Stopwatch watch = Stopwatch.StartNew();

			Explore(Trees);

			foreach (ISymbolTarget target in Targets)
				_ = target.Symbol; // Note(Nightowl): Ensure very target has a symbol assigned;

			return new SymbolDiscoveryResult(Diagnostics, watch.Elapsed, ResultScope, Targets);
		}

		/// <summary>Explores the given concrete syntax <paramref name="trees"/> (CSTs) for the defined symbols.</summary>
		/// <param name="trees">The concrete syntax trees (CSTs) to explore.</param>
		protected virtual void Explore(IReadOnlyCollection<TConcrete> trees)
		{
			foreach (TConcrete tree in trees)
				Explore(tree);
		}

		/// <summary>Explores the given concrete syntax <paramref name="tree"/> (CST) for the defined symbols.</summary>
		/// <param name="tree">The concrete syntax tree (CST) to explore.</param>
		protected abstract void Explore(TConcrete tree);

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
	public ISymbolDiscoveryResult Explore(ISymbolScope baseScope, IReadOnlyCollection<TConcrete> trees)
	{
		FinderInstance finder = CreateInstance(baseScope, trees);

		return finder.Explore();
	}

	/// <summary>Creates a new instance of the finder.</summary>
	/// <param name="baseScope">The base scope to use for built-in / externally defined symbols.</param>
	/// <param name="trees">The concrete syntax trees (CSTs) to explore for the defined symbols.</param>
	/// <returns>The created symbol finder instance.</returns>
	protected abstract FinderInstance CreateInstance(ISymbolScope baseScope, IReadOnlyCollection<TConcrete> trees);
	#endregion
}
