namespace OwlDomain.ParsingTools.Tests;

public record EqualityTestData<TLeft, TRight>(TLeft Left, TRight Right, string TestName) where TLeft : notnull;

public interface IEqualityTestData<TLeft, TRight>
	where TLeft : notnull
{
	#region Functions
	static abstract IEnumerable<EqualityTestData<TLeft, TRight>> GetEqualValues();
	static abstract IEnumerable<EqualityTestData<TLeft, TRight?>> GetUnequalValues();
	#endregion
}

[TestClass]
public abstract class EqualityTests<TLeft, TRight, TData>
	where TLeft : notnull, IEquatable<TRight?>, IEqualityOperators<TLeft, TRight?, bool>
	where TData : notnull, IEqualityTestData<TLeft, TRight>
{
	#region Tests
	[DynamicData(nameof(GetEqualityTestArguments))]
	[TestMethod]
	public void Equals_Typed_WithEqualValues_ReturnsTrue(TLeft sut, TRight other)
	{
		// Act
		bool result = sut.Equals(other);

		// Assert
		Assert.IsTrue(result);
	}

	[DynamicData(nameof(GetInequalityTestArguments))]
	[TestMethod]
	public void Equals_Typed_WithUnequalValues_ReturnsFalse(TLeft sut, TRight? other)
	{
		// Act
		bool result = sut.Equals(other);

		// Assert
		Assert.IsFalse(result);
	}

	[DynamicData(nameof(GetEqualityTestArguments))]
	[TestMethod]
	public void Equals_Untyped_WithEqualValues_ReturnsTrue(TLeft sut, object other)
	{
		// Act
		bool result = sut.Equals(other);

		// Assert
		Assert.IsTrue(result);
	}

	[DynamicData(nameof(GetInequalityTestArguments))]
	[TestMethod]
	public void Equals_Untyped_WithUnequalValues_ReturnsFalse(TLeft sut, object? other)
	{
		// Act
		bool result = sut.Equals(other);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Equals_Untyped_WithDifferentType_ReturnsFalse()
	{
		// Arrange
		TLeft sut = GetDefaultLeft();
		object other = new();

		// Act
		bool result = sut.Equals(other);

		// Assert
		Assert.IsFalse(result);
	}

	[DynamicData(nameof(GetEqualityTestArguments))]
	[TestMethod]
	public void EqualityOperator_WithEqualValues_ReturnsTrue(TLeft sut, TRight other)
	{
		// Act
		bool result = sut == other;

		// Assert
		Assert.IsTrue(result);
	}

	[DynamicData(nameof(GetInequalityTestArguments))]
	[TestMethod]
	public void EqualityOperator_WithUnequalValues_ReturnsFalse(TLeft sut, TRight? other)
	{
		// Act
		bool result = sut == other;

		// Assert
		Assert.IsFalse(result);
	}

	[DynamicData(nameof(GetEqualityTestArguments))]
	[TestMethod]
	public void InequalityOperator_WithEqualValues_ReturnsFalse(TLeft sut, TRight? other)
	{
		// Act
		bool result = sut != other;

		// Assert
		Assert.IsFalse(result);
	}

	[DynamicData(nameof(GetInequalityTestArguments))]
	[TestMethod]
	public void InequalityOperator_WithUnequalValues_ReturnsTrue(TLeft sut, TRight? other)
	{
		// Act
		bool result = sut != other;

		// Assert
		Assert.IsTrue(result);
	}
	#endregion

	#region Helpers
	[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
	public static IEnumerable<TestDataRow<(TLeft sut, TRight other)>> GetEqualityTestArguments()
	{
		foreach (EqualityTestData<TLeft, TRight> row in TData.GetEqualValues())
			yield return new((row.Left, row.Right)) { DisplayName = row.TestName };
	}

	[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
	public static IEnumerable<TestDataRow<(TLeft sut, TRight? other)>> GetInequalityTestArguments()
	{
		foreach (EqualityTestData<TLeft, TRight?> row in TData.GetUnequalValues())
			yield return new((row.Left, row.Right)) { DisplayName = row.TestName };
	}

	protected virtual TLeft GetDefaultLeft() => default(TLeft) ?? GetEqualityTestArguments().First().Value.sut;
	#endregion
}
