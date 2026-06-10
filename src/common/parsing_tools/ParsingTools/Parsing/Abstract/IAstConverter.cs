namespace OwlDomain.ParsingTools.Parsing.Abstract;

/// <summary>
/// 	Represents a concrete syntax tree (CST) to an abstract syntax tree (AST) converter.
/// </summary>
public interface IAstConverter : IDiagnosticProvider
{
}

/// <summary>
/// 	Represents a concrete syntax tree (CST) to an abstract syntax tree (AST) converter.
/// </summary>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TAbstractRoot">The type of the root node in the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST).</typeparam>
public interface IAstConverter<out TAbstract, out TAbstractRoot, TConcrete> : IAstConverter
	where TAbstract : notnull, IAbstractSyntaxTree<TAbstractRoot, TConcrete>
	where TAbstractRoot : notnull, IAbstractSyntaxNode
	where TConcrete : notnull, IConcreteSyntaxTree
{
	#region Methods
	/// <summary>Converts the given concrete syntax tree (CST) into an abstract syntax tree (AST).</summary>
	/// <param name="concrete">The concrete syntax tree (CST) to convert.</param>
	/// <returns>A result of converting the concrete syntax tree (CST) into an abstract syntax tree (AST).</returns>
	IAstConversionResult<TAbstract, TAbstractRoot, TConcrete> Convert(TConcrete concrete);
	#endregion
}

/// <summary>
/// 	Represents a concrete syntax tree (CST) to an abstract syntax tree (AST) converter.
/// </summary>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TAbstractRoot">The type of the root node in the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST).</typeparam>
/// <typeparam name="TConcreteRoot">The type of the root node in the concrete syntax tree (CST).</typeparam>
public interface IAstConverter<out TAbstract, out TAbstractRoot, TConcrete, TConcreteRoot> : IAstConverter<TAbstract, TAbstractRoot, TConcrete>
	where TAbstract : notnull, IAbstractSyntaxTree<TAbstractRoot, TConcrete, TConcreteRoot>
	where TAbstractRoot : notnull, IAbstractSyntaxNode<TConcreteRoot>
	where TConcrete : notnull, IConcreteSyntaxTree<TConcreteRoot>
	where TConcreteRoot : notnull, IConcreteSyntaxNode
{
	#region Methods
	/// <summary>Converts the given concrete syntax tree (CST) into an abstract syntax tree (AST).</summary>
	/// <param name="concrete">The concrete syntax tree (CST) to convert.</param>
	/// <returns>A result of converting the concrete syntax tree (CST) into an abstract syntax tree (AST).</returns>
	new IAstConversionResult<TAbstract, TAbstractRoot, TConcrete, TConcreteRoot> Convert(TConcrete concrete);
	IAstConversionResult<TAbstract, TAbstractRoot, TConcrete> IAstConverter<TAbstract, TAbstractRoot, TConcrete>.Convert(TConcrete concrete) => Convert(concrete);
	#endregion
}

/// <summary>
/// 	Represents a concrete syntax tree (CST) to an abstract syntax tree (AST) converter.
/// </summary>
/// <typeparam name="TResult">The type of the AST converter result.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TAbstractRoot">The type of the root node in the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST).</typeparam>
/// <typeparam name="TConcreteRoot">The type of the root node in the concrete syntax tree (CST).</typeparam>
public interface IAstConverter<out TResult, out TAbstract, out TAbstractRoot, TConcrete, TConcreteRoot> :
	IAstConverter<TAbstract, TAbstractRoot, TConcrete, TConcreteRoot>
	where TResult : notnull, IAstConversionResult<TAbstract, TAbstractRoot, TConcrete, TConcreteRoot>
	where TAbstract : notnull, IAbstractSyntaxTree<TAbstractRoot, TConcrete, TConcreteRoot>
	where TAbstractRoot : notnull, IAbstractSyntaxNode<TConcreteRoot>
	where TConcrete : notnull, IConcreteSyntaxTree<TConcreteRoot>
	where TConcreteRoot : notnull, IConcreteSyntaxNode
{
	#region Methods
	/// <summary>Converts the given concrete syntax tree (CST) into an abstract syntax tree (AST).</summary>
	/// <param name="concrete">The concrete syntax tree (CST) to convert.</param>
	/// <returns>A result of converting the concrete syntax tree (CST) into an abstract syntax tree (AST).</returns>
	new TResult Convert(TConcrete concrete);
	IAstConversionResult<TAbstract, TAbstractRoot, TConcrete, TConcreteRoot> IAstConverter<TAbstract, TAbstractRoot, TConcrete, TConcreteRoot>.Convert(TConcrete concrete) => Convert(concrete);
	#endregion
}


