namespace OwlDomain.ParsingTools.Syntax.SstConversion;

/// <summary>
/// 	Represents a abstract syntax tree (AST) to an semantic syntax tree (SST) converter.
/// </summary>
public interface ISstConverter : IDiagnosticProvider
{
}

/// <summary>
/// 	Represents a abstract syntax tree (AST) to an semantic syntax tree (SST) converter.
/// </summary>
/// <typeparam name="TSemantic">The type of the semantic syntax tree (SST).</typeparam>
/// <typeparam name="TSemanticRoot">The type of the root node in the semantic syntax tree (SST).</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST).</typeparam>
public interface ISstConverter<out TSemantic, out TSemanticRoot, TAbstract> : ISstConverter
	where TSemantic : notnull, ISemanticSyntaxTree<TSemanticRoot, TAbstract>
	where TSemanticRoot : notnull, ISemanticSyntaxNode
	where TAbstract : notnull, IAbstractSyntaxTree
{
	#region Methods
	/// <summary>Converts the given abstract syntax tree (AST) into an semantic syntax tree (SST).</summary>
	/// <param name="abstract">The abstract syntax tree (AST) to convert.</param>
	/// <returns>A result of converting the abstract syntax tree (AST) into an semantic syntax tree (SST).</returns>
	ISstConversionResult<TSemantic, TSemanticRoot, TAbstract> Convert(TAbstract @abstract);
	#endregion
}

/// <summary>
/// 	Represents a abstract syntax tree (AST) to an semantic syntax tree (SST) converter.
/// </summary>
/// <typeparam name="TSemantic">The type of the semantic syntax tree (SST).</typeparam>
/// <typeparam name="TSemanticRoot">The type of the root node in the semantic syntax tree (SST).</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TAbstractRoot">The type of the root node in the abstract syntax tree (AST).</typeparam>
public interface ISstConverter<out TSemantic, out TSemanticRoot, TAbstract, TAbstractRoot> : ISstConverter<TSemantic, TSemanticRoot, TAbstract>
	where TSemantic : notnull, ISemanticSyntaxTree<TSemanticRoot, TAbstract, TAbstractRoot>
	where TSemanticRoot : notnull, ISemanticSyntaxNode<TAbstractRoot>
	where TAbstract : notnull, IAbstractSyntaxTree<TAbstractRoot>
	where TAbstractRoot : notnull, IAbstractSyntaxNode
{
	#region Methods
	/// <summary>Converts the given abstract syntax tree (AST) into an semantic syntax tree (SST).</summary>
	/// <param name="abstract">The abstract syntax tree (AST) to convert.</param>
	/// <returns>A result of converting the abstract syntax tree (AST) into an semantic syntax tree (SST).</returns>
	new ISstConversionResult<TSemantic, TSemanticRoot, TAbstract, TAbstractRoot> Convert(TAbstract @abstract);
	ISstConversionResult<TSemantic, TSemanticRoot, TAbstract> ISstConverter<TSemantic, TSemanticRoot, TAbstract>.Convert(TAbstract @abstract) => Convert(@abstract);
	#endregion
}

/// <summary>
/// 	Represents a abstract syntax tree (AST) to an semantic syntax tree (SST) converter.
/// </summary>
/// <typeparam name="TResult">The type of the SST converter result.</typeparam>
/// <typeparam name="TSemantic">The type of the semantic syntax tree (SST).</typeparam>
/// <typeparam name="TSemanticRoot">The type of the root node in the semantic syntax tree (SST).</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TAbstractRoot">The type of the root node in the abstract syntax tree (AST).</typeparam>
public interface ISstConverter<out TResult, out TSemantic, out TSemanticRoot, TAbstract, TAbstractRoot> :
	ISstConverter<TSemantic, TSemanticRoot, TAbstract, TAbstractRoot>
	where TResult : notnull, ISstConversionResult<TSemantic, TSemanticRoot, TAbstract, TAbstractRoot>
	where TSemantic : notnull, ISemanticSyntaxTree<TSemanticRoot, TAbstract, TAbstractRoot>
	where TSemanticRoot : notnull, ISemanticSyntaxNode<TAbstractRoot>
	where TAbstract : notnull, IAbstractSyntaxTree<TAbstractRoot>
	where TAbstractRoot : notnull, IAbstractSyntaxNode
{
	#region Methods
	/// <summary>Converts the given abstract syntax tree (AST) into an semantic syntax tree (SST).</summary>
	/// <param name="abstract">The abstract syntax tree (AST) to convert.</param>
	/// <returns>A result of converting the abstract syntax tree (AST) into an semantic syntax tree (SST).</returns>
	new TResult Convert(TAbstract @abstract);
	ISstConversionResult<TSemantic, TSemanticRoot, TAbstract, TAbstractRoot> ISstConverter<TSemantic, TSemanticRoot, TAbstract, TAbstractRoot>.Convert(TAbstract @abstract) => Convert(@abstract);
	#endregion
}


