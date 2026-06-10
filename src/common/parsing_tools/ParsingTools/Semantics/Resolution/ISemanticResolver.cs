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
