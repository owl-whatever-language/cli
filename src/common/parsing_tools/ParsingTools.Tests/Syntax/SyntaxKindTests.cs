namespace OwlDomain.ParsingTools.Tests.Syntax;

[TestClass]
public sealed class SyntaxKindTests
{
	#region Constructor tests
	[TestMethod]
	public void Constructor_Parameterless_SameAsDefault()
	{
		// Arrange
		SyntaxKind expected = default;

		// Act
		SyntaxKind sut = new();

		// Assert
		Assert.AreEqual(expected, sut);
	}

	[TestMethod]
	public void Constructor_WithValidValues_SetsExpectedProperties()
	{
		// Arrange
		const string expectedName = "kind_name";
		SyntaxCategory expectedCategory = SyntaxCategory.Token;

		// Act
		SyntaxKind sut = new(expectedName, expectedCategory);

		// Assert
		Assert.AreEqual(expectedName, sut.Name);
		Assert.AreEqual(expectedCategory, sut.Category);
	}

	[TestMethod]
	public void Constructor_WithDefaultCategory_ThrowsArgumentException()
	{
		// Arrange
		const string expectedParameterName = "category";

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		static void Act() => _ = new SyntaxKind("name", default);

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
		const string expected = "keyword_token";
		SyntaxKind sut = new("keyword", SyntaxCategory.Token);

		// Act
		string result = sut.ToString();

		// Assert
		Assert.AreEqual(expected, result);
	}
	#endregion
}

[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
public sealed class SyntaxKindTestData : ISelfEqualityTestData<SyntaxKindTestData, SyntaxKind>
{
	#region Functions
	public static IEnumerable<SelfEqualityTestData<SyntaxKind>> GetEqualValues()
	{
		yield return new(new("kind_name", SyntaxCategory.Token), new("kind_name", SyntaxCategory.Token), "Same values");
		yield return new(default, default, "Default values");
	}
	public static IEnumerable<SelfEqualityTestData<SyntaxKind>> GetUnequalValues()
	{
		yield return new(new("kind_name1", SyntaxCategory.Token), new("kind_name2", SyntaxCategory.Token), "Different name");
		yield return new(new("kind_name", SyntaxCategory.Token), new("kind_name", SyntaxCategory.Trivia), "Different category");
		yield return new(new("kind_name1", SyntaxCategory.Token), new("kind_name2", SyntaxCategory.Trivia), "Different name & category");
	}
	#endregion
}

[TestClass]
public sealed class SyntaxKindEqualityTests : SelfEqualityTests<SyntaxKind, SyntaxKindTestData> { }