/// <summary>
/// 	Represents the base implementation for a abstract syntax tree (AST) to an semantic syntax tree (SST) converter.
/// </summary>
/// <typeparam name="TResult">The type of the SST converter result.</typeparam>
/// <typeparam name="TSemantic">The type of the semantic syntax tree (SST).</typeparam>
/// <typeparam name="TSemanticRoot">The type of the root node in the semantic syntax tree (SST).</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TAbstractRoot">The type of the root node in the abstract syntax tree (AST).</typeparam>
public abstract class BaseSstConverter<TResult, TSemantic, TSemanticRoot, TAbstract, TAbstractRoot> :
	ISstConverter<TResult, TSemantic, TSemanticRoot, TAbstract, TAbstractRoot>
	where TResult : notnull, ISstConversionResult<TSemantic, TSemanticRoot, TAbstract, TAbstractRoot>
	where TSemantic : notnull, ISemanticSyntaxTree<TSemanticRoot, TAbstract, TAbstractRoot>
	where TSemanticRoot : notnull, ISemanticSyntaxNode<TAbstractRoot>
	where TAbstract : notnull, IAbstractSyntaxTree<TAbstractRoot>
	where TAbstractRoot : notnull, IAbstractSyntaxNode
{
	#region Nested types
	/// <summary>Represents the converter instance that can be used for a single semantic syntax tree (SST) conversion.</summary>
	protected abstract class ConverterInstance
	{
		#region Properties
		/// <summary>The converter that created this instance.</summary>
		protected ISstConverter Converter { get; }

		/// <summary>The abstract syntax tree (AST) that is being converted.</summary>
		protected TAbstract Abstract { get; }

		/// <summary>The diagnostics that have occurred during the conversion.</summary>
		protected DiagnosticBag Diagnostics { get; } = [];
		#endregion

		#region Constructors
		/// <summary>Populates the converter instance properties.</summary>
		/// <param name="converter">The converter that created this instance.</param>
		/// <param name="abstract">The abstract syntax tree (AST) to convert.</param>
		protected ConverterInstance(ISstConverter converter, TAbstract @abstract)
		{
			Converter = converter;
			Abstract = @abstract;
		}
		#endregion

		#region Methods
		/// <summary>Converts the abstract syntax tree (AST) into an semantic syntax tree (SST).</summary>
		/// <returns>The result of converting the abstract syntax tree (AST) into an semantic syntax tree (SST).</returns>
		public TResult Convert()
		{
			Stopwatch watch = Stopwatch.StartNew();

			TSemantic tree = Convert(Abstract);
			TResult result = CreateResult(tree, watch.Elapsed);

			return result;
		}

		/// <summary>Converts the given <paramref name="abstract"/> syntax tree (AST) into an semantic syntax tree (SST).</summary>
		/// <param name="abstract">The abstract syntax tree (AST) to convert.</param>
		/// <returns>The generated semantic syntax tree (SST).</returns>
		protected abstract TSemantic Convert(TAbstract @abstract);

		/// <summary>Creates the converter result.</summary>
		/// <param name="tree">The semantic syntax tree (SST) that was generated.</param>
		/// <param name="duration">The amount of time it took to generate the semantic syntax tree (SST).</param>
		/// <returns>The created converter result.</returns>
		protected abstract TResult CreateResult(TSemantic tree, TimeSpan duration);
		#endregion
	}
	#endregion

	#region Properties
	/// <inheritdoc/>
	public virtual string Name => "ast_converter";
	#endregion

	#region Methods
	/// <inheritdoc/>
	public TResult Convert(TAbstract @abstract)
	{
		ConverterInstance converter = CreateConverter(@abstract);

		return converter.Convert();
	}

	/// <summary>Creates a new converter instance.</summary>
	/// <param name="abstract">The abstract syntax tree (AST) to convert.</param>
	/// <returns>The converter instance to use for the semantic syntax tree (SST) conversion.</returns>
	protected abstract ConverterInstance CreateConverter(TAbstract @abstract);
	#endregion
}