/// <summary>
/// 	Represents the base implementation for a concrete syntax tree (CST) to an abstract syntax tree (AST) converter.
/// </summary>
/// <typeparam name="TResult">The type of the AST converter result.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TAbstractRoot">The type of the root node in the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST).</typeparam>
/// <typeparam name="TConcreteRoot">The type of the root node in the concrete syntax tree (CST).</typeparam>
public abstract class BaseAstConverter<TResult, TAbstract, TAbstractRoot, TConcrete, TConcreteRoot> :
	IAstConverter<TResult, TAbstract, TAbstractRoot, TConcrete, TConcreteRoot>
	where TResult : notnull, IAstConversionResult<TAbstract, TAbstractRoot, TConcrete, TConcreteRoot>
	where TAbstract : notnull, IAbstractSyntaxTree<TAbstractRoot, TConcrete, TConcreteRoot>
	where TAbstractRoot : notnull, IAbstractSyntaxNode<TConcreteRoot>
	where TConcrete : notnull, IConcreteSyntaxTree<TConcreteRoot>
	where TConcreteRoot : notnull, IConcreteSyntaxNode
{
	#region Nested types
	/// <summary>Represents the converter instance that can be used for a single abstract syntax tree (AST) conversion.</summary>
	protected abstract class ConverterInstance
	{
		#region Properties
		/// <summary>The converter that created this instance.</summary>
		protected IAstConverter Converter { get; }

		/// <summary>The concrete syntax tree (CST) that is being converted.</summary>
		protected TConcrete Concrete { get; }

		/// <summary>The diagnostics that have occurred during the conversion.</summary>
		protected DiagnosticBag Diagnostics { get; } = [];
		#endregion

		#region Constructors
		/// <summary>Populates the converter instance properties.</summary>
		/// <param name="converter">The converter that created this instance.</param>
		/// <param name="concrete">The concrete syntax tree (CST) to convert.</param>
		protected ConverterInstance(IAstConverter converter, TConcrete concrete)
		{
			Converter = converter;
			Concrete = concrete;
		}
		#endregion

		#region Methods
		/// <summary>Converts the concrete syntax tree (CST) into an abstract syntax tree (AST).</summary>
		/// <returns>The result of converting the concrete syntax tree (CST) into an abstract syntax tree (AST).</returns>
		public TResult Convert()
		{
			Stopwatch watch = Stopwatch.StartNew();

			TAbstract tree = Convert(Concrete);
			TResult result = CreateResult(tree, watch.Elapsed);

			return result;
		}

		/// <summary>Converts the given <paramref name="concrete"/> syntax tree (CST) into an abstract syntax tree (AST).</summary>
		/// <param name="concrete">The concrete syntax tree (CST) to convert.</param>
		/// <returns>The generated abstract syntax tree (AST).</returns>
		protected abstract TAbstract Convert(TConcrete concrete);

		/// <summary>Creates the converter result.</summary>
		/// <param name="tree">The abstract syntax tree (AST) that was generated.</param>
		/// <param name="duration">The amount of time it took to generate the abstract syntax tree (AST).</param>
		/// <returns>The created converter result.</returns>
		protected abstract TResult CreateResult(TAbstract tree, TimeSpan duration);
		#endregion
	}
	#endregion

	#region Properties
	/// <inheritdoc/>
	public virtual string Name => "ast_converter";
	#endregion

	#region Methods
	/// <inheritdoc/>
	public TResult Convert(TConcrete concrete)
	{
		ConverterInstance converter = CreateConverter(concrete);

		return converter.Convert();
	}

	/// <summary>Creates a new converter instance.</summary>
	/// <param name="concrete">The concrete syntax tree (CST) to convert.</param>
	/// <returns>The converter instance to use for the abstract syntax tree (AST) conversion.</returns>
	protected abstract ConverterInstance CreateConverter(TConcrete concrete);
	#endregion
}
