namespace OwlDomain.Owl.Code.CodeAnalysis.Annotation.Symbols;

public sealed class DeclaredScopeAnnotation : CodeAnnotation
{
	#region Properties
	public override string Kind => "declared_scope_declaration";
	public IDeclaredSymbolScope Scope { get; }
	#endregion

	#region Constructors
	public DeclaredScopeAnnotation(IDeclaredSymbolScope scope)
	{
		Scope = scope;
	}
	#endregion
}

public static class DeclaredScopeAnnotationExtensions
{
	extension(IAnnotatedSyntaxNode node)
	{
		#region Methods
		public void AddScopeDeclaration(IDeclaredSymbolScope scope)
		{
			DeclaredScopeAnnotation annotation = new(scope);
			node.Annotations.Add(annotation);
		}
		public IDeclaredSymbolScope GetDeclaredScope() => node.Annotations.Get<DeclaredScopeAnnotation>().Scope;
		#endregion
	}
	extension(ISyntaxNode node)
	{
		#region Methods
		public IDeclaredSymbolScope? TryGetDeclaredScope()
		{
			if (TryGetDeclaredScope(node, out IDeclaredSymbolScope? scope))
				return scope;

			return default;
		}
		public bool TryGetDeclaredScope([NotNullWhen(true)] out IDeclaredSymbolScope? scope)
		{
			if (node.MostDetailed is not IAnnotatedSyntaxNode annotated)
			{
				scope = default;
				return false;
			}

			if (annotated.Annotations.TryGet(out DeclaredScopeAnnotation? annotation))
			{
				scope = annotation.Scope;
				return true;
			}

			scope = default;
			return false;
		}
		#endregion
	}
}
