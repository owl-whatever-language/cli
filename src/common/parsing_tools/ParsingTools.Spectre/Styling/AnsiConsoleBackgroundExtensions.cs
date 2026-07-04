namespace OwlDomain.ParsingTools.Styling;

public static class AnsiConsoleBackgroundExtensions
{
	#region Nested types
	public readonly struct ConsoleBackgroundScope(Color oldColor) : IDisposable
	{
		#region Methods
		public void Dispose() => AnsiConsole.Background = oldColor;
		#endregion
	}
	#endregion

	extension(AnsiConsole)
	{
		#region Functions
		public static ConsoleBackgroundScope WithBackground(Color newBackground)
		{
			Color old = AnsiConsole.Background;
			AnsiConsole.Background = newBackground;

			return new(old);
		}
		public static ConsoleBackgroundScope? WithBackground(IClassificationStyling styling)
		{
			if (styling.Background is not null)
				return WithBackground(styling.Background.Value.AsSpectre);

			return null;
		}
		#endregion
	}
}
