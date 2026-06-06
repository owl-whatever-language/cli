namespace OwlDomain.ParsingTools.Tests.Positioning.Ranges;

[TestClass]
public sealed class IndexedPositionRangeTests
{
	#region Constructor tests
	[TestMethod]
	public void Constructor_Parameterless_SameAsDefault()
	{
		// Arrange
		IndexedPositionRange expected = default;

		// Act
		IndexedPositionRange sut = new();

		// Assert
		Assert.AreEqual(expected, sut);
	}

	[TestMethod]
	public void Constructor_WithStartAndEnd_SetsExpectedProperties()
	{
		// Arrange
		IndexedLinePosition expectedStart = new(1, 2, 3);

		// Column is purposefully +4 and not +3 to ensure length is derived from the index
		IndexedLinePosition expectedEnd = new(4, 2, 7);

		// Act
		IndexedPositionRange sut = new(expectedStart, expectedEnd);

		// Assert
		Assert.AreEqual(expectedStart, sut.Start);
		Assert.AreEqual(expectedEnd, sut.End);
		Assert.IsFalse(sut.IsMultiline);
		Assert.AreEqual(1, sut.Lines);
		Assert.AreEqual(3, sut.Length);
	}

	[TestMethod]
	public void Constructor_WithIndexAndStartAndEnd_SetsExpectedProperties()
	{
		// Arrange
		const int startIndex = 1;
		const int endIndex = 4;

		LinePosition startPosition = new(2, 3);

		// Column is purposefully +4 and not +3 to ensure length is derived from the index
		LinePosition endPosition = new(3, 7);

		IndexedLinePosition expectedStart = new(startIndex, startPosition);
		IndexedLinePosition expectedEnd = new(endIndex, endPosition);

		// Act
		IndexedPositionRange sut = new(startIndex, startPosition, endIndex, endPosition);

		// Assert
		Assert.AreEqual(expectedStart, sut.Start);
		Assert.AreEqual(expectedEnd, sut.End);
		Assert.IsTrue(sut.IsMultiline);
		Assert.AreEqual(2, sut.Lines);
		Assert.AreEqual(3, sut.Length);
	}

	[TestMethod]
	public void Constructor_WithIndividualValues_SetsExpectedProperties()
	{
		// Arrange
		const int startIndex = 1, startLine = 2, startColumn = 3;
		const int endIndex = 4, endLine = 3, endColumn = 5;

		IndexedLinePosition expectedStart = new(startIndex, startLine, startColumn);
		IndexedLinePosition expectedEnd = new(endIndex, endLine, endColumn);

		// Act
		IndexedPositionRange sut = new(startIndex, startLine, startColumn, endIndex, endLine, endColumn);

		// Assert
		Assert.AreEqual(expectedStart, sut.Start);
		Assert.AreEqual(expectedEnd, sut.End);
		Assert.IsTrue(sut.IsMultiline);
		Assert.AreEqual(2, sut.Lines);
		Assert.AreEqual(3, sut.Length);
	}

