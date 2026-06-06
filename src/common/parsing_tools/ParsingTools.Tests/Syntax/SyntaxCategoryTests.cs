namespace OwlDomain.ParsingTools.Tests.Syntax;

[TestClass]
public sealed class SyntaxCategoryTests
{
	#region Constructor tests
	[TestMethod]
	public void Constructor_Parameterless_SameAsDefault()
	{
		// Arrange
		SyntaxCategory expected = default;

		// Act
		SyntaxCategory sut = new();

		// Assert
		Assert.AreEqual(expected, sut);
	}

	[TestMethod]
	public void Constructor_WithName_SetsExpectedProperties()
	{
		// Arrange
		const string expectedName = "category_name";

		// Act
		SyntaxCategory sut = new(expectedName);

		// Assert
		Assert.AreEqual(expectedName, sut.Name);
	}
	#endregion

	#region ToString tests
	[TestMethod]
	public void ToString_ReturnsName()
	{
		// Arrange
		const string expected = "category_name";
		SyntaxCategory sut = new(expected);

		// Act
		string result = sut.ToString();

		// Assert
		Assert.AreEqual(expected, result);
	}
	#endregion
}

[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
public sealed class SyntaxCategoryTestData : ISelfEqualityTestData<SyntaxCategoryTestData, SyntaxCategory>
{
	#region Functions
	public static IEnumerable<SelfEqualityTestData<SyntaxCategory>> GetEqualValues()
	{
		yield return new(new("category_name"), new("category_name"), "Same name");
		yield return new(default, default, "Default values");
	}
	public static IEnumerable<SelfEqualityTestData<SyntaxCategory>> GetUnequalValues()
	{
		yield return new(new("category_name1"), new("category_name2"), "Different name");
	}
	#endregion
}

[TestClass]
public sealed class SyntaxCategoryEqualityTests : SelfEqualityTests<SyntaxCategory, SyntaxCategoryTestData> { }
