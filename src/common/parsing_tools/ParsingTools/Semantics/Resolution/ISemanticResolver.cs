namespace OwlDomain.ParsingTools.Semantics.Resolution;

/// <summary>
/// 	Represents the final resolver for semantic information.
/// </summary>
public interface ISemanticResolver : IDiagnosticProvider
{
}

/// <summary>
/// 	Represents the final resolver for semantic information.
/// </summary>
/// <typeparam name="TInput">The type that represents the additional inputs to the resolver.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST) that will be used as input.</typeparam>
public interface ISemanticResolver<TInput, TAbstract> : ISemanticResolver
	where TInput : notnull, ISemanticResolutionInput
	where TAbstract : notnull, IAbstractSyntaxTree
{
	#region Methods
	/// <summary>Resolves the abstract syntax <paramref name="tree"/> (AST) into a semantic syntax tree (SST).</summary>
	/// <param name="additionalInputs">The additional inputs to the resolver.</param>
	/// <param name="tree">The abstract syntax tree (AST) to resolve.</param>
	/// <returns>The result of the semantic resolution.</returns>
	ISemanticResolutionResult Resolve(TInput additionalInputs, TAbstract tree);
	#endregion
}

/// <summary>
/// 	Represents the final resolver for semantic information.
/// </summary>
/// <typeparam name="TInput">The type that represents the additional inputs to the resolver.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST) that will be used as input.</typeparam>
/// <typeparam name="TSemantic">The type of the generated semantic syntax tree (SST).</typeparam>
public interface ISemanticResolver<TInput, TAbstract, TSemantic> : ISemanticResolver<TInput, TAbstract>
	where TInput : notnull, ISemanticResolutionInput
	where TAbstract : notnull, IAbstractSyntaxTree
	where TSemantic : notnull, ISemanticSyntaxTree<TAbstract>
{
	#region Methods
	/// <summary>Resolves the abstract syntax <paramref name="tree"/> (AST) into a semantic syntax tree (SST).</summary>
	/// <param name="additionalInputs">The additional inputs to the resolver.</param>
	/// <param name="tree">The abstract syntax tree (AST) to resolve.</param>
	/// <returns>The result of the semantic resolution.</returns>
	new ISemanticResolutionResult<TSemantic, TAbstract> Resolve(TInput additionalInputs, TAbstract tree);
	ISemanticResolutionResult ISemanticResolver<TInput, TAbstract>.Resolve(TInput additionalInputs, TAbstract tree) => Resolve(additionalInputs, tree);
	#endregion
}

/// <summary>
/// 	Represents the final resolver for semantic information.
/// </summary>
/// <typeparam name="TInput">The type that represents the additional inputs to the resolver.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST) that will be used as input.</typeparam>
/// <typeparam name="TSemantic">The type of the generated semantic syntax tree (SST).</typeparam>
/// <typeparam name="TResult">The type of the semantic resolution result.</typeparam>
public interface ISemanticResolver<TInput, TAbstract, TSemantic, TResult> : ISemanticResolver<TInput, TAbstract, TSemantic>
	where TInput : notnull, ISemanticResolutionInput
	where TAbstract : notnull, IAbstractSyntaxTree
	where TSemantic : notnull, ISemanticSyntaxTree<TAbstract>
	where TResult : notnull, ISemanticResolutionResult<TSemantic, TAbstract>
{
	#region Methods
	/// <summary>Resolves the abstract syntax <paramref name="tree"/> (AST) into a semantic syntax tree (SST).</summary>
	/// <param name="additionalInputs">The additional inputs to the resolver.</param>
	/// <param name="tree">The abstract syntax tree (AST) to resolve.</param>
	/// <returns>The result of the semantic resolution.</returns>
	new TResult Resolve(TInput additionalInputs, TAbstract tree);
	ISemanticResolutionResult<TSemantic, TAbstract> ISemanticResolver<TInput, TAbstract, TSemantic>.Resolve(
		TInput additionalInputs,
		TAbstract tree)
	{
		return Resolve(additionalInputs, tree);
	}
	#endregion
}

