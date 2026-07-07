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
	public IEnumerable<IAnnotatedSyntaxTree> Trees => Children.Select(r => r.Tree);
	#endregion

	#region Constructors
	public ParallelAnnotationPreparingResult(IPerformanceResult performance, IReadOnlyCollection<AnnotationPreparingResult> children)
	{
		Performance = performance;
		Children = children;
	}
	#endregion
}

public sealed class AnnotationPreparer : BaseSemanticToAnnotatedTreeConverter
{
	#region Nested types
	private readonly struct Scope(AnnotationPreparer resolver) : IDisposable
	{
		#region Methods
		public void Dispose() => resolver.ExitScope();
		#endregion
	}
	private readonly ref struct ValueScope<T>(ref T field, T oldValue) : IDisposable
	{
		#region Fields
		private readonly ref T _field = ref field;
		#endregion

		#region Methods
		public void Dispose() => _field = oldValue;
		#endregion
	}
	#endregion

	#region Fields
	private ICallableType? _currentCallable;
	private IDeclaredFunction? _currentFunction;
	private int _currentCallableParameterIndex;
	#endregion

	#region Properties
	private ISourceFile Source { get; }
	private ISymbolScope BaseScope { get; }
	private ISymbolScope CurrentScope { get; set; }
	#endregion

	#region Constructors
	private AnnotationPreparer(ISourceFile source, ISymbolScope baseScope)
	{
		Source = source;
		BaseScope = baseScope;
		CurrentScope = baseScope;
	}
	#endregion

	#region Functions
	public static AnnotationPreparingResult Prepare(ISymbolScope baseScope, ISemanticSyntaxTree semantic)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			AnnotationPreparer preparer = new(semantic.Source, baseScope);
			IAnnotatedSyntaxTree annotated = preparer.Convert(semantic);

			return new(performance, annotated);
		}
	}
	public static ParallelAnnotationPreparingResult Prepare(ISymbolScope baseScope, IReadOnlyCollection<ISemanticSyntaxTree> trees)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			if (trees.Count is 0)
				return new(performance, []);

			if (trees.Count is 1)
			{
				AnnotationPreparingResult result = Prepare(baseScope, trees.Single());
				return new(performance, [result]);
			}

			AnnotationPreparingResult[] results = new AnnotationPreparingResult[trees.Count];
			ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

			Parallel.ForEach(trees, options, (tree, _, index) => results[index] = Prepare(baseScope, tree));
			return new(performance, results);
		}
	}
	#endregion

	#region Declaration refinement methods
	protected override AnnotatedVariableDeclarationStatementSyntax ConvertCore(ISemanticVariableDeclarationStatementSyntax semantic)
	{
		AnnotatedVariableDeclarationStatementSyntax annotated = base.ConvertCore(semantic);
		Update(semantic.Variable, annotated);

		return annotated;
	}
	protected override AnnotatedFunctionDeclarationStatementSyntax ConvertCore(ISemanticFunctionDeclarationStatementSyntax semantic)
	{
		AnnotatedFunctionDeclarationStatementSyntax annotated;
		using (WithValue(ref _currentFunction, semantic.Function))
		using (EnterScope(semantic))
		{
			annotated = base.ConvertCore(semantic);
			Update(annotated.Function, annotated);
		}

		Update(semantic, annotated);
		return annotated;
	}
	#endregion

	#region Function call methods
	protected override AnnotatedFunctionCallExpressionSyntax ConvertCore(ISemanticFunctionCallExpressionSyntax semantic)
	{
		using (WithValue(ref _currentCallableParameterIndex, 0))
		using (WithValue(ref _currentCallable, semantic.Callable))
			return base.ConvertCore(semantic);
	}
	protected override AnnotatedRegularFunctionArgumentSyntax ConvertCore(ISemanticRegularFunctionArgumentSyntax semantic)
	{
		var value = Convert(semantic.Value);
		var parameter = _currentCallable?.Parameters.ElementAtOrDefault(_currentCallableParameterIndex++);

		return new(value, parameter);
	}
	protected override AnnotatedNamedFunctionArgumentSyntax ConvertCore(ISemanticNamedFunctionArgumentSyntax semantic)
	{
		var parameter = _currentCallable?.Parameters.FirstOrDefault(p => p.Name == (semantic.Name.Value as string));

		var name = Convert(semantic.Name, (parameter as ICallableFunctionParameter)?.FunctionParameter);
		var separator = Convert(semantic.Separator);
		var value = Convert(semantic.Value);

		if (parameter?.Index == _currentCallableParameterIndex)
			_currentCallableParameterIndex++;

		return new(name, separator, value, parameter);
	}
	#endregion

	#region Scope helpers
	private ValueScope<T> WithValue<T>(ref T field, T value)
	{
		T old = field;
		field = value;
		return new(ref field, old);
	}
	private Scope EnterScope(ISemanticSyntaxNode declaration)
	{
		// Note(Nightowl):
		// We could get the scope directly from the declaration in each method,
		// however this approach ensures we don't accidentally skip any scopes,
		// as that would mess up exiting the current scope. Extra line padding;

		// I just wanted to make the comment line lengths line up;

		CurrentScope = CurrentScope.GetChild(declaration);
		return new(this);
	}
	private void ExitScope()
	{
		if (CurrentScope == BaseScope)
			ThrowHelper.ThrowInvalidOperationException($"Exiting the base scope ({BaseScope.Name}) is not allowed.");

		Debug.Assert(CurrentScope.Parent is not null);
		CurrentScope = CurrentScope.Parent;
	}
	#endregion

	#region Symbol helpers
	private void Update(IDeclaredSymbol symbol, IAnnotatedSyntaxNode declaration)
	{
		CurrentScope.Update(symbol, declaration);
	}
	private void Update(ISemanticSyntaxNode oldDeclaration, IAnnotatedSyntaxNode newDeclaration)
	{
		CurrentScope.UpdateChild(oldDeclaration, newDeclaration);
	}
	#endregion
}
