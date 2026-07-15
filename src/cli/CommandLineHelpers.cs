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

		public void AddGroup(string[] args, Command group)
		{
			group.SetAction(parsing =>
			{
				command.Parse([.. args, "--help"]).Invoke();
			});

			command.Add(group);
		}
		#endregion
	}
}
