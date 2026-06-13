namespace OwlDomain.Owl.CLI.Commands;

public static class AppConfiguratorExtensions
{
	extension(IConfigurator configurator)
	{
		#region Methods
		public IConfigurator AddCommandWithMeta<TCommand>(Action<ICommandConfigurator>? config = null) where TCommand : class, ICommand
		{
			Type type = typeof(TCommand);

			string? name = type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
			string? description = type.GetCustomAttribute<DescriptionAttribute>()?.Description;

			if (name is null)
				ThrowHelper.ThrowInvalidOperationException($"The {type.Name} command was missing a name.");

			if (description is null)
				ThrowHelper.ThrowInvalidOperationException($"The {type.Name} command was missing a description.");

			ICommandConfigurator command = configurator.AddCommand<TCommand>(name).WithDescription(description);
			config?.Invoke(command);

			return configurator;
		}
		#endregion
	}
}
