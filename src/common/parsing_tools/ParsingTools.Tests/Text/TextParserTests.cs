namespace OwlDomain.ParsingTools.Tests.Text;

[TestClass]
public abstract class TextParserTests<T>
	where T : notnull, ITextParser
{
	#region Advance tests
	[DataRow(0, DisplayName = "Zero advance")]
	[DataRow(-1, DisplayName = "Negative advance")]
	[TestMethod]
	public void Advance_WithInvalidAmount_ThrowsArgumentOutOfRangeException(int invalidAmount)
	{
		// Arrange
		const string expectedParameterName = "amount";
		T sut = CreateParser("");

		// Act
		void Act() => _ = sut.Advance(invalidAmount);

		// Assert
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(Act);

		Assert.AreEqual(expectedParameterName, exception.ParamName);
		Assert.AreEqual(invalidAmount, exception.ActualValue);
	}

	[TestMethod]
	public void Advance_WithRemaining_WithSingleAdvance_HasExpectedEndState()
	{
		// Arrange
		const string input = "123";
		IndexedLinePosition expectedPosition = new(1, 1, 2);
		TextElement expectedCurrent = new('2');
		TextElement expectedNext = new('3');

		T sut = CreateParser(input);

		// Act
		bool result = sut.Advance();

		// Assert
		Assert.IsTrue(result);
		Assert.IsTrue(sut.HasRemaining);
		Assert.IsFalse(sut.IsAtEnd);
		Assert.AreEqual(expectedPosition, sut.Position);
		Assert.AreEqual(expectedCurrent, sut.Current);
		Assert.AreEqual(expectedNext, sut.Next);
	}

	[TestMethod]
	public void Advance_WithRemaining_WithMultipleAdvance_HasExpectedEndState()
	{
		// Arrange
		const string input = "123";
		const int advance = 3;
		IndexedLinePosition expectedPosition = new(advance, 1, advance + 1); // column is 1-based

		T sut = CreateParser(input);

		// Act
		bool result = sut.Advance(advance);

		// Assert
		Assert.IsTrue(result);
		Assert.IsFalse(sut.HasRemaining);
		Assert.IsTrue(sut.IsAtEnd);
		Assert.AreEqual(expectedPosition, sut.Position);
		Assert.AreEqual(default, sut.Current);
		Assert.AreEqual(default, sut.Next);
	}

	[TestMethod]
	public void Advance_AtEnd_ReturnsFalse()
	{
		// Arrange
		string input = "123";
		T sut = CreateParser(input);

		sut.Advance(input.Length);
		Assert.IsTrue(sut.IsAtEnd);

		IndexedLinePosition startPosition = sut.Position;

		// Act
		bool result = sut.Advance();

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(startPosition, sut.Position);
	}

	[DataRow("\r", DisplayName = "CR")]
	[DataRow("\n", DisplayName = "LF")]
	[DataRow("\r\n", DisplayName = "CRLF")]
	[TestMethod]
	public void Advance_AcrossNewLine_MarksNewLine(string lineBreak)
	{
		// Arrange
		string input = "12" + lineBreak + "34";
		int advance = 3;

		IndexedLinePosition expectedPosition = new(
			index: advance,
			line: 2,
			column: 1);

		T sut = CreateParser(input);

		// Act
		bool result = sut.Advance(advance);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expectedPosition, sut.Position);
	}
	#endregion

	#region Peek tests
	[DataRow(0, "1", DisplayName = "No offset")]
	[DataRow(1, "2", DisplayName = "Single offset")]
	[DataRow(2, "👨‍👩‍👧‍👦", DisplayName = "Multiple offset")]
	[TestMethod]
	public void Peek_WithRemaining_ReturnsExpectedValue(int offset, string expectedValue)
	{
		// Arrange
		const string input = "12👨‍👩‍👧‍👦4";
		TextElement expected = new(expectedValue);

		T sut = CreateParser(input);

		// Act
		TextElement result = sut.Peek(offset);

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void Peek_WithRemaining_WithOffsetPastEnd_ReturnsDefault()
	{
		// Arrange
		const string input = "123";
		T sut = CreateParser(input);
		int offset = input.Length;

		// Act
		TextElement result = sut.Peek(offset);

		// Assert
		Assert.AreEqual(default, result);
	}

	[TestMethod]
	public void Peek_AtEnd_ReturnsDefault()
	{
		// Arrange
		const string input = "";
		T sut = CreateParser(input);

		// Act
		TextElement result = sut.Peek(0);

		// Assert
		Assert.AreEqual(default, result);
	}
	#endregion

	#region Match tests
	[DataRow("1", DisplayName = "Simple")]
	[DataRow("👨‍👩‍👧‍👦", DisplayName = "Complex")]
	[TestMethod]
	public void Match_WithTextElement_WithMatch_ReturnsTrue(string input)
	{
		// Arrange
		T sut = CreateParser(input);

		TextElement match = new(input);
		IndexedLinePosition expectedPosition = new(1, 1, 2);

		// Act
		bool result = sut.Match(match);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expectedPosition, sut.Position);
	}

	[DataRow("1", DisplayName = "Simple")]
	[DataRow("👨‍👩‍👧‍👦", DisplayName = "Complex")]
	[TestMethod]
	public void Match_WithString_WithMatch_ReturnsTrue(string input)
	{
		// Arrange
		T sut = CreateParser(input);

		IndexedLinePosition expectedPosition = new(1, 1, 2);

		// Act
		bool result = sut.Match(input);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expectedPosition, sut.Position);
	}

	[TestMethod]
	public void Match_WithRune_WithMatch_ReturnsTrue()
	{
		// Arrange
		const string input = "1";
		T sut = CreateParser(input);

		IndexedLinePosition expectedPosition = new(1, 1, 2);
		Rune match = input.EnumerateRunes().Single();

		// Act
		bool result = sut.Match(match);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expectedPosition, sut.Position);
	}

	[TestMethod]
	public void Match_WithChar_WithMatch_ReturnsTrue()
	{
		// Arrange
		const string input = "1";
		T sut = CreateParser(input);

		IndexedLinePosition expectedPosition = new(1, 1, 2);
		char match = input.Single();

		// Act
		bool result = sut.Match(match);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expectedPosition, sut.Position);
	}

	[TestMethod]
	public void Match_WithTextElement_WithoutMatch_ReturnsFalse()
	{
		// Arrange
		T sut = CreateParser("1");

		TextElement match = default;
		IndexedLinePosition startPosition = sut.Position;

		// Act
		bool result = sut.Match(match);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(startPosition, sut.Position);
	}

	[TestMethod]
	public void Match_WithString_WithoutMatch_ReturnsFalse()
	{
		// Arrange
		T sut = CreateParser("1");

		string match = "";
		IndexedLinePosition startPosition = sut.Position;

		// Act
		bool result = sut.Match(match);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(startPosition, sut.Position);
	}

	[TestMethod]
	public void Match_WithRune_WithoutMatch_ReturnsFalse()
	{
		// Arrange
		T sut = CreateParser("1");

		Rune match = default;
		IndexedLinePosition startPosition = sut.Position;

		// Act
		bool result = sut.Match(match);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(startPosition, sut.Position);
	}

	[TestMethod]
	public void Match_WithChar_WithoutMatch_ReturnsFalse()
	{
		// Arrange
		T sut = CreateParser("1");

		char match = default;
		IndexedLinePosition startPosition = sut.Position;

		// Act
		bool result = sut.Match(match);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(startPosition, sut.Position);
	}
	#endregion

	#region MatchSequence tests
	[DataRow("1", 1, DisplayName = "Simple")]
	[DataRow("👨‍👩‍👧‍👦", 1, DisplayName = "Complex")]
	[DataRow("1👨‍👩‍👧‍👦", 2, DisplayName = "Simple & Complex")]
	[TestMethod]
	public void MatchSequence_WithTextElement_WithMatch_ReturnsTrue(string input, int advance)
	{
		// Arrange
		ReadOnlySpan<TextElement> sequence = input.EnumerateTextElements().ToArray().AsSpan();
		T sut = CreateParser(input);

		IndexedLinePosition expectedPosition = new(advance, 1, advance + 1); // column is 1-based

		// Act
		bool result = sut.MatchSequence(sequence);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expectedPosition, sut.Position);
	}

	[DataRow("1", 1, DisplayName = "Simple")]
	[DataRow("👨‍👩‍👧‍👦", 1, DisplayName = "Complex")]
	[DataRow("1👨‍👩‍👧‍👦", 2, DisplayName = "Simple & Complex")]
	[TestMethod]
	public void MatchSequence_WithString_WithMatch_ReturnsTrue(string input, int advance)
	{
		// Arrange
		T sut = CreateParser(input);
		IndexedLinePosition expectedPosition = new(advance, 1, advance + 1); // column is 1-based

		// Act
		bool result = sut.MatchSequence(input);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expectedPosition, sut.Position);
	}

	[DataRow("1", 1, DisplayName = "Simple")]
	[DataRow("👨‍👩‍👧‍👦", 1, DisplayName = "Complex")]
	[DataRow("1👨‍👩‍👧‍👦", 2, DisplayName = "Simple & Complex")]
	[TestMethod]
	public void MatchSequence_WithRune_WithMatch_ReturnsTrue(string input, int advance)
	{
		// Arrange
		ReadOnlySpan<Rune> sequence = input.EnumerateRunes().ToArray().AsSpan();
		T sut = CreateParser(input);

		IndexedLinePosition expectedPosition = new(advance, 1, advance + 1); // column is 1-based

		// Act
		bool result = sut.MatchSequence(sequence);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expectedPosition, sut.Position);
	}

	[DataRow("1", 1, DisplayName = "Simple")]
	[DataRow("👨‍👩‍👧‍👦", 1, DisplayName = "Complex")]
	[DataRow("1👨‍👩‍👧‍👦", 2, DisplayName = "Simple & Complex")]
	[TestMethod]
	public void MatchSequence_WithChar_WithMatch_ReturnsTrue(string input, int advance)
	{
		// Arrange
		ReadOnlySpan<char> sequence = input;
		T sut = CreateParser(input);

		IndexedLinePosition expectedPosition = new(advance, 1, advance + 1); // column is 1-based

		// Act
		bool result = sut.MatchSequence(sequence);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expectedPosition, sut.Position);
	}

	#region Constructors
	[TestMethod]
	public void MatchSequence_WithTextElement_WithoutMatch_ReturnsFalse()
	{
		// Arrange
		T sut = CreateParser("1");

		ReadOnlySpan<TextElement> sequence = [default];
		IndexedLinePosition startPosition = sut.Position;

		// Act
		bool result = sut.MatchSequence(sequence);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(startPosition, sut.Position);
	}

	[TestMethod]
	public void MatchSequence_WithString_WithoutMatch_ReturnsFalse()
	{
		// Arrange
		T sut = CreateParser("1");

		string sequence = "\0";
		IndexedLinePosition startPosition = sut.Position;

		// Act
		bool result = sut.MatchSequence(sequence);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(startPosition, sut.Position);
	}

	[TestMethod]
	public void MatchSequence_WithRune_WithoutMatch_ReturnsFalse()
	{
		// Arrange
		T sut = CreateParser("1");

		ReadOnlySpan<Rune> sequence = [default];
		IndexedLinePosition startPosition = sut.Position;

		// Act
		bool result = sut.MatchSequence(sequence);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(startPosition, sut.Position);
	}

	[TestMethod]
	public void MatchSequence_WithChar_WithoutMatch_ReturnsFalse()
	{
		// Arrange
		T sut = CreateParser("1");

		ReadOnlySpan<char> sequence = [default];
		IndexedLinePosition startPosition = sut.Position;

		// Act
		bool result = sut.MatchSequence(sequence);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(startPosition, sut.Position);
	}
	#endregion

	[TestMethod]
	public void MatchSequence_WithTextElement_WithEmptySequence_ThrowsArgumentException()
	{
		// Arrange
		const string expectedParameterName = "sequence";

		T sut = CreateParser("");

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		void Act()
		{
			ReadOnlySpan<TextElement> sequence = [];
			_ = sut.MatchSequence(sequence);
		}

		// Assert
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(Act);
		Assert.AreEqual(expectedParameterName, exception.ParamName);
	}

	[TestMethod]
	public void MatchSequence_WithString_WithEmptySequence_ThrowsArgumentException()
	{
		// Arrange
		const string expectedParameterName = "sequence";

		T sut = CreateParser("");

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		void Act()
		{
			string sequence = "";
			_ = sut.MatchSequence(sequence);
		}

		// Assert
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(Act);
		Assert.AreEqual(expectedParameterName, exception.ParamName);
	}

	[TestMethod]
	public void MatchSequence_WithRune_WithEmptySequence_ThrowsArgumentException()
	{
		// Arrange
		const string expectedParameterName = "sequence";

		T sut = CreateParser("");

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		void Act()
		{
			ReadOnlySpan<Rune> sequence = [];
			_ = sut.MatchSequence(sequence);
		}

		// Assert
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(Act);
		Assert.AreEqual(expectedParameterName, exception.ParamName);
	}

	[TestMethod]
	public void MatchSequence_WithChar_WithEmptySequence_ThrowsArgumentException()
	{
		// Arrange
		const string expectedParameterName = "sequence";

		T sut = CreateParser("");

		// Act
		[ExcludeFromCodeCoverage(Justification = "Assignment is never done.")]
		void Act()
		{
			ReadOnlySpan<char> sequence = [];
			_ = sut.MatchSequence(sequence);
		}

		// Assert
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(Act);
		Assert.AreEqual(expectedParameterName, exception.ParamName);
	}
	#endregion

	#region MatchAny tests
	[TestMethod]
	public void MatchAny_WithTextElement_WithMatch_ReturnsTrue()
	{
		// Arrange
		const string input = "2";

		TextElement expectedMatch = new(input);
		ReadOnlySpan<TextElement> values = [
			new("1"),
			expectedMatch,
			new("3"),
		];

		IndexedLinePosition expectedPosition = new(1, 1, 2);

		T sut = CreateParser(input);

		// Act
		bool result = sut.MatchAny(values, out TextElement match);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expectedPosition, sut.Position);
		Assert.AreEqual(expectedMatch, match);
	}

	[TestMethod]
	public void MatchAny_WithString_WithMatch_ReturnsTrue()
	{
		// Arrange
		const string input = "2";

		string expectedMatch = input;
		ReadOnlySpan<string> values = [
			"1",
			expectedMatch,
			"3",
		];

		IndexedLinePosition expectedPosition = new(1, 1, 2);

		T sut = CreateParser(input);

		// Act
		bool result = sut.MatchAny(values, out string? match);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expectedPosition, sut.Position);
		Assert.IsNotNull(match);
		Assert.AreEqual(expectedMatch, match);
	}

	[TestMethod]
	public void MatchAny_WithRune_WithMatch_ReturnsTrue()
	{
		// Arrange
		const string input = "2";

		Rune expectedMatch = input.EnumerateRunes().Single();
		ReadOnlySpan<Rune> values = [
			new('1'),
			expectedMatch,
			new('3'),
		];

		IndexedLinePosition expectedPosition = new(1, 1, 2);

		T sut = CreateParser(input);

		// Act
		bool result = sut.MatchAny(values, out Rune match);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expectedPosition, sut.Position);
		Assert.AreEqual(expectedMatch, match);
	}

	[TestMethod]
	public void MatchAny_WithChar_WithMatch_ReturnsTrue()
	{
		// Arrange
		const string input = "2";

		char expectedMatch = input.Single();
		ReadOnlySpan<char> values = [
			'1',
			expectedMatch,
			'3',
		];

		IndexedLinePosition expectedPosition = new(1, 1, 2);

		T sut = CreateParser(input);

		// Act
		bool result = sut.MatchAny(values, out char match);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expectedPosition, sut.Position);
		Assert.AreEqual(expectedMatch, match);
	}

	[TestMethod]
	public void MatchAny_WithTextElement_WithoutMatch_ReturnsFalse()
	{
		// Arrange
		ReadOnlySpan<TextElement> values = [
			new("1"),
			new("2"),
		];

		T sut = CreateParser("");
		IndexedLinePosition startPosition = sut.Position;

		// Act
		bool result = sut.MatchAny(values, out TextElement match);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(startPosition, sut.Position);
		Assert.AreEqual(default, match);
	}

	[TestMethod]
	public void MatchAny_WithString_WithoutMatch_ReturnsFalse()
	{
		// Arrange
		ReadOnlySpan<string> values = [
			"1",
			"2",
		];

		T sut = CreateParser("");
		IndexedLinePosition startPosition = sut.Position;

		// Act
		bool result = sut.MatchAny(values, out string? match);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(startPosition, sut.Position);
		Assert.IsNull(match);
	}

	[TestMethod]
	public void MatchAny_WithRune_WithoutMatch_ReturnsFalse()
	{
		// Arrange
		ReadOnlySpan<Rune> values = [
			new('1'),
			new('2'),
		];

		T sut = CreateParser("");
		IndexedLinePosition startPosition = sut.Position;

		// Act
		bool result = sut.MatchAny(values, out Rune match);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(startPosition, sut.Position);
		Assert.AreEqual(default, match);
	}

	[TestMethod]
	public void MatchAny_WithChar_WithoutMatch_ReturnsFalse()
	{
		// Arrange
		ReadOnlySpan<char> values = [
			'1',
			'2',
		];

		T sut = CreateParser("");
		IndexedLinePosition startPosition = sut.Position;

		// Act
		bool result = sut.MatchAny(values, out char match);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(startPosition, sut.Position);
		Assert.AreEqual(default, match);
	}
	#endregion

	#region Methods
	protected abstract T CreateParser(string input);
	#endregion
}
