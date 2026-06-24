namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;

public interface IType : IEquatable<IType>
{
	#region Methods
	/// <summary>Checks whether the current type can be assigned to the given <paramref name="target"/> type.</summary>
	/// <param name="target">The target type to check against.</param>
	/// <returns><see langword="true"/> if a value of the current type can be assigned to the given <paramref name="target"/> type.</returns>
	/// <remarks>This method should check things like implicit conversions and the like.</remarks>
	bool CanAssignTo(IType target);
	#endregion
}

public static class ITypeExtensions
{
	extension(IType type)
	{
		#region Methods
		public void ThrowIfInvalidShadow(IType @new, [CallerArgumentExpression(nameof(@new))] string? parameter = null)
		{
			if (type != SpecialTypes.Unknown)
				ThrowHelper.ThrowArgumentException(parameter, $"Only types that are still unknown can be replaced.");
		}
		#endregion
	}
}
