using System.IO;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.Registration;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceWatchedFile;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceWatchedFile.Watch;

namespace OwlDomain.Owl.LSP;

internal interface ILspContext
{
	#region Properties
	LanguageServer Server { get; }
	IReadOnlyCollection<IOwlWorkspace> Workspaces { get; }
	#endregion

	#region Methods
	void AddFile(string path, string text);
	void UpdateFile(string path, string text, int? version);
	void RemoveFile(string path);
	bool TryGet(string path, [NotNullWhen(true)] out IOwlWorkspace? workspace);
	void WatchWorkspace(string path);
	void StopWatchingWorkspace(string path);
	#endregion
}

internal sealed class LspContext : ILspContext
{
	#region Fields
	private readonly Dictionary<string, Registration> _workspaceWatchRegistrations = [];

	private readonly ReaderWriterLockSlim _lock = new();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IOwlWorkspace> _workspaces = [];
	#endregion

	#region Properties
	public LanguageServer Server { get; }
	public IReadOnlyCollection<IOwlWorkspace> Workspaces => _workspaces;
	#endregion

	#region Constructors
	public LspContext(LanguageServer server)
	{
		Server = server;
	}
	#endregion

	#region Methods
	public void AddFile(string path, string text)
	{
		using (_lock.WriteLock())
		{
			IOwlWorkspace workspace = GetOrCreate(path);

			if (workspace.IsRelevantFile(path, out ISourceFile? original))
				workspace.RemoveFile(original);

			WorkspaceSourceFile source = new(path, text);
			workspace.AddFile(source);

			workspace.Analyse();
			PrintAnalysisInfo(workspace);
		}
	}
	public void UpdateFile(string path, string text, int? version)
	{
		using (_lock.WriteLock())
		{
			IOwlWorkspace workspace = GetOrCreate(path);

			if (workspace.IsRelevantFile(path, out ISourceFile? original))
			{
				if (original is WorkspaceSourceFile source)
				{
					if (ShouldUpdate(source.Version, version) is false)
						return;

					source.Text = text;
					source.Version = version;
					workspace.UpdateFile(source);
				}
				else
				{
					source = new(path, text) { Version = version };

					workspace.RemoveFile(original);
					workspace.AddFile(source);
				}
			}
			else
			{
				WorkspaceSourceFile source = new(path, text);
				workspace.AddFile(source);
			}

			workspace.Analyse();
			PrintAnalysisInfo(workspace);
		}
	}
	public void RemoveFile(string path)
	{
		using (_lock.WriteLock())
		{
			if (TryGet(path, out IOwlWorkspace? workspace))
			{
				if (workspace.IsRelevantFile(path, out ISourceFile? source))
				{
					workspace.RemoveFile(source);

					if (workspace.Directory is not null)
						workspace.AddFile(new FileSystemSourceFile(path));
				}

				if (workspace.IsEmpty || (workspace.Files.OfType<WorkspaceSourceFile>().Any() is false))
				{
					_workspaces.Remove(workspace);
					Console.Error.WriteLine($"Removed workspace #{workspace.Id:n0}: {workspace.Directory ?? path}.");
				}
				else
				{
					workspace.Analyse();
					PrintAnalysisInfo(workspace);
				}
			}
		}
	}
	public void WatchWorkspace(string path)
	{
		lock (_workspaceWatchRegistrations)
		{
			if (_workspaceWatchRegistrations.ContainsKey(path))
				return;

			DidChangeWatchedFilesRegistrationOptions watchers = new()
			{
				Watchers =
				[
					new()
					{
						GlobalPattern = "**/owl.{workspace,package,config}",
						Kind = WatchKind.Create | WatchKind.Change | WatchKind.Delete
					},
					new()
					{
						GlobalPattern = "**/*.owl",
						Kind = WatchKind.Create | WatchKind.Change | WatchKind.Delete
					},
				]
			};

			Registration registration = new()
			{
				Method = "workspace/didChangeWatchedFiles",
				Id = Guid.NewGuid().ToString(),
				RegisterOptions = watchers
			};

			Server.Client.DynamicRegisterCapability(new()
			{
				Registrations = [registration]
			});

			Console.Error.WriteLine($"Watching workspace: {path}");
			_workspaceWatchRegistrations.Add(path, registration);
		}
	}
	public void StopWatchingWorkspace(string path)
	{
		lock (_workspaceWatchRegistrations)
		{
			if (_workspaceWatchRegistrations.Remove(path, out Registration? registration) is false)
				return;

			Server.Client.DynamicUnregisterCapability(new()
			{
				Unregisterations =
				[
					new()
					{
						Method = "workspace/didChangeWatchedFiles",
						Id = registration.Id
					}
				]
			});

			Console.Error.WriteLine($"No longer watching workspace: {path}");
		}
	}
	#endregion

