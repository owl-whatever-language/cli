CommandApp app = new();

app.Configure(config =>
{
	if (GitInfo.Version is not null)
		config.SetApplicationVersion(GitInfo.Version);

	config
		.SetApplicationName("owl")
		.AddCommand<VersionCommand>("version").WithDescription("Provides more detailed version information than --version");

#if DEBUG
	config
		.PropagateExceptions()
		.ValidateExamples();
#endif
});

return await app.RunAsync(args);
