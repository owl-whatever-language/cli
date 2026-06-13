using System.Reflection;

namespace OwlDomain.Owl.CLI;

public static class GitInfo
{
	#region Properties
	public static bool IsAvailable => Branch.IsWhiteSpace() is false;
	public static string? Version { get; }
	public static string? Branch { get; }
	public static bool HasChanges { get; }
	public static string? Hash { get; }
	public static string? HashShort { get; }
	public static string? Subject { get; }
	public static string? Body { get; }
	public static DateTimeOffset? Date { get; }
	#endregion

	#region Constructors
	static GitInfo()
	{
		// Note(Nightowl): Not actually git info but easier to have it here;
		AssemblyInformationalVersionAttribute? version = Assembly
				.GetExecutingAssembly()
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

		Version = version?.InformationalVersion.Split('+').FirstOrDefault();

		Dictionary<string, string?> metadata = Assembly
			.GetExecutingAssembly()
			.GetCustomAttributes<AssemblyMetadataAttribute>()
			.ToDictionary(m => m.Key, m => m.Value);

		Branch = metadata.GetValueOrDefault("git.branch");
		HasChanges = metadata.GetValueOrDefault("git.uncommitted").IsWhiteSpace() is false;
		Hash = metadata.GetValueOrDefault("git.commit.hash");
		HashShort = metadata.GetValueOrDefault("git.commit.hash.short");
		Subject = metadata.GetValueOrDefault("git.commit.subject");
		Body = metadata.GetValueOrDefault("git.commit.body");

		string? rawDate = metadata.GetValueOrDefault("git.commit.date");

		if (rawDate.IsWhiteSpace() is false && DateTimeOffset.TryParse(rawDate, out DateTimeOffset date))
			Date = date;
	}
	#endregion
}