	[TestMethod]
	public void Constructor_WithStartAndEnd_WithEarlierEnd_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		IndexedLinePosition start = new(4, 5, 6);
		IndexedLinePosition expectedEnd = new(1, 2, 3);
		const string expectedParameterName = "end";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		void Act() => _ = new IndexedPositionRange(start, expectedEnd);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(expectedEnd, exception.ActualValue);
	}

	[TestMethod]
	public void Constructor_WithIndexAndStartAndEnd_WithEarlierEndIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		const int startIndex = 4;
		const int expectedEndIndex = 1;

		LinePosition startPosition = new(1, 2);
		LinePosition endPosition = new(1, 2);

		const string expectedParameterName = "endIndex";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		void Act() => _ = new IndexedPositionRange(startIndex, startPosition, expectedEndIndex, endPosition);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(expectedEndIndex, exception.ActualValue);
	}

	[TestMethod]
	public void Constructor_WithIndexAndStartAndEnd_WithEarlierEndPosition_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		const int startIndex = 1;
		const int endIndex = 1;

		LinePosition startPosition = new(3, 4);
		LinePosition expectedEndPosition = new(1, 2);

		const string expectedParameterName = "endPosition";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		void Act() => _ = new IndexedPositionRange(startIndex, startPosition, endIndex, expectedEndPosition);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(expectedEndPosition, exception.ActualValue);
	}

	[TestMethod]
	public void Constructor_WithIndividualValues_WithEarlierIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		const int startIndex = 4;
		const int expectedEndIndex = 1;

		const string expectedParameterName = "endIndex";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		static void Act() => _ = new IndexedPositionRange(startIndex, 1, 2, expectedEndIndex, 1, 2);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(expectedEndIndex, exception.ActualValue);
	}

	[TestMethod]
	public void Constructor_WithIndividualValues_WithEarlierLine_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		const int startLine = 4;
		const int expectedEndLine = 1;

		const string expectedParameterName = "endLine";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		static void Act() => _ = new IndexedPositionRange(1, startLine, 2, 1, expectedEndLine, 2);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(expectedEndLine, exception.ActualValue);
	}

	[TestMethod]
	public void Constructor_WithIndividualValues_WithSameLineAndEarlierColumn_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		const int startColumn = 4;
		const int expectedEndColumn = 1;

		const string expectedParameterName = "endColumn";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		static void Act() => _ = new IndexedPositionRange(1, 1, startColumn, 1, 1, expectedEndColumn);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(expectedEndColumn, exception.ActualValue);
	}

	[DataRow(1, 0, 1, 1, 1, 1, 0, "startLine", DisplayName = "Zero start line"), DataRow(1, -1, 1, 1, 1, 1, -1, "startLine", DisplayName = "Negative start line")]
	[DataRow(1, 1, 0, 1, 1, 1, 0, "startColumn", DisplayName = "Zero start column"), DataRow(1, 1, -1, 1, 1, 1, -1, "startColumn", DisplayName = "Negative start column")]
	[DataRow(1, 1, 1, 1, 0, 1, 0, "endLine", DisplayName = "Zero end line"), DataRow(1, 1, 1, 1, -1, 1, -1, "endLine", DisplayName = "Negative start line")]
	[DataRow(1, 1, 1, 1, 1, 0, 0, "endColumn", DisplayName = "Zero end column"), DataRow(1, 1, 1, 1, 1, -1, -1, "endColumn", DisplayName = "Negative end column")]
	[DataRow(-1, 1, 1, 1, 1, 1, -1, "startIndex", DisplayName = "Negative start index")]
	[DataRow(1, 1, 1, -1, 1, 1, -1, "endIndex", DisplayName = "Negative end index")]
	[TestMethod]
	public void Constructor_WithInvalidIndividualValues_ThrowsArgumentOutOfRangeException(
			int startIndex, int startLine, int startColumn,
			int endIndex, int endLine, int endColumn,
			int expectedValue, string expectedParameterName)
	{
		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		void Act() => _ = new IndexedPositionRange(startIndex, startLine, startColumn, endIndex, endLine, endColumn);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(expectedValue, exception.ActualValue);
	}
	#endregion

	#region ToString tests
	[TestMethod]
	public void ToString_ReturnsExpectedString()
	{
		// Arrange
		const string expected = "1, 2, 3 -> 4, 5, 6";
		IndexedPositionRange sut = new(1, 2, 3, 4, 5, 6);

		// Act
		string result = sut.ToString();

		// Assert
		Assert.AreEqual(expected, result);
	}
	#endregion
}

[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
public sealed class IndexedPositionRangeTestData : ISelfEqualityTestData<IndexedPositionRangeTestData, IndexedPositionRange>
{
	#region Functions
	public static IEnumerable<SelfEqualityTestData<IndexedPositionRange>> GetEqualValues()
	{
		yield return new(new(1, 2, 3, 4, 5, 6), new(1, 2, 3, 4, 5, 6), "Same values");
		yield return new(default, default, "Default values");
	}
	public static IEnumerable<SelfEqualityTestData<IndexedPositionRange>> GetUnequalValues()
	{
		yield return new(new(1, 2, 3, 4, 5, 6), new(2, 3, 1, 4, 5, 6), "Different start");
		yield return new(new(1, 2, 3, 4, 5, 6), new(1, 2, 3, 5, 6, 4), "Different end");
		yield return new(new(1, 2, 3, 4, 5, 6), new(6, 5, 4, 7, 8, 9), "Different start & end");
	}
	#endregion
}

[TestClass]
public sealed class IndexedPositionRangeEqualityTests : SelfEqualityTests<IndexedPositionRange, IndexedPositionRangeTestData> { }
