namespace OwlDomain.ParsingTools.Tests.Positioning;

[TestClass]
public sealed class IndexedLinePositionTests
{
	#region Constructor tests
	[TestMethod]
	public void Constructor_Parameterless_SameAsDefault()
	{
		// Arrange
		IndexedLinePosition expected = default;

		// Act
		IndexedLinePosition sut = new();

		// Assert
		Assert.AreEqual(expected, sut);
	}

	[TestMethod]
	public void Constructor_WithPosition_SetsExpectedProperties()
	{
		// Arrange
		const int expectedIndex = 1;
		const int expectedLine = 2;
		const int expectedColumn = 3;

		LinePosition expectedPosition = new(expectedLine, expectedColumn);

		// Act
		IndexedLinePosition sut = new(expectedIndex, expectedPosition);

		// Assert
		Assert.AreEqual(expectedIndex, sut.Index);
		Assert.AreEqual(expectedPosition, sut.Position);
		Assert.AreEqual(expectedLine, sut.Line);
		Assert.AreEqual(expectedColumn, sut.Column);
	}

	[TestMethod]
	public void Constructor_WithLineAndColumn_SetsExpectedProperties()
	{
		// Arrange
		const int expectedIndex = 1;
		const int expectedLine = 2;
		const int expectedColumn = 3;

		LinePosition expectedPosition = new(expectedLine, expectedColumn);

		// Act
		IndexedLinePosition sut = new(expectedIndex, expectedLine, expectedColumn);

		// Assert
		Assert.AreEqual(expectedIndex, sut.Index);
		Assert.AreEqual(expectedPosition, sut.Position);
		Assert.AreEqual(expectedLine, sut.Line);
		Assert.AreEqual(expectedColumn, sut.Column);
	}

	[TestMethod]
	public void Constructor_WithPosition_WithInvalidIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		const int invalidIndex = -1;
		const string expectedParameterName = "index";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		static void Act() => _ = new IndexedLinePosition(invalidIndex, position: default);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(invalidIndex, exception.ActualValue);
	}

	[TestMethod]
	public void Constructor_WithLineAndColumn_WithInvalidIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		const int invalidIndex = -1;
		const string expectedParameterName = "index";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		static void Act() => _ = new IndexedLinePosition(invalidIndex, line: default, column: default);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(invalidIndex, exception.ActualValue);
	}

	[DataRow(0, DisplayName = "Zero line")]
	[DataRow(-1, DisplayName = "negative line")]
	[TestMethod]
	public void Constructor_WithInvalidLine_ThrowsArgumentOutOfRangeException(int invalidLine)
	{
		// Arrange
		const string expectedParameterName = "line";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		void Act() => _ = new IndexedLinePosition(0, invalidLine, 1);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(invalidLine, exception.ActualValue);
	}

	[DataRow(0, DisplayName = "Zero column")]
	[DataRow(-1, DisplayName = "negative column")]
	[TestMethod]
	public void Constructor_WithInvalidColumn_ThrowsArgumentOutOfRangeException(int invalidColumn)
	{
		// Arrange
		const string expectedParameterName = "column";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		void Act() => _ = new IndexedLinePosition(0, 1, invalidColumn);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(invalidColumn, exception.ActualValue);
	}
	#endregion

	#region ToString tests
	[TestMethod]
	public void ToString_ReturnsExpectedString()
	{
		// Arrange
		const string expected = "1, 2, 3";
		IndexedLinePosition sut = new(1, 2, 3);

		// Act
		string result = sut.ToString();

		// Assert
		Assert.AreEqual(expected, result);
	}
	#endregion
}

[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
public sealed class IndexedLinePositionTestData : ISelfComparisonTestData<IndexedLinePositionTestData, IndexedLinePosition>
{
	#region Functions
	public static IEnumerable<SelfEqualityTestData<IndexedLinePosition>> GetEqualValues()
	{
		yield return new(new(0, 1, 2), new(0, 1, 2), "Same values");
		yield return new(default, default, "Default values");
	}
	public static IEnumerable<SelfEqualityTestData<IndexedLinePosition>> GetUnequalValues()
	{
		yield return new(new(0, 1, 2), new(1, 1, 2), "Different index");
		yield return new(new(0, 1, 2), new(0, 2, 3), "Different position");
		yield return new(new(0, 1, 2), new(1, 2, 3), "Different index & position");
	}
	public static IEnumerable<SelfComparisonTestData<IndexedLinePosition>> GetLesserValues()
	{
		yield return new(new(0, 1, 2), new(1, 1, 2), "Lower index");
	}
	public static IEnumerable<SelfComparisonTestData<IndexedLinePosition>> GetGreaterValues()
	{
		yield return new(new(1, 1, 2), new(0, 1, 2), "Greater index");
	}
	#endregion
}

[TestClass]
public sealed class IndexedLinePositionEqualityTests : SelfEqualityTests<IndexedLinePosition, IndexedLinePositionTestData> { }

[TestClass]
public sealed class IndexedLinePositionComparisonTests : SelfComparisonTests<IndexedLinePosition, IndexedLinePositionTestData> { }
