namespace OwlDomain.ParsingTools.Finalisation.Syntax;

/// <summary>
/// 	Represents a finaliser for syntax trees.
/// </summary>
public interface ISyntaxFinaliser : IDiagnosticProvider
{
}

/// <summary>
/// 	Represents a finaliser for syntax trees.
/// </summary>
/// <typeparam name="TFinal">The type of the final syntax tree (FST).</typeparam>
/// <typeparam name="TSemantic">The type of the semantic syntax tree (SST).</typeparam>
public interface ISyntaxFinaliser<out TFinal, TSemantic> : ISyntaxFinaliser
	where TFinal : notnull, IFinalSyntaxTree<TSemantic>
	where TSemantic : notnull, ISemanticSyntaxTree
{
	#region Methods
	/// <summary>Finalises the given <paramref name="semantic"/> syntax tree (SST).</summary>
	/// <param name="semantic">The semantic syntax tree (SST) to finalise.</param>
	/// <returns>The result of finalising the given <paramref name="semantic"/> syntax tree (SST).</returns>
	ISyntaxFinalisationResult<TFinal, TSemantic> Finalise(TSemantic semantic);
	#endregion
}

/// <summary>
/// 	Represents a finaliser for syntax trees.
/// </summary>
/// <typeparam name="TFinal">The type of the final syntax tree (FST).</typeparam>
/// <typeparam name="TSemantic">The type of the semantic syntax tree (SST).</typeparam>
/// <typeparam name="TResult">The type of the syntax finalisation result.</typeparam>
public interface ISyntaxFinaliser<out TFinal, TSemantic, out TResult> : ISyntaxFinaliser<TFinal, TSemantic>
	where TFinal : notnull, IFinalSyntaxTree<TSemantic>
	where TSemantic : notnull, ISemanticSyntaxTree
	where TResult : notnull, ISyntaxFinalisationResult<TFinal, TSemantic>
{
	#region Methods
	/// <summary>Finalises the given <paramref name="semantic"/> syntax tree (SST).</summary>
	/// <param name="semantic">The semantic syntax tree (SST) to finalise.</param>
	/// <returns>The result of finalising the given <paramref name="semantic"/> syntax tree (SST).</returns>
	new TResult Finalise(TSemantic semantic);
	ISyntaxFinalisationResult<TFinal, TSemantic> ISyntaxFinaliser<TFinal, TSemantic>.Finalise(TSemantic semantic) => Finalise(semantic);
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a syntax tree finaliser.
/// </summary>
/// <typeparam name="TFinal">The type of the final syntax tree (FST).</typeparam>
/// <typeparam name="TSemantic">The type of the semantic syntax tree (SST).</typeparam>
/// <typeparam name="TResult">The type of the syntax finalisation result.</typeparam>
public abstract class BaseSyntaxFinaliser<TFinal, TSemantic, TResult> :
	ISyntaxFinaliser<TFinal, TSemantic, TResult>
	where TFinal : notnull, IFinalSyntaxTree<TSemantic>
	where TSemantic : notnull, ISemanticSyntaxTree
	where TResult : notnull, ISyntaxFinalisationResult<TFinal, TSemantic>
{
	#region Nested types
	/// <summary>Represents the finaliser instance that can be used for a single final syntax tree (FST) generation.</summary>
	protected abstract class FinaliserInstance : StageInstance
	{
		#region Properties
		/// <summary>The semantic syntax tree (SST) that is being finalised.</summary>
		protected TSemantic Semantic { get; }
		#endregion

		#region Constructors
		/// <summary>Populates the <see cref="FinaliserInstance"/> properties.</summary>
		/// <param name="finaliser">The finaliser that created this instance.</param>
		/// <param name="semantic">The semantic syntax tree (SST) to finalise.</param>
		protected FinaliserInstance(ISyntaxFinaliser finaliser, TSemantic semantic) : base(finaliser)
		{
			Semantic = semantic;
		}
		#endregion

		#region Methods
		/// <summary>Finalises the provided semantic syntax tree (SST).</summary>
		/// <returns>The result of finalising the semantic syntax tree (SST).</returns>
		public TResult Finalise()
		{
			Stopwatch watch = Stopwatch.StartNew();
			TFinal tree = Convert(Semantic);

			return CreateResult(watch.Elapsed, tree);
		}

		/// <summary>Converts the given <paramref name="semantic"/> syntax tree (SST) into a final syntax tree (FST).</summary>
		/// <param name="semantic">The semantic syntax tree (SST) to convert.</param>
		/// <returns>The generated final syntax tree (FST).</returns>
		protected abstract TFinal Convert(TSemantic semantic);

		/// <summary>Creates the finaliser result.</summary>
		/// <param name="duration">The amount of time it took to generate the final syntax tree (FST).</param>
		/// <param name="tree">The final syntax tree (FST) that was generated.</param>
		/// <returns>The created finaliser result.</returns>
		protected abstract TResult CreateResult(TimeSpan duration, TFinal tree);
		#endregion
	}
	#endregion

	#region Properties
	/// <inheritdoc/>
	public string Name => "syntax_finaliser";
	#endregion

	#region Methods
	/// <inheritdoc/>
	public TResult Finalise(TSemantic semantic)
	{
		FinaliserInstance finaliser = CreateInstance(semantic);

		return finaliser.Finalise();
	}

	/// <summary>Creates a new finaliser instance.</summary>
	/// <param name="semantic">The semantic syntax tree (SST) to convert.</param>
	/// <returns>The finalise instance to use for the final syntax tree (FST) generation.</returns>
	protected abstract FinaliserInstance CreateInstance(TSemantic semantic);
	#endregion
}
