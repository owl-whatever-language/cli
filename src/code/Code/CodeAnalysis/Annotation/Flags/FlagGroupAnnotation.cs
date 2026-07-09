using System.Collections.Concurrent;

namespace OwlDomain.Owl.Code.CodeAnalysis.Annotation.Flags;

public static partial class AnnotationFlag
{

}

public sealed class FlagGroupAnnotation : CodeAnnotation
{
	#region Fields
	private readonly ConcurrentDictionary<string, bool> _flags = [];
	#endregion

	#region Properties
	public override string Kind => "flag_group";
	#endregion

	#region Methods
	public void SetFlag(string flag, bool value = true)
	{
		if (_flags.TryAdd(flag, value) is false)
			ThrowHelper.ThrowArgumentException(nameof(flag), $"The flag '{flag}' has already been set.");
	}
	public bool TryGetFlag(string flag, out bool value) => _flags.TryGetValue(flag, out value);
	public bool? TryGetFlag(string flag)
	{
		if (_flags.TryGetValue(flag, out bool value))
			return value;

		return null;
	}
	public bool GetFlag(string flag)
	{
		if (_flags.TryGetValue(flag, out bool value))
			return value;

		ThrowHelper.ThrowArgumentException(nameof(flag), $"The flag '{flag}' hasn't been set yet.");
		return default;
	}
	#endregion
}


public static partial class FlagGroupAnnotationExtensions
{
	extension(ICodeAnnotations annotations)
	{
		#region Methods
		public ICodeAnnotations SetFlag(string flag, bool value = true)
		{
			FlagGroupAnnotation flags = annotations.Ensure<FlagGroupAnnotation>();
			flags.SetFlag(flag, value);

			return annotations;
		}
		public bool TryGetFlag(string flag, out bool value)
		{
			if (annotations.TryGet(out FlagGroupAnnotation? group))
				return group.TryGetFlag(flag, out value);

			value = default;
			return false;
		}
		public bool? TryGetFlag(string flag)
		{
			if (annotations.TryGet(out FlagGroupAnnotation? group))
				return group.TryGetFlag(flag);

			return null;
		}
		public bool GetFlag(string flag)
		{
			if (annotations.TryGet(out FlagGroupAnnotation? group))
				return group.GetFlag(flag);

			ThrowHelper.ThrowArgumentException(nameof(flag), $"The flag '{flag}' hasn't been set yet.");
			return default;
		}
		#endregion
	}
	extension(IAnnotatedSyntaxNode node)
	{
		#region Methods
		public IAnnotatedSyntaxNode SetFlag(string flag, bool value = true)
		{
			node.Annotations.SetFlag(flag, value);
			return node;
		}
		public bool TryGetFlag(string flag, out bool value) => node.Annotations.TryGetFlag(flag, out value);
		public bool? TryGetFlag(string flag) => node.Annotations.TryGetFlag(flag);
		public bool GetFlag(string flag) => node.Annotations.GetFlag(flag);
		#endregion
	}
}
