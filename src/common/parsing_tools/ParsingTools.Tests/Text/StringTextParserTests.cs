namespace OwlDomain.ParsingTools.Tests.Text;

[TestClass]
public sealed class StringTextParserTests : TextParserTests<StringTextParser>
{
	#region Constructor tests
	[TestMethod]
	public void Constructor_WithEmptyInput_SetsExpectedProperties()
	{
		// Arrange
		const string input = "";

		// Act
		StringTextParser sut = new(input);

		// Assert
		Assert.IsTrue(sut.IsAtEnd);
		Assert.IsFalse(sut.HasRemaining);
		Assert.AreEqual(default, sut.Current);
		Assert.AreEqual(default, sut.Next);
		Assert.AreEqual(default, sut.Position);
	}

	[TestMethod]
	public void Constructor_WithSimpleInput_SetsExpectedProperties()
	{
		// Arrange
		const string input = "12";
		TextElement expectedCurrent = new('1');
		TextElement expectedNext = new('2');

		// Act
		StringTextParser sut = new(input);

		// Assert
		Assert.IsFalse(sut.IsAtEnd);
		Assert.IsTrue(sut.HasRemaining);
		Assert.AreEqual(default, sut.Position);
		Assert.AreEqual(expectedCurrent, sut.Current);
		Assert.AreEqual(expectedNext, sut.Next);
	}
	#endregion

	#region Methods
	protected override StringTextParser CreateParser(string input) => new(input);
	#endregion
}
