namespace OwlDomain.ParsingTools.Tests.Text;

[TestClass]
public sealed class TextElementTests
{
	#region Constructor tests
	[TestMethod]
	public void Constructor_Parameterless_SameAsDefault()
	{
		// Arrange
		TextElement expected = default;

		// Act
		TextElement sut = new();

		// Assert
		Assert.AreEqual(expected, sut);
	}

	[TestMethod]
	public void Constructor_WithValidString_SetsExpectedProperties()
	{
		// Arrange
		const string expectedValue = "1";

		// Act
		TextElement sut = new(expectedValue);

		// Assert
		Assert.AreEqual(expectedValue, sut.Value);
	}

	[TestMethod]
	public void Constructor_WithRune_SetsExpectedProperties()
	{
		// Arrange
		string expectedValue = "1";

		// Act
		TextElement sut = new(new Rune(expectedValue[0]));

		// Assert
		Assert.AreEqual(expectedValue, sut.Value);
	}

	[TestMethod]
	public void Constructor_WithChar_SetsExpectedProperties()
	{
		// Arrange
		string expectedValue = "1";

		// Act
		TextElement sut = new(expectedValue[0]);

		// Assert
		Assert.AreEqual(expectedValue, sut.Value);
	}

	[DataRow("", DisplayName = "Empty string")]
	[DataRow("123", DisplayName = "Multiple text elements")]
	[TestMethod]
	public void Constructor_WithInvalidString_ThrowsArgumentException(string invalidValue)
	{
		// Arrange
		const string expectedParameterName = "value";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		void Act() => _ = new TextElement(invalidValue);

		// Assert
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(Act);
		Assert.AreEqual(expectedParameterName, exception.ParamName);
	}
	#endregion

	#region ToString tests
	[TestMethod]
	public void ToString_ReturnsExpectedString()
	{
		// Arrange
		const string expected = "1";
		TextElement sut = new(expected);

		// Act
		string result = sut.ToString();

		// Assert
		Assert.AreEqual(expected, result);
	}
	#endregion

	#region EnumerateTextElements
	[TestMethod]
	public void EnumerateTextElements_WithEmptyInput_ReturnsEmptyEnumerable()
	{
		// Arrange
		const string input = "";

		// Act
		TextElement[] result = TextElementExtensions.EnumerateTextElements(input).ToArray();

		// Assert
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public void EnumerateTextElements_WithSimpleInput_ReturnsExpectedEnumerable()
	{
		// Arrange
		const string input = "123";
		TextElement[] expected = [new("1"), new("2"), new("3")];

		// Act
		TextElement[] result = TextElementExtensions.EnumerateTextElements(input).ToArray();

		// Assert
		CollectionAssert.AreEqual(expected, result);
	}

	[TestMethod]
	public void EnumerateTextElements_WithComplexInput_ReturnsExpectedEnumerable()
	{
		// Arrange
		const string input = "1👨‍👩‍👧‍👦👨‍👩";
		TextElement[] expected = [new("1"), new("👨‍👩‍👧‍👦"), new("👨‍👩")];

		// Act
		TextElement[] result = TextElementExtensions.EnumerateTextElements(input).ToArray();

		// Assert
		CollectionAssert.AreEqual(expected, result);
	}
	#endregion
}

[TestClass]
public sealed class TextElementEqualityTests : SelfEqualityTests<TextElement, TextElementEqualityTests.TestData>
{
	#region Nested types
	[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
	public sealed class TestData : ISelfEqualityTestData<TestData, TextElement>
	{
		#region Methods
		public static IEnumerable<SelfEqualityTestData<TextElement>> GetEqualValues()
		{
			yield return new(new("1"), new("1"), "Same simple value");
			yield return new(new("👨‍👩‍👧‍👦"), new("👨‍👩‍👧‍👦"), "Same complex value");
			yield return new(default, default, "Default values");
		}
		public static IEnumerable<SelfEqualityTestData<TextElement>> GetUnequalValues()
		{
			yield return new(new("1"), new("0"), "Different value");
		}
		#endregion
	}
	#endregion
}

[TestClass]
public sealed class TextElementStringEqualityTests : EqualityTests<TextElement, string, TextElementStringEqualityTests.TestData>
{
	#region Nested types
	[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
	public sealed class TestData : IEqualityTestData<TextElement, string>
	{
		#region Methods
		public static IEnumerable<EqualityTestData<TextElement, string>> GetEqualValues()
		{
			yield return new(new("1"), "1", "Same value");
		}
		public static IEnumerable<EqualityTestData<TextElement, string?>> GetUnequalValues()
		{
			yield return new(new("1"), "2", "Different value");
			yield return new(new("1"), "12", "Longer value");
			yield return new(new("1"), "", "Empty value");
			yield return new(new("👨‍👩‍👧‍👦"), "👨", "Shorter value");
		}
		#endregion
	}
	#endregion
}

[TestClass]
public sealed class TextElementRuneEqualityTests : EqualityTests<TextElement, Rune, TextElementRuneEqualityTests.TestData>
{
	#region Nested types
	[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
	public sealed class TestData : IEqualityTestData<TextElement, Rune>
	{
		#region Methods
		public static IEnumerable<EqualityTestData<TextElement, Rune>> GetEqualValues()
		{
			yield return new(new("1"), new('1'), "Same value");
		}
		public static IEnumerable<EqualityTestData<TextElement, Rune>> GetUnequalValues()
		{
			yield return new(new("1"), new('2'), "Different value");
			yield return new(new("👨‍👩‍👧‍👦"), "👨‍👩‍👧‍👦".EnumerateRunes().First(), "Shorter value");
		}
		#endregion
	}
	#endregion
}

[TestClass]
public sealed class TextElementCharEqualityTests : EqualityTests<TextElement, char, TextElementCharEqualityTests.TestData>
{
	#region Nested types
	[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
	public sealed class TestData : IEqualityTestData<TextElement, char>
	{
		#region Methods
		public static IEnumerable<EqualityTestData<TextElement, char>> GetEqualValues()
		{
			yield return new(new("1"), '1', "Same value");
		}
		public static IEnumerable<EqualityTestData<TextElement, char>> GetUnequalValues()
		{
			yield return new(new("1"), '2', "Different value");
			yield return new(new("👨‍👩‍👧‍👦"), "👨‍👩‍👧‍👦"[0], "Shorter value");
		}
		#endregion
	}
	#endregion
}
