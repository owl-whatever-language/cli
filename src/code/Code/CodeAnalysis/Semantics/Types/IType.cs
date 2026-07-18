namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;

public interface IType : IEquatable<IType>, IDebugTextFactory
{
	#region Properties
	IReadOnlyCollection<ITypeMember> Members { get; }
	IReadOnlyCollection<ITypeProperty> Properties { get; }
	IReadOnlyCollection<ITypeMethod> Methods { get; }
	IReadOnlyCollection<IBinaryOperator> BinaryOperators { get; }
	#endregion

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
		#region Properties
		public bool IsError => type == SpecialTypes.Error;
		public bool IsNotError => type != SpecialTypes.Error;
		public bool IsVoid => type == SpecialTypes.Void;
		public bool IsNotVoid => type != SpecialTypes.Void;
		#endregion

		#region Methods
		public void ThrowIfInvalidShadow(IType @new, [CallerArgumentExpression(nameof(@new))] string? parameter = null)
		{
			if (type != SpecialTypes.Unknown)
				ThrowHelper.ThrowArgumentException(parameter, $"Only types that are still unknown can be replaced.");
		}
		public IBinaryOperator? FindOperation(IType left, IType right, OperatorKind kind)
		{
			foreach (IBinaryOperator operation in type.BinaryOperators)
			{
				if (operation.Left.Equals(left) && operation.Right.Equals(right) && operation.Kind == kind)
					return operation;
			}

			return null;
		}
		public bool TryFindOperation(IType left, IType right, OperatorKind kind, [NotNullWhen(true)] out IBinaryOperator? operation)
		{
			operation = FindOperation(type, left, right, kind);
			return operation is not null;
		}
		#endregion
	}
}
