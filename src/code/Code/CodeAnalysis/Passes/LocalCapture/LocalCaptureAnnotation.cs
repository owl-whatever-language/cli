namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.LocalCapture;

public class LocalCaptureAnnotation : CodeAnnotation
{
	#region Properties
	public override string Kind => "local_capture";

	// Note(Nightowl):
	// It might be fine for this to be a IDeclaredLocalVariable,
	// Why do I even have types for non-declared ones?
	// How could that possibly even occur?
	public IReadOnlyDictionary<ILocalVariable, IReadOnlyCollection<IAnnotatedGetExpressionSyntax>> Variables { get; }
	#endregion

	#region Constructors
	public LocalCaptureAnnotation(IReadOnlyDictionary<ILocalVariable, IReadOnlyCollection<IAnnotatedGetExpressionSyntax>> variables)
	{
		Variables = variables;
	}
	#endregion
}

public static class LocalCaptureExtensions
{
	extension(IAnnotatedFunctionDeclarationStatementSyntax function)
	{
		#region Methods
		public void AddLocalCapture(IReadOnlyDictionary<ILocalVariable, IReadOnlyCollection<IAnnotatedGetExpressionSyntax>> variables)
		{
			LocalCaptureAnnotation annotation = new(variables);
			function.Annotations.Add(annotation);
		}
		public LocalCaptureAnnotation GetLocalCapture() => function.Annotations.Get<LocalCaptureAnnotation>();
		#endregion
	}
}
