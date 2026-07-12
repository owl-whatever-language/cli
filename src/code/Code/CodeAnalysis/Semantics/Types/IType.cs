namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;

public interface IType : IEquatable<IType>, IDebugTextFactory
{
	#region Properties
	IReadOnlyCollection<ITypeProperty> Properties { get; }
	IReadOnlyCollection<ITypeMember> Members { get; }
	#endregion

	#region Methods
	/// <summary>Checks whether the current type can be assigned to the given <paramref name="target"/> type.</summary>
	/// <param name="target">The target type to check against.</param>
	/// <returns><see langword="true"/> if a value of the current type can be assigned to the given <paramref name="target"/> type.</returns>
	/// <remarks>This method should check things like implicit conversions and the like.</remarks>
	bool CanAssignTo(IType target);

	bool FindOperation(IType left, IType right, OperatorKind @operator, [NotNullWhen(true)] out IFunction? function);
	#endregion
}

public static class ITypeExtensions
{
	extension(IType type)
	{
		#region Properties
		public bool IsError => type == SpecialTypes.Error;
		public bool IsNotError => type != SpecialTypes.Error;
		public bool IsVoid => type == SpecialTypes.Void;
		#endregion

		#region Methods
		public void ThrowIfInvalidShadow(IType @new, [CallerArgumentExpression(nameof(@new))] string? parameter = null)
		{
			if (type != SpecialTypes.Unknown)
				ThrowHelper.ThrowArgumentException(parameter, $"Only types that are still unknown can be replaced.");
		}
		public IFunction? FindOperation(IType left, IType right, OperatorKind @operator)
		{
			if (type.FindOperation(left, right, @operator, out IFunction? function))
				return function;

			return default;
		}
		#endregion
	}
}
