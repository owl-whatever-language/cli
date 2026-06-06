namespace OwlDomain.ParsingTools.Tests.Positioning;

[TestClass]
public sealed class LinePositionTests
{
	#region Constructor tests
	[TestMethod]
	public void Constructor_Parameterless_CreatesDefaultLinePosition()
	{
		// Arrange
		LinePosition expected = default;

		// Act
		LinePosition sut = new();

		// Assert
		Assert.AreEqual(expected, sut);
	}

	[TestMethod]
	public void Constructor_WithValidArguments_SetsExpectedProperties()
	{
		// Arrange
		const int expectedLine = 2;
		const int expectedColumn = 3;

		// Act
		LinePosition sut = new(expectedLine, expectedColumn);

		// Assert
		Assert.AreEqual(expectedLine, sut.Line);
		Assert.AreEqual(expectedColumn, sut.Column);
	}

	[DataRow(0, 1, "line", DisplayName = "Invalid line")]
	[DataRow(1, 0, "column", DisplayName = "Invalid column")]
	[DataRow(-1, -1, "line", DisplayName = "Invalid line & column")]
	[TestMethod]
	public void Constructor_WithInvalidArguments_ThrowsArgumentOutOfRangeException(int line, int column, string parameterName)
	{
		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment never happens since it throws an exception.")]
		void Act() => _ = new LinePosition(line, column);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);
		Assert.AreEqual(parameterName, exception.ParamName);
	}
	#endregion

	#region ToString tests
	[TestMethod]
	public void ToString_ReturnsExpectedLineAndColumn()
	{
		// Arrange
		LinePosition sut = new(1, 1);
		const string expected = "1, 1";

		// Act
		string result = sut.ToString();

		// Assert
		Assert.AreEqual(expected, result);
	}
	#endregion
}

[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
public sealed class LinePositionTestData : ISelfComparisonTestData<LinePositionTestData, LinePosition>
{
	#region Functions
	public static IEnumerable<SelfEqualityTestData<LinePosition>> GetEqualValues()
	{
		yield return new(new(1, 2), new(1, 2), "Same line & column");
		yield return new(default, default, "Default values");
	}
	public static IEnumerable<SelfEqualityTestData<LinePosition>> GetUnequalValues()
	{
		yield return new(new(1, 2), new(3, 2), "Different line");
		yield return new(new(1, 2), new(1, 3), "Different column");
		yield return new(new(1, 2), new(3, 4), "Different line & column");
	}
	public static IEnumerable<SelfComparisonTestData<LinePosition>> GetLesserValues()
	{
		yield return new(new(1, 1), new(2, 1), "Earlier line");
		yield return new(new(1, 1), new(1, 2), "Same line, earlier column");
	}
	public static IEnumerable<SelfComparisonTestData<LinePosition>> GetGreaterValues()
	{
		yield return new(new(2, 1), new(1, 1), "Later line");
		yield return new(new(1, 2), new(1, 1), "Same line, later column");
	}
	#endregion
}

[TestClass]
public sealed class LinePositionEqualityTests : SelfEqualityTests<LinePosition, LinePositionTestData> { }

[TestClass]
public sealed class LinePositionComparisonTests : SelfComparisonTests<LinePosition, LinePositionTestData> { }
