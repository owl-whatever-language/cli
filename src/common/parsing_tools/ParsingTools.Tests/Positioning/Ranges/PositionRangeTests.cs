namespace OwlDomain.ParsingTools.Tests.Positioning.Ranges;

[TestClass]
public sealed class PositionRangeTests
{
	#region Constructors
	[TestMethod]
	public void Constructor_Parameterless_SameAsDefault()
	{
		// Arrange
		PositionRange expected = default;

		// Act
		PositionRange sut = new();

		// Assert
		Assert.AreEqual(expected, sut);
	}

	[TestMethod]
	public void Constructor_WithStartAndEnd_SetsExpectedProperties()
	{
		// Arrange
		LinePosition expectedStart = new(1, 2);
		LinePosition expectedEnd = new(1, 3);

		// Act
		PositionRange sut = new(expectedStart, expectedEnd);

		// Assert
		Assert.AreEqual(expectedStart, sut.Start);
		Assert.AreEqual(expectedEnd, sut.End);
		Assert.IsFalse(sut.IsMultiline);
		Assert.AreEqual(1, sut.Lines);
	}

	[TestMethod]
	public void Constructor_WithIndividualValues_SetsExpectedProperties()
	{
		// Arrange
		const int expectedStartLine = 1;
		const int expectedStartColumn = 2;
		const int expectedEndLine = 2;
		const int expectedEndColumn = 3;

		// Act
		PositionRange sut = new(expectedStartLine, expectedStartColumn, expectedEndLine, expectedEndColumn);

		// Assert
		Assert.AreEqual(expectedStartLine, sut.Start.Line);
		Assert.AreEqual(expectedStartColumn, sut.Start.Column);
		Assert.AreEqual(expectedEndLine, sut.End.Line);
		Assert.AreEqual(expectedEndColumn, sut.End.Column);
		Assert.IsTrue(sut.IsMultiline);
		Assert.AreEqual(2, sut.Lines);
	}

	[TestMethod]
	public void Constructor_WithStartAndEnd_WithEndBeforeStart_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		LinePosition start = new(3, 4);
		LinePosition expectedEnd = new(1, 2);
		const string expectedParameterName = "end";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		void Act() => _ = new PositionRange(start, expectedEnd);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(expectedEnd, exception.ActualValue);
	}

	[TestMethod]
	public void Constructor_WithIndividualValues_EarlierEndLine_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		const int expectedEndLine = 1;
		const string expectedParameterName = "endLine";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		static void Act() => _ = new PositionRange(2, 3, expectedEndLine, 4);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(expectedEndLine, exception.ActualValue);
	}

	[TestMethod]
	public void Constructor_WithIndividualValues_SameLineEarlierEndColumn_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		const int expectedEndColumn = 1;
		const string expectedParameterName = "endColumn";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		static void Act() => _ = new PositionRange(1, 2, 1, expectedEndColumn);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(expectedEndColumn, exception.ActualValue);
	}

	[DataRow(0, 1, 1, 1, 0, "startLine", DisplayName = "Zero start line"), DataRow(-1, 1, 1, 1, -1, "startLine", DisplayName = "Negative start line")]
	[DataRow(1, 0, 1, 1, 0, "startColumn", DisplayName = "Zero start column"), DataRow(1, -1, 1, 1, -1, "startColumn", DisplayName = "Negative start column")]
	[DataRow(1, 1, 0, 1, 0, "endLine", DisplayName = "Zero end line"), DataRow(1, 1, -1, 1, -1, "endLine", DisplayName = "Negative end line")]
	[DataRow(1, 1, 1, 0, 0, "endColumn", DisplayName = "Zero end column"), DataRow(1, 1, 1, -1, -1, "endColumn", DisplayName = "Negative end column")]
	[TestMethod]
	public void Constructor_WithInvalidIndividualValues_ThrowsArgumentOutOfRangeException(
			int startLine,
			int startColumn,
			int endLine,
			int endColumn,
			int expectedValue,
			string expectedParameterName)
	{
		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		void Act() => _ = new PositionRange(startLine, startColumn, endLine, endColumn);

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
		const string expected = "1, 2 -> 3, 4";
		PositionRange sut = new(1, 2, 3, 4);

		// Act
		string result = sut.ToString();

		// Assert
		Assert.AreEqual(expected, result);
	}
	#endregion
}

[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
public sealed class PositionRangeTestData : ISelfEqualityTestData<PositionRangeTestData, PositionRange>
{
	#region Functions
	public static IEnumerable<SelfEqualityTestData<PositionRange>> GetEqualValues()
	{
		yield return new(new(1, 2, 3, 4), new(1, 2, 3, 4), "Same start & end values");
		yield return new(default, default, "Default values");
	}
	public static IEnumerable<SelfEqualityTestData<PositionRange>> GetUnequalValues()
	{
		yield return new(new(1, 2, 3, 4), new(2, 1, 3, 4), "Different start values");
		yield return new(new(1, 2, 3, 4), new(1, 2, 4, 3), "Different end values");
		yield return new(new(1, 2, 3, 4), new(5, 6, 7, 8), "Different start & end values");
	}
	#endregion
}

[TestClass]
public sealed class PositionRangeEqualityTests : SelfEqualityTests<PositionRange, PositionRangeTestData> { }
