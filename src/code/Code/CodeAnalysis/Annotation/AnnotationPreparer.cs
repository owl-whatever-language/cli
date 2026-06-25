namespace OwlDomain.Owl.Code.CodeAnalysis.Annotation;

public sealed class AnnotationPreparingResult : ISourceStageResult, IStageResultPerformance
{
	#region Properties
	public string Stage => "annotation_preparing";
	public ISourceFile Source => Tree.Source;
	public IPerformanceResult Performance { get; }
	public IAnnotatedSyntaxTree Tree { get; }
	#endregion

	#region Constructors
	public AnnotationPreparingResult(IPerformanceResult performance, IAnnotatedSyntaxTree tree)
	{
		Performance = performance;
		Tree = tree;
	}
	#endregion
}

public sealed class ParallelAnnotationPreparingResult : IParallelStageResult<AnnotationPreparingResult>
{
	#region Properties
	public string Stage => "annotation_preparing";
	public IPerformanceResult Performance { get; }
	public IReadOnlyCollection<AnnotationPreparingResult> Children { get; }
	#endregion

	#region Constructors
	public ParallelAnnotationPreparingResult(IPerformanceResult performance, IReadOnlyCollection<AnnotationPreparingResult> children)
	{
		Performance = performance;
		Children = children;
	}
	#endregion
}

public sealed class AnnotationPrepared : BaseSemanticToAnnotatedTreeConverter
{
	#region Constructors
	private AnnotationPrepared() { }
	#endregion

	#region Functions
	public static AnnotationPreparingResult Prepare(ISemanticSyntaxTree semantic)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			AnnotationPrepared preparer = new();
			IAnnotatedSyntaxTree annotated = preparer.Convert(semantic);

			return new(performance, annotated);
		}
	}

	public static ParallelAnnotationPreparingResult Prepare(IReadOnlyCollection<ISemanticSyntaxTree> trees)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			if (trees.Count is 1)
			{
				AnnotationPreparingResult result = Prepare(trees.Single());
				return new(performance, [result]);
			}

			AnnotationPreparingResult[] results = new AnnotationPreparingResult[trees.Count];
			ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

			Parallel.ForEach(trees, options, (tree, _, index) =>
			{
				AnnotationPreparingResult result = Prepare(tree);
				results[index] = result;
			});

			return new(performance, results);
		}
	}
	#endregion

	#region Methods
	protected override AnnotatedFunctionDeclarationStatementSyntax Convert(ISemanticFunctionDeclarationStatementSyntax semantic)
	{
		AnnotatedFunctionDeclarationStatementSyntax annotated = base.Convert(semantic);

		return annotated;
	}
	protected override IAnnotatedFunctionParameterSyntax Convert(ISemanticFunctionParameterSyntax semantic)
	{
		IAnnotatedFunctionParameterSyntax annotated = base.Convert(semantic);

		return annotated;
	}

	protected override AnnotatedVariableDeclarationStatementSyntax Convert(ISemanticVariableDeclarationStatementSyntax semantic)
	{
		AnnotatedVariableDeclarationStatementSyntax annotated = base.Convert(semantic);

		return annotated;
	}
	#endregion
}
