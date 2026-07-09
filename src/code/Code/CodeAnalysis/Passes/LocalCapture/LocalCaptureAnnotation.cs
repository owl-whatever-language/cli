namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.LocalCapture;

public interface IUsedVariableInfo
{
	#region Properties
	ILocalVariable Variable { get; }
	IReadOnlyCollection<IAnnotatedGetExpressionSyntax> Uses { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class UsedVariableInfo : IUsedVariableInfo
{
	#region Properties
	public ILocalVariable Variable { get; }
	public List<IAnnotatedGetExpressionSyntax> Uses { get; } = [];

	IReadOnlyCollection<IAnnotatedGetExpressionSyntax> IUsedVariableInfo.Uses => Uses;
	#endregion

	#region Constructors
	public UsedVariableInfo(ILocalVariable variable)
	{
		Variable = variable;
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Variable: {Variable.Name} {{ Uses = ({Uses.Count:n0}) }}";
	#endregion
}

public class LocalCaptureAnnotation : CodeAnnotation
{
	#region Properties
	public override string Kind => "local_capture";

	// Note(Nightowl):
	// It might be fine for this to be a IDeclaredLocalVariable,
	// Why do I even have types for non-declared ones?
	// How could that possibly even occur?
	public IReadOnlyCollection<IUsedVariableInfo> Variables { get; }
	#endregion

	#region Constructors
	public LocalCaptureAnnotation(IReadOnlyCollection<IUsedVariableInfo> variables)
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
		public void AddLocalCapture(IReadOnlyCollection<IUsedVariableInfo> variables)
		{
			LocalCaptureAnnotation annotation = new(variables);
			function.Annotations.Add(annotation);
		}
		public LocalCaptureAnnotation GetLocalCapture() => function.Annotations.Get<LocalCaptureAnnotation>();
		#endregion
	}
}
