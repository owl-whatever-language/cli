namespace OwlDomain.ParsingTools.Tests;

public sealed record SelfEqualityTestData<T>(T Left, T? Right, string TestName) : EqualityTestData<T, T?>(Left, Right, TestName) where T : notnull;

public interface ISelfEqualityTestData<TSelf, T> : IEqualityTestData<T, T>
	where TSelf : notnull, ISelfEqualityTestData<TSelf, T>
	where T : notnull
{
	#region Functions
	new static abstract IEnumerable<SelfEqualityTestData<T>> GetEqualValues();
	static IEnumerable<EqualityTestData<T, T>> IEqualityTestData<T, T>.GetEqualValues() => TSelf.GetEqualValues()!;
	new static abstract IEnumerable<SelfEqualityTestData<T>> GetUnequalValues();
	static IEnumerable<EqualityTestData<T, T?>> IEqualityTestData<T, T>.GetUnequalValues() => TSelf.GetUnequalValues();
	#endregion
}

[TestClass]
public abstract class SelfEqualityTests<TType, TData> : EqualityTests<TType, TType, TData>
	where TType : notnull, IEquatable<TType?>, IEqualityOperators<TType, TType?, bool>
	where TData : notnull, ISelfEqualityTestData<TData, TType>
{
	#region GetHashCode tests

	[DynamicData(nameof(GetEqualityTestArguments))]
	[TestMethod]
	public void GetHashCode_WithEqualValues_ReturnSameHashCode(TType sut, TType other)
	{
		// Arrange
		int expected = other.GetHashCode();

		// Act
		int result = sut.GetHashCode();

		// Assert
		Assert.AreEqual(expected, result);
	}

	[DynamicData(nameof(GetInequalityTestArguments))]
	[TestMethod]
	public void GetHashCode_WithDifferentValues_ReturnDifferentHashCodes(TType sut, TType? other)
	{
		// Arrange
		int otherHashCode = other?.GetHashCode() ?? 0;

		// Act
		int sutHashCode = sut.GetHashCode();

		// Assert
		Assert.AreNotEqual(otherHashCode, sutHashCode);
	}
	#endregion
}
