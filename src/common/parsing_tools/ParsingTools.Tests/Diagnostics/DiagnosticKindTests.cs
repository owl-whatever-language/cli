namespace OwlDomain.ParsingTools.Tests.Diagnostics;

[TestClass]
public sealed class DiagnosticKindTests
{
	#region Constructor tests
	[TestMethod]
	public void Constructor_Parameterless_SameAsDefault()
	{
		// Arrange
		DiagnosticKind expected = default;

		// Act
		DiagnosticKind sut = new();

		// Assert
		Assert.AreEqual(expected, sut);
	}

	[TestMethod]
	public void Constructor_WithName_SetsExpectedProperties()
	{
		// Arrange
		const string expectedName = "name";

		// Act
		DiagnosticKind sut = new(expectedName);

		// Assert
		Assert.AreEqual(expectedName, sut.Name);
	}
	#endregion

	#region ToString tests
	[TestMethod]
	public void ToString_ReturnsName()
	{
		// Arrange
		const string expected = "name";
		DiagnosticKind sut = new(expected);

		// Act
		string result = sut.ToString();

		// Assert
		Assert.AreEqual(expected, result);
	}
	#endregion
}

[TestClass]
public sealed class DiagnosticKindEqualityTests : SelfEqualityTests<DiagnosticKind, DiagnosticKindEqualityTests.TestData>
{
	#region Nested types
	[ExcludeFromCodeCoverage(Justification = "Called by testing framework.")]
	public sealed class TestData : ISelfEqualityTestData<TestData, DiagnosticKind>
	{
		#region Functions
		public static IEnumerable<SelfEqualityTestData<DiagnosticKind>> GetEqualValues()
		{
			yield return new(new("name"), new("name"), "Same name");
			yield return new(default, default, "Default values");
		}
		public static IEnumerable<SelfEqualityTestData<DiagnosticKind>> GetUnequalValues()
		{
			yield return new(new("name1"), new("name2"), "Different name");
		}
		#endregion
	}
	#endregion
}
