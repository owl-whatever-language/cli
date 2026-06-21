namespace OwlDomain.Owl.Code.CodeAnalysis;

public sealed class CompilationUpdateResult : IStageResultPerformance, IStageResultParent
{
	#region Properties
	public string Stage => "compilation_update";
	public IPerformanceResult Performance { get; }
	public IReadOnlyCollection<IStageResult> Children { get; }
	#endregion

	#region Constructors
	public CompilationUpdateResult(
		IPerformanceResult performance,
		IReadOnlyCollection<IStageResult> children)
	{
		Performance = performance;
		Children = children;
	}
	#endregion
}

public class CompilationContext
{
	#region Fields
	private readonly Dictionary<ISourceFile, SyntaxTreeBundle> _trees = [];
	private readonly ISymbolScope _baseScope = CreateBaseScope();
	#endregion

	#region Properties
	public IReadOnlyDictionary<ISourceFile, ISyntaxTreeBundle> Trees => _trees.ToDictionary(pair => pair.Key, pair => (ISyntaxTreeBundle)pair.Value);
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
		foreach (LexingAndParsingResult result in parsingResult.GetByFile().Values)
		{
			SyntaxTreeBundle bundle = _trees[result.Source];
			bundle.Concrete = result.Parsing.Tree;
		}

		IReadOnlyCollection<IConcreteSyntaxTree> concrete = GetConcreteTrees().ToArray();
		SymbolCollectionResult symbolCollection = SymbolCollector.Collect(_baseScope, concrete);
		ParallelSemanticResolutionResult semanticResolutionResult = SemanticResolver.Resolve(concrete, symbolCollection.Symbols);

		foreach (ISymbolTarget target in symbolCollection.Targets)
		{
			if (target.IsMutable)
				ThrowHelper.ThrowInvalidOperationException($"The target '{target}' was still mutable.");
		}

		foreach (SemanticResolutionResult result in semanticResolutionResult.Children)
		{
			SyntaxTreeBundle bundle = _trees[result.Source];
			bundle.Semantic = result.Tree;
		}

		return new(performance, [parsingResult, symbolCollection, semanticResolutionResult]);
	}
	#endregion

	#region Helpers
	private IEnumerable<IConcreteSyntaxTree> GetConcreteTrees()
	{
		foreach (ISyntaxTreeBundle bundle in _trees.Values)
		{
			if (bundle.Concrete is null)
				ThrowHelper.ThrowInvalidOperationException("Expect the concrete tree to be set.");

			yield return bundle.Concrete;
		}
	}
	private static ISymbolScope CreateBaseScope()
	{
		SymbolScope root = new("root");
		SymbolScope builtin = root.NestScope("builtin", null);

		foreach (INamedTypeInfo type in SpecialTypes.GetAll())
			builtin.AddSymbol(type);

		foreach (IFunction function in SpecialFunctions.GetAll())
			builtin.AddSymbol(function);

		return builtin;
	}
	#endregion
}
