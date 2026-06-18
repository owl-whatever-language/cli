namespace OwlDomain.Owl.Code.CodeAnalysis;

public sealed class CompilationUpdateResult : StageResult
{
	#region Properties
	public override string Stage => "compilation_update";
	#endregion

	#region Constructors
	public CompilationUpdateResult(
		IPerformanceResult performance,
		IReadOnlyList<IStageResult> subResults)
		: base(new DiagnosticBag(), performance, subResults)
	{
	}
	#endregion
}

public class CompilationContext
{
	#region Fields
	private readonly Dictionary<ISourceFile, SyntaxTreeBundle> _trees = [];
	#endregion

	#region Properties
	public IReadOnlyCollection<ISyntaxTreeBundle> Trees => _trees.Values;
	#endregion

	#region Methods
	public CompilationUpdateResult Update(
		IReadOnlyCollection<ISourceFile>? added = null,
		IReadOnlyCollection<ISourceFile>? removed = null,
		IReadOnlyCollection<ISourceFile>? changed = null)
	{
		added ??= [];
		removed ??= [];
		changed ??= [];

		using PerformanceScope _ = PerformanceResult.Scope(out IPerformanceResult performance);

		foreach (ISourceFile file in removed)
			_trees.Remove(file);

		foreach (ISourceFile file in added)
		{
			SyntaxTreeBundle bundle = new(file);
			_trees.Add(file, bundle);
		}

		HashSet<ISourceFile> toReparse = [.. added, .. changed];

		ParallelParsingResult parsingResult = Parser.Parse(toReparse);
		foreach (LexingAndParsingResult result in parsingResult.ByFile.Values)
		{
			SyntaxTreeBundle bundle = _trees[result.Source];
			bundle.Concrete = result.Parsing.Tree;
		}

		return new(performance, [parsingResult]);
	}
	#endregion
}
