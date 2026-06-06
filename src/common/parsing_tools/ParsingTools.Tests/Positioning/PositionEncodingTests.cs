namespace OwlDomain.ParsingTools.Tests.Positioning;

[TestClass]
public sealed class PositionEncodingTests
{
	#region Constructor tests
	[TestMethod]
	public void Constructor_Parameterless_SameAsDefault()
	{
		// Arrange
		PositionEncoding expected = default;

		// Act
		PositionEncoding sut = new();

		// Assert
		Assert.AreEqual(expected, sut);
	}

	[TestMethod]
	public void Constructor_WithName_SetsExpectedProperties()
	{
		// Arrange
		const string expectedName = "encoding_name";

		// Act
		PositionEncoding sut = new(expectedName);

		// Assert
		Assert.AreEqual(expectedName, sut.Name);
	}
	#endregion

	#region ToString tests
	[TestMethod]
	public void ToString_ReturnsName()
	{
		// Arrange
		const string expected = "encoding_name";
		PositionEncoding sut = new(expected);

		// Act
		string result = sut.ToString();

		// Assert
		Assert.AreEqual(expected, result);
	}
	#endregion

	#region Constant tests
	[TestMethod]
	public void Constant_Ascii_HasExpectedName()
	{
		// Arrange
		const string expected = "ascii";
		PositionEncoding sut = PositionEncoding.Ascii;

		// Act
		string result = sut.Name;

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void Constant_Utf8_HasExpectedName()
	{
		// Arrange
		const string expected = "utf-8";
		PositionEncoding sut = PositionEncoding.Utf8;

		// Act
		string result = sut.Name;

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void Constant_Utf16_HasExpectedName()
	{
		// Arrange
		const string expected = "utf-16";
		PositionEncoding sut = PositionEncoding.Utf16;

		// Act
		string result = sut.Name;

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void Constant_Utf32_HasExpectedName()
	{
		// Arrange
		const string expected = "utf-32";
		PositionEncoding sut = PositionEncoding.Utf32;

		// Act
		string result = sut.Name;

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void Constant_TextElement_HasExpectedName()
	{
		// Arrange
		const string expected = "text_element";
		PositionEncoding sut = PositionEncoding.TextElement;

		// Act
		string result = sut.Name;

		// Assert
		Assert.AreEqual(expected, result);
	}
	#endregion
}

[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
public sealed class PositionEncodingTestData : ISelfEqualityTestData<PositionEncodingTestData, PositionEncoding>
{
	#region Functions
	public static IEnumerable<SelfEqualityTestData<PositionEncoding>> GetEqualValues()
	{
		yield return new(new("encoding_name"), new("encoding_name"), "Same name");
		yield return new(default, default, "Default values");
	}
	public static IEnumerable<SelfEqualityTestData<PositionEncoding>> GetUnequalValues()
	{
		yield return new(new("encoding_name1"), new("encoding_name2"), "Different name");
	}
	#endregion
}

[TestClass]
public sealed class PositionEncodingEqualityTests : SelfEqualityTests<PositionEncoding, PositionEncodingTestData> { }