/// <summary>
/// 	Represents the base implementation for the final resolver for semantic information.
/// </summary>
/// <typeparam name="TInput">The type that represents the additional inputs to the resolver.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST) that will be used as input.</typeparam>
/// <typeparam name="TSemantic">The type of the generated semantic syntax tree (SST).</typeparam>
/// <typeparam name="TResult">The type of the semantic resolution result.</typeparam>
public abstract class BaseSemanticResolver<TInput, TAbstract, TSemantic, TResult> : ISemanticResolver<TInput, TAbstract, TSemantic, TResult>
	where TInput : notnull, ISemanticResolutionInput
	where TAbstract : notnull, IAbstractSyntaxTree
	where TSemantic : notnull, ISemanticSyntaxTree<TAbstract>
	where TResult : notnull, ISemanticResolutionResult<TSemantic, TAbstract>
{
	#region Nested types
	/// <summary>
	/// 	Represents the resolver instance that can be used for a single semantic resolution operation.
	/// </summary>
	protected abstract class ResolverInstance : StageInstance
	{
		#region Properties
		/// <summary>The additional inputs that were provided to the resolver.</summary>
		protected TInput AdditionalInputs { get; }

		/// <summary>The abstract syntax tree (AST) to resolve.</summary>
		protected TAbstract Tree { get; }

		/// <summary>The current symbol scope.</summary>
		protected ISymbolScope Symbols { get; }
		#endregion

		#region Constructors
		/// <summary>Populates the <see cref="ResolverInstance"/> properties.</summary>
		/// <param name="resolver">The resolver that created this instance.</param>
		/// <param name="additionalInputs">The additional inputs that were provided to the resolver.</param>
		/// <param name="tree">The abstract syntax tree (AST) to resolve.</param>
		protected ResolverInstance(ISemanticResolver resolver, TInput additionalInputs, TAbstract tree) : base(resolver)
		{
			AdditionalInputs = additionalInputs;
			Tree = tree;

			Symbols = additionalInputs.Symbols;
		}
		#endregion

		#region Methods
		/// <summary>Resolves the provided abstract syntax tree (AST).</summary>
		/// <returns>The result of the semantic resolution.</returns>
		public TResult Resolve()
		{
			Stopwatch watch = Stopwatch.StartNew();

			TSemantic semantic = Resolve(Tree);

			foreach (ISymbolTarget target in AdditionalInputs.Targets)
			{
				if (target.IsMutable)
					ThrowHelper.ThrowInvalidOperationException($"The symbol target ({target}) for the symbol ({target.Symbol.Name}) is still mutable.");
			}

			return CreateResult(watch.Elapsed, semantic);
		}

		/// <summary>Creates the semantic resolution result.</summary>
		/// <param name="duration">The amount of time that the semantic resolution operation took.</param>
		/// <param name="semantic">The generated semantic syntax tree (SST).</param>
		/// <returns></returns>
		protected abstract TResult CreateResult(TimeSpan duration, TSemantic semantic);

		/// <summary>Resolves the given abstract syntax <paramref name="tree"/> (AST).</summary>
		/// <param name="tree">The abstract syntax tree (AST) to resolve.</param>
		/// <returns>The generated semantic syntax tree (SST).</returns>
		protected abstract TSemantic Resolve(TAbstract tree);

		/// <summary>Tries to get a single unambiguous symbol target for the given <paramref name="name"/>.</summary>
		/// <typeparam name="TTarget">The type of the symbol target.</typeparam>
		/// <param name="name">The name of the symbol target.</param>
		/// <param name="expectedKind">The target kind of the expected <typeparamref name="TTarget"/> target.</param>
		/// <param name="position">The position to report the diagnostics at.</param>
		/// <returns>The found target, or <see langword="null"/>.</returns>
		protected TTarget? TryGetSymbol<TTarget>(string? name, string expectedKind, IndexedPositionRange position)
			where TTarget : class, ISymbolTarget
		{
			if (name is null)
				return null;

			if (Symbols.TryGet(name, out ISymbolGroup? group) is false)
			{
				ReportMissingSymbol(name, expectedKind, position);
				return default;
			}
			Debug.Assert(group.Count > 0);

			TTarget[] targets = group.Select(s => s.Target).OfType<TTarget>().ToArray();
			if (targets.Length is 1)
				return targets[0];

			if (targets.Length is 0)
			{
				ReportInvalidSymbolKind(name, group.Select(s => s.Target).ToArray(), expectedKind, position);
				return default;
			}

			ReportAmbiguousSymbolTargets(targets, position);
			return default;
		}

		/// <summary>Tries to get a single unambiguous symbol target for the given <paramref name="name"/>.</summary>
		/// <param name="name">The name of the symbol target.</param>
		/// <param name="position">The position to report the diagnostics at.</param>
		/// <returns>The found target, or <see langword="null"/>.</returns>
		protected ISymbolTarget? TryGetSymbol(string? name, IndexedPositionRange position) => TryGetSymbol<ISymbolTarget>(name, "value", position);

		/// <summary>Gets the symbol target that has been declared for the given <paramref name="node"/>.</summary>
		/// <typeparam name="TTarget">The type of the symbol target.</typeparam>
		/// <param name="node">The node that the symbol was declared for.</param>
		/// <returns>The declared symbol.</returns>
		/// <exception cref="InvalidOperationException">
		/// 	Thrown if the symbol declared for the given <paramref name="node"/>
		/// 	was not of the expected type <typeparamref name="TTarget"/>.
		/// </exception>
		/// <exception cref="ArgumentException">Thrown if no symbol was declared for the given <paramref name="node"/>.</exception>
		protected TTarget GetSymbol<TTarget>(IAbstractSyntaxNode node)
			where TTarget : notnull
		{
			if (Symbols.TryGet(node, out ISymbol? symbol))
			{
				if (symbol.Target is TTarget typed)
					return typed;

				ThrowHelper.ThrowInvalidOperationException($"The ({symbol}) symbol ({symbol.GetType()}) found for the given node ({node}) was not of the expected type ({typeof(TTarget)}).");
			}

			ThrowHelper.ThrowArgumentException(nameof(node), $"Could not find a symbol for the given node ({node}).");
			return default;
		}
		#endregion

		#region Diagnostic methods
		/// <summary>Reports that a symbol with the given <paramref name="name"/> is missing.</summary>
		/// <param name="name">The name of the symbol that was missing.</param>
		/// <param name="expectedKind">The kind of the expected target.</param>
		/// <param name="position">The position to report the diagnostic at.</param>
		protected abstract void ReportMissingSymbol(string name, string expectedKind, IndexedPositionRange position);

		/// <summary>Reports an ambiguity between the given <paramref name="targets"/>.</summary>
		/// <param name="targets">The symbol targets that were ambiguous.</param>
		/// <param name="position">The position to report the diagnostic at.</param>
		protected abstract void ReportAmbiguousSymbolTargets(IReadOnlyCollection<ISymbolTarget> targets, IndexedPositionRange position);

		/// <summary>Reports that a target with the <paramref name="expectedKind"/> was missing, but that <paramref name="targets"/> with other kinds did exist.</summary>
		/// <param name="name">The name of the symbol that was missing.</param>
		/// <param name="targets">The found targets that didn't match the <paramref name="expectedKind"/>.</param>
		/// <param name="expectedKind">The kind of the expected target.</param>
		/// <param name="position">The position to report hte diagnostic at.</param>
		protected abstract void ReportInvalidSymbolKind(string name, IReadOnlyCollection<ISymbolTarget> targets, string expectedKind, IndexedPositionRange position);
		#endregion
	}
	#endregion

	#region Properties
	/// <inheritdoc/>
	public string Name => "semantic_resolver";
	#endregion

	#region Methods
	/// <inheritdoc/>
	public TResult Resolve(TInput additionalInputs, TAbstract tree)
	{
		ResolverInstance resolver = CreateInstance(additionalInputs, tree);

		return resolver.Resolve();
	}

	/// <summary>Creates a new resolver instance.</summary>
	/// <param name="additionalInputs">The additional inputs that were provided to the resolver.</param>
	/// <param name="tree">The abstract syntax tree (AST) to resolve.</param>
	/// <returns>The resolver instance.</returns>
	protected abstract ResolverInstance CreateInstance(TInput additionalInputs, TAbstract tree);
	#endregion
}
