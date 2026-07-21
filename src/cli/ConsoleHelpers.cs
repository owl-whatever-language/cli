namespace OwlDomain.Owl.CLI;

public static class ConsoleHelpers
{
	extension(Console)
	{
		#region Functions
		public static void WriteError(string message)
		{
			// Note(Nightowl): Add a helper for this so that I can customise colors later or something;
			Console.Error.WriteLine(message);
		}
		#endregion
	}
}
