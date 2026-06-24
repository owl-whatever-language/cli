namespace OwlDomain.Owl.Code.CodeAnalysis.Finalisation;

public sealed class SyntaxFinalisationResult : ISourceStageResult, IStageResultPerformance
{
	#region Properties
	public string Stage => "syntax_finalisation";
	public ISourceFile Source => Tree.Source;
	public IPerformanceResult Performance { get; }
	public IFinalSyntaxTree Tree { get; }
	#endregion

	#region Constructors
	public SyntaxFinalisationResult(IPerformanceResult performance, IFinalSyntaxTree tree)
	{
		Performance = performance;
		Tree = tree;
	}
	#endregion
}

public sealed class ParallelFinalisationResult : IParallelStageResult<SyntaxFinalisationResult>
{
	#region Properties
	public string Stage => "parallel_syntax_finalisation";
	public IPerformanceResult Performance { get; }
	public IReadOnlyCollection<SyntaxFinalisationResult> Children { get; }
	#endregion

	#region Constructors
	public ParallelFinalisationResult(IPerformanceResult performance, IReadOnlyCollection<SyntaxFinalisationResult> children)
	{
		Performance = performance;
		Children = children;
	}
	#endregion
}

public sealed class SyntaxFinaliser : BaseSemanticToFinalTreeConverter
{
	#region Constructors
	private SyntaxFinaliser() { }
	#endregion

	#region Functions
	public static SyntaxFinalisationResult Finalise(ISemanticSyntaxTree semantic)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			SyntaxFinaliser finaliser = new();
			IFinalSyntaxTree final = finaliser.Convert(semantic);

			return new(performance, final);
		}
	}

	public static ParallelFinalisationResult Finalise(IReadOnlyCollection<ISemanticSyntaxTree> trees)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			if (trees.Count is 1)
			{
				SyntaxFinalisationResult result = Finalise(trees.Single());
				return new(performance, [result]);
			}

			SyntaxFinalisationResult[] results = new SyntaxFinalisationResult[trees.Count];
			ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

			Parallel.ForEach(trees, options, (tree, _, index) =>
			{
				SyntaxFinalisationResult result = Finalise(tree);
				results[index] = result;
			});

			return new(performance, results);
		}
	}
	#endregion

	#region Methods
	protected override FinalFunctionDeclarationStatementSyntax Convert(ISemanticFunctionDeclarationStatementSyntax semantic)
	{
		FinalFunctionDeclarationStatementSyntax final = base.Convert(semantic);

		return final;
	}
	protected override IFinalFunctionParameterSyntax Convert(ISemanticFunctionParameterSyntax semantic)
	{
		IFinalFunctionParameterSyntax final = base.Convert(semantic);

		return final;
	}

	protected override FinalVariableDeclarationStatementSyntax Convert(ISemanticVariableDeclarationStatementSyntax semantic)
	{
		FinalVariableDeclarationStatementSyntax final = base.Convert(semantic);

		return final;
	}
	#endregion
}
