namespace OwlDomain.ParsingTools.Parsing.Abstract;

/// <summary>
/// 	Represents a concrete syntax tree (CST) to an abstract syntax tree (AST) creator.
/// </summary>
public interface IAstCreator : IDiagnosticProvider
{
}

/// <summary>
/// 	Represents a concrete syntax tree (CST) to an abstract syntax tree (AST) creator.
/// </summary>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST).</typeparam>
public interface IAstCreator<out TAbstract, TConcrete> : IAstCreator
	where TAbstract : notnull, IAbstractSyntaxTree<TConcrete>
	where TConcrete : notnull, IConcreteSyntaxTree
{
	#region Methods
	/// <summary>Converts the given concrete syntax tree (CST) into an abstract syntax tree (AST).</summary>
	/// <param name="concrete">The concrete syntax tree (CST) to convert.</param>
	/// <returns>A result of converting the concrete syntax tree (CST) into an abstract syntax tree (AST).</returns>
	IAstCreationResult<TAbstract, TConcrete> Convert(TConcrete concrete);
	#endregion
}


/// <summary>
/// 	Represents a concrete syntax tree (CST) to an abstract syntax tree (AST) creator.
/// </summary>
/// <typeparam name="TResult">The type of the AST creator result.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST).</typeparam>
public interface IAstCreator<out TAbstract, TConcrete, out TResult> :
	IAstCreator<TAbstract, TConcrete>
	where TAbstract : notnull, IAbstractSyntaxTree<TConcrete>
	where TConcrete : notnull, IConcreteSyntaxTree
	where TResult : notnull, IAstCreationResult<TAbstract, TConcrete>
{
	#region Methods
	/// <summary>Converts the given concrete syntax tree (CST) into an abstract syntax tree (AST).</summary>
	/// <param name="concrete">The concrete syntax tree (CST) to convert.</param>
	/// <returns>A result of converting the concrete syntax tree (CST) into an abstract syntax tree (AST).</returns>
	new TResult Convert(TConcrete concrete);
	IAstCreationResult<TAbstract, TConcrete> IAstCreator<TAbstract, TConcrete>.Convert(TConcrete concrete) => Convert(concrete);
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a concrete syntax tree (CST) to an abstract syntax tree (AST) creator.
/// </summary>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST).</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST).</typeparam>
/// <typeparam name="TResult">The type of the AST creator result.</typeparam>
public abstract class BaseAstCreator<TAbstract, TConcrete, TResult> :
	IAstCreator<TAbstract, TConcrete, TResult>
	where TAbstract : notnull, IAbstractSyntaxTree<TConcrete>
	where TConcrete : notnull, IConcreteSyntaxTree
	where TResult : notnull, IAstCreationResult<TAbstract, TConcrete>
{
	#region Nested types
	/// <summary>Represents the creator instance that can be used for a single abstract syntax tree (AST) creation.</summary>
	protected abstract class CreatorInstance : StageInstance
	{
		#region Properties
		/// <summary>The concrete syntax tree (CST) that is being converted.</summary>
		protected TConcrete Concrete { get; }
		#endregion

		#region Constructors
		/// <summary>Populates the creator instance properties.</summary>
		/// <param name="creator">The creator that created this instance.</param>
		/// <param name="concrete">The concrete syntax tree (CST) to convert.</param>
		protected CreatorInstance(IAstCreator creator, TConcrete concrete) : base(creator)
		{
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
			TResult result = CreateResult(watch.Elapsed, tree);

			return result;
		}

		/// <summary>Converts the given <paramref name="concrete"/> syntax tree (CST) into an abstract syntax tree (AST).</summary>
		/// <param name="concrete">The concrete syntax tree (CST) to convert.</param>
		/// <returns>The generated abstract syntax tree (AST).</returns>
		protected abstract TAbstract Convert(TConcrete concrete);

		/// <summary>Creates the creator result.</summary>
		/// <param name="duration">The amount of time it took to generate the abstract syntax tree (AST).</param>
		/// <param name="tree">The abstract syntax tree (AST) that was generated.</param>
		/// <returns>The created creator result.</returns>
		protected abstract TResult CreateResult(TimeSpan duration, TAbstract tree);
		#endregion
	}
	#endregion

	#region Properties
	/// <inheritdoc/>
	public virtual string Name => "ast_creator";
	#endregion

	#region Methods
	/// <inheritdoc/>
	public TResult Convert(TConcrete concrete)
	{
		CreatorInstance creator = CreateInstance(concrete);

		return creator.Convert();
	}

	/// <summary>Creates a new creator instance.</summary>
	/// <param name="concrete">The concrete syntax tree (CST) to convert.</param>
	/// <returns>The creator instance to use for the abstract syntax tree (AST) creation.</returns>
	protected abstract CreatorInstance CreateInstance(TConcrete concrete);
	#endregion
}
