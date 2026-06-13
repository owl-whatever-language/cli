namespace OwlDomain.ParsingTools.Syntax;

/// <summary>
/// 	Represents a bundle of syntax trees for a single source file.
/// </summary>
public interface ISyntaxBundle
{
	#region Properties
	/// <summary>The source file that the syntax trees are for.</summary>
	ISourceFile Source { get; }

	/// <summary>The concrete syntax tree.</summary>
	IConcreteSyntaxTree? Concrete { get; }

	/// <summary>The semantic syntax tree.</summary>
	ISemanticSyntaxTree? Semantic { get; }

	/// <summary>The final syntax tree.</summary>
	IFinalSyntaxTree? Final { get; }
	#endregion
}

/// <summary>
/// 	Represents a bundle of syntax trees for a single source file.
/// </summary>
/// <typeparam name="TConcrete">The type of the concrete syntax tree.</typeparam>
/// <typeparam name="TSemantic">The type of the semantic syntax tree.</typeparam>
/// <typeparam name="TFinal">The type of the final syntax tree.</typeparam>
public interface ISyntaxBundle<TConcrete, TSemantic, TFinal> : ISyntaxBundle
	where TConcrete : notnull, IConcreteSyntaxTree
	where TSemantic : notnull, ISemanticSyntaxTree
	where TFinal : notnull, IFinalSyntaxTree
{
	#region Properties
	/// <summary>The concrete syntax tree.</summary>
	new TConcrete? Concrete { get; set; }
	IConcreteSyntaxTree? ISyntaxBundle.Concrete => Concrete;

	/// <summary>The semantic syntax tree.</summary>
	new TSemantic? Semantic { get; set; }
	ISemanticSyntaxTree? ISyntaxBundle.Semantic => Semantic;

	/// <summary>The final syntax tree.</summary>
	new TFinal? Final { get; set; }
	IFinalSyntaxTree? ISyntaxBundle.Final => Final;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a bundle of syntax trees for a single source file.
/// </summary>
/// <typeparam name="TConcrete">The type of the concrete syntax tree.</typeparam>
/// <typeparam name="TSemantic">The type of the semantic syntax tree.</typeparam>
/// <typeparam name="TFinal">The type of the final syntax tree.</typeparam>
public abstract class BaseSyntaxBundle<TConcrete, TSemantic, TFinal> : ISyntaxBundle<TConcrete, TSemantic, TFinal>
	where TConcrete : notnull, IConcreteSyntaxTree
	where TSemantic : notnull, ISemanticSyntaxTree
	where TFinal : notnull, IFinalSyntaxTree
{
	#region Properties
	/// <inheritdoc/>
	public ISourceFile Source { get; }

	/// <inheritdoc/>
	public TConcrete? Concrete { get; set; }

	/// <inheritdoc/>
	public TSemantic? Semantic { get; set; }

	/// <inheritdoc/>
	public TFinal? Final { get; set; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseSyntaxBundle{TConcrete, TSemantic, TFinal}"/> properties.</summary>
	/// <param name="source">The source file that the syntax trees are for.</param>
	protected BaseSyntaxBundle(ISourceFile source)
	{
		Source = source;
	}
	#endregion
}
