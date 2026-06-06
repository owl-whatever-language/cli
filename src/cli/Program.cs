using System.Reflection;

CommandApp app = new();

app.Configure(config =>
{
	config
		.SetApplicationName("owl")
		.SetApplicationVersion(GetVersion());
});

return await app.RunAsync(args);

static string GetVersion()
{
	AssemblyInformationalVersionAttribute? attribute = Assembly
		.GetExecutingAssembly()
		.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

	if (attribute is null)
		return "dev";

	string version = attribute.InformationalVersion;
	int index = version.IndexOf('+');

	if (index < 0 || index + 1 == version.Length)
		return $"dev";

	const int shortLength = 7;
	string commitHash = version[(index + 1)..];
	string shortHash = commitHash.Length < shortLength ? commitHash : commitHash[..shortLength];

	return $"dev-{shortHash}";
}
