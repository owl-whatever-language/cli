namespace OwlDomain.Owl.Code.CodeAnalysis.Annotation.Flags;

partial class AnnotationFlag
{
	#region Constants
	public const string IsReachable = "is_reachable";
	#endregion
}

static partial class FlagGroupAnnotationExtensions
{
	extension(IAnnotatedSyntaxNode node)
	{
		#region Methods
		public bool? IsReachable()
		{
			ISyntaxNode? current = node;
			while (current is not null)
			{
				if (current is IAnnotatedSyntaxNode annotated && annotated.TryGetFlag(AnnotationFlag.IsReachable, out bool value))
					return value;

				current = current.Parent;
			}

			return null;
		}
		#endregion
	}

	extension(TextFragment fragment)
	{
		#region Methods
		public TextFragment MarkUnreachable()
		{
			if (fragment.Syntax is not IAnnotatedSyntaxNode annotated)
				return fragment;

			if (annotated.IsReachable() is false)
				return fragment.With(ClassificationKind.Unreachable);

			return fragment;
		}
		#endregion
	}
	extension(TextFragmentLine fragments)
	{
		#region Methods
		public TextFragmentLine MarkUnreachable() => fragments.Replace(MarkUnreachable);
		#endregion
	}
	extension(TextFragmentLineCollection fragments)
	{
		#region Methods
		public TextFragmentLineCollection MarkUnreachable() => fragments.Replace(MarkUnreachable);
		#endregion
	}
}
