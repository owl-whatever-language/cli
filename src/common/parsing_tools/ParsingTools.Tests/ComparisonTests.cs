namespace OwlDomain.ParsingTools.Tests;

public record ComparisonTestData<TLeft, TRight>(TLeft Left, TRight Right, string TestName) where TLeft : notnull;

public interface IComparisonTestData<TLeft, TRight> : IEqualityTestData<TLeft, TRight>
	where TLeft : notnull
{
	#region Functions
	static abstract IEnumerable<ComparisonTestData<TLeft, TRight?>> GetLesserValues();
	static abstract IEnumerable<ComparisonTestData<TLeft, TRight?>> GetGreaterValues();
	#endregion
}

[TestClass]
public abstract class ComparisonTests<TLeft, TRight, TData>
	where TLeft : notnull, IComparable<TRight?>, IComparisonOperators<TLeft, TRight?, bool>
	where TData : notnull, IComparisonTestData<TLeft, TRight>
{
	#region Tests
	[DynamicData(nameof(GetLessThanTestArguments))]
	[TestMethod]
	public void CompareTo_WithLesserValue_ReturnsNegative(TLeft sut, TRight? other)
	{
		// Act
		int result = sut.CompareTo(other);

		// Assert
		Assert.IsLessThan(0, result);
	}

	[DynamicData(nameof(GetGreaterThanTestArguments))]
	[TestMethod]
	public void CompareTo_WithGreaterValue_ReturnsPositive(TLeft sut, TRight? other)
	{
		// Act
		int result = sut.CompareTo(other);

		// Assert
		Assert.IsGreaterThan(0, result);
	}

	[DynamicData(nameof(GetEqualTestArguments))]
	[TestMethod]
	public void CompareTo_WithEqualValue_ReturnsZero(TLeft sut, TRight? other)
	{
		// Act
		const int expected = 0;
		int result = sut.CompareTo(other);

		// Assert
		Assert.AreEqual(expected, result);
	}

	[DynamicData(nameof(GetLessThanTestArguments))]
	[TestMethod]
	public void LessThanOperator_WithLesserValue_ReturnsTrue(TLeft sut, TRight? other)
	{
		// Act
		bool result = sut < other;

		// Assert
		Assert.IsTrue(result);
	}

	[DynamicData(nameof(GetGreaterThanOrEqualToTestArguments))]
	[TestMethod]
	public void LessThanOperator_WithGreaterOrEqualValue_ReturnsFalse(TLeft sut, TRight? other)
	{
		// Act
		bool result = sut < other;

		// Assert
		Assert.IsFalse(result);
	}

	[DynamicData(nameof(GetLessThanTestArguments))]
	[TestMethod]
	public void LessThanOrEqualToOperator_WithLesserOrEqualToValue_ReturnsTrue(TLeft sut, TRight? other)
	{
		// Act
		bool result = sut <= other;

		// Assert
		Assert.IsTrue(result);
	}

	[DynamicData(nameof(GetGreaterThanTestArguments))]
	[TestMethod]
	public void LessThanOrEqualToOperator_WithGreaterValue_ReturnsFalse(TLeft sut, TRight? other)
	{
		// Act
		bool result = sut <= other;

		// Assert
		Assert.IsFalse(result);
	}

	[DynamicData(nameof(GetGreaterThanTestArguments))]
	[TestMethod]
	public void GreaterThanOperator_WithGreaterValue_ReturnsTrue(TLeft sut, TRight? other)
	{
		// Act
		bool result = sut > other;

		// Assert
		Assert.IsTrue(result);
	}

	[DynamicData(nameof(GetLessThanOrEqualToTestArguments))]
	[TestMethod]
	public void GreaterThanOperator_WithLesserThanOrEqualValue_ReturnsFalse(TLeft sut, TRight? other)
	{
		// Act
		bool result = sut > other;

		// Assert
		Assert.IsFalse(result);
	}

	[DynamicData(nameof(GetGreaterThanTestArguments))]
	[TestMethod]
	public void GreaterThanOrEqualToOperator_WithGreaterOrEqualToValue_ReturnsTrue(TLeft sut, TRight? other)
	{
		// Act
		bool result = sut >= other;

		// Assert
		Assert.IsTrue(result);
	}

	[DynamicData(nameof(GetLessThanTestArguments))]
	[TestMethod]
	public void GreaterThanOrEqualToOperator_WithLesserValue_ReturnsFalse(TLeft sut, TRight? other)
	{
		// Act
		bool result = sut >= other;

		// Assert
		Assert.IsFalse(result);
	}
	#endregion

	#region Helpers
	[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
	public static IEnumerable<TestDataRow<(TLeft sut, TRight? other)>> GetLessThanTestArguments()
	{
		foreach (ComparisonTestData<TLeft, TRight?> row in TData.GetLesserValues())
			yield return new((row.Left, row.Right)) { DisplayName = row.TestName };
	}

	[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
	public static IEnumerable<TestDataRow<(TLeft sut, TRight? other)>> GetGreaterThanTestArguments()
	{
		foreach (ComparisonTestData<TLeft, TRight?> row in TData.GetGreaterValues())
			yield return new((row.Left, row.Right)) { DisplayName = row.TestName };
	}

	[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
	public static IEnumerable<TestDataRow<(TLeft sut, TRight other)>> GetEqualTestArguments()
	{
		foreach (EqualityTestData<TLeft, TRight> row in TData.GetEqualValues())
			yield return new((row.Left, row.Right)) { DisplayName = row.TestName };
	}

	[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
	public static IEnumerable<TestDataRow<(TLeft sut, TRight? other)>> GetLessThanOrEqualToTestArguments()
	{
		foreach (var row in GetLessThanTestArguments())
			yield return row;

		foreach (var row in GetEqualTestArguments())
			yield return new(row.Value) { DisplayName = row.DisplayName };
	}

	[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
	public static IEnumerable<TestDataRow<(TLeft sut, TRight? other)>> GetGreaterThanOrEqualToTestArguments()
	{
		foreach (var row in GetGreaterThanTestArguments())
			yield return row;

		foreach (var row in GetEqualTestArguments())
			yield return new(row.Value) { DisplayName = row.DisplayName };
	}
	#endregion
}
