CommandApp app = new();

app.Configure(config =>
{
	if (GitInfo.Version is not null)
		config.SetApplicationVersion(GitInfo.Version);

	config
		.SetApplicationName("owl")
		.AddCommandWithMeta<VersionCommand>()
		.AddCommandWithMeta<RunCommand>();

#if DEBUG
	config
		.PropagateExceptions()
		.ValidateExamples();
#endif
});

return await app.RunAsync(args);
