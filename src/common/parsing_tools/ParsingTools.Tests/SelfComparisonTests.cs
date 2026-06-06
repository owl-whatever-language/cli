namespace OwlDomain.ParsingTools.Tests;

public sealed record SelfComparisonTestData<T>(T Left, T? Right, string TestName) : ComparisonTestData<T, T?>(Left, Right, TestName) where T : notnull;

public interface ISelfComparisonTestData<TSelf, T> : IComparisonTestData<T, T>, ISelfEqualityTestData<TSelf, T>
	where TSelf : notnull, ISelfComparisonTestData<TSelf, T>
	where T : notnull
{
	#region Functions
	new static abstract IEnumerable<SelfComparisonTestData<T>> GetLesserValues();
	static IEnumerable<ComparisonTestData<T, T?>> IComparisonTestData<T, T>.GetLesserValues() => TSelf.GetLesserValues();

	new static abstract IEnumerable<SelfComparisonTestData<T>> GetGreaterValues();
	static IEnumerable<ComparisonTestData<T, T?>> IComparisonTestData<T, T>.GetGreaterValues() => TSelf.GetGreaterValues();
	#endregion
}

[TestClass]
public abstract class SelfComparisonTests<TType, TData> : ComparisonTests<TType, TType, TData>
	where TType : notnull, IComparable<TType?>, IComparisonOperators<TType, TType?, bool>
	where TData : notnull, ISelfComparisonTestData<TData, TType>
{
}