	#region Helpers
	private bool ShouldUpdate(int? currentVersion, int? newVersion)
	{
		if (currentVersion is null || newVersion is null)
			return true;

		return currentVersion < newVersion;
	}
	private void PrintAnalysisInfo(IOwlWorkspace workspace)
	{
		int code = workspace.CodeContext.Bundles.Count;
		int workspaces = workspace.WorkspaceContext.Bundles.Count;
		int configs = workspace.ConfigContext.Bundles.Count;
		int packages = workspace.PackageContext.Bundles.Count;

		Console.Error.WriteLine($"Analysed workspace #{workspace.Id}: {code:n0} source(s) {workspaces:n0} workspace(s) {configs:n0} config(s) {packages:n0} package(s)");
	}
	public bool TryGet(string filePath, [NotNullWhen(true)] out IOwlWorkspace? workspace)
	{
		foreach (IOwlWorkspace current in _workspaces)
		{
			if (current.IsRelevant(filePath, out _))
			{
				workspace = current;
				return true;
			}
		}

		workspace = default;
		return false;
	}
	private IOwlWorkspace GetOrCreate(string filePath)
	{
		if (TryGet(filePath, out IOwlWorkspace? workspace) is false)
		{
			workspace = CreateWorkspace(filePath, out string? directory);
			Console.Error.WriteLine($"Created new workspace #{workspace.Id:n0}: {directory ?? filePath}");

			_workspaces.Add(workspace);
		}

		return workspace;
	}
	private IOwlWorkspace CreateWorkspace(string filePath, out string? directory)
	{
		string? lastPackage = null;

		directory = Path.GetDirectoryName(filePath);
		while (directory is not null)
		{
			string packagePath = Path.Combine(directory, "owl.package");
			if (File.Exists(packagePath))
				lastPackage = directory;

			string workspacePath = Path.Combine(directory, "owl.workspace");
			if (File.Exists(workspacePath))
				return CreateFromDirectory(directory);

			directory = Path.GetDirectoryName(directory);
		}

		if (lastPackage is not null)
			return CreateFromDirectory(lastPackage);

		return new OwlWorkspace(null);
	}
	private IOwlWorkspace CreateFromDirectory(string directory)
	{
		IReadOnlyCollection<string> GetFiles(params ReadOnlySpan<string> filters)
		{
			List<string> files = [];

			foreach (string filter in filters)
			{
				string[] current = Directory.GetFiles(directory, filter, SearchOption.AllDirectories);
				files.AddRange(current);
			}

			return files;
		}

		OwlWorkspace workspace = new(directory);
		WatchWorkspace(directory);

		IReadOnlyCollection<string> files = GetFiles("*.owl", "owl.workspace", "owl.config", "owl.package");

		foreach (string file in files)
		{
			FileSystemSourceFile source = new(file);
			workspace.AddFile(source);
		}

		HashSet<IOwlWorkspace> inlined = [];

		foreach (IOwlWorkspace current in _workspaces)
		{
			if (current.Directory is null)
			{
				string? path = current.Files.FirstOrDefault()?.Path;
				if (path is not null && workspace.IsRelevant(path, out _))
					inlined.Add(current);

				continue;
			}

			if (workspace.IsRelevant(current.Directory, out _))
				inlined.Add(current);
		}

		foreach (IOwlWorkspace current in inlined)
		{
			_workspaces.Remove(current);

			if (current.Directory is not null)
				StopWatchingWorkspace(current.Directory);

			foreach (WorkspaceSourceFile source in current.Files.OfType<WorkspaceSourceFile>())
			{
				if (workspace.IsRelevantFile(source.Path, out ISourceFile? manual))
					workspace.RemoveFile(manual);

				workspace.AddFile(source);
			}

			Console.Error.WriteLine($"Inlined workspace #{current.Id:n0} into workspace #{workspace.Id:n0}");
		}

		return workspace;
	}
	#endregion
}
