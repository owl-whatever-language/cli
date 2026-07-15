namespace OwlDomain.Owl.CLI;

public static class CommandLineHelpers
{
	extension(Command command)
	{
		#region Methods
		public void CustomiseOption<T>(Action<T> callback)
			where T : Option
		{
			foreach (T option in command.Options.OfType<T>())
				callback.Invoke(option);
		}
		#endregion
	}
}
