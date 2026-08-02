using System.IO;

namespace OwlDomain.Owl.LSP;

internal interface ILspContext
{
	#region Properties
	LanguageServer Server { get; }
	IReadOnlyCollection<IOwlWorkspace> Workspaces { get; }
	#endregion

	#region Methods
	void AddFile(string path, string text);
	void UpdateFile(string path, string text);
	void RemoveFile(string path);
	bool TryGet(string path, [NotNullWhen(true)] out IOwlWorkspace? workspace);
	#endregion
}

internal sealed class LspContext : ILspContext
{
	#region Fields
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
		}
	}
	public void UpdateFile(string path, string text)
	{
		using (_lock.WriteLock())
		{
			IOwlWorkspace workspace = GetOrCreate(path);

			if (workspace.IsRelevantFile(path, out ISourceFile? original))
			{
				if (original is WorkspaceSourceFile source)
				{
					source.Text = text;
					workspace.UpdateFile(source);
				}
				else
				{
					source = new(path, text);

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
		}
	}
	public void RemoveFile(string path)
	{
		using (_lock.WriteLock())
		{
			if (TryGet(path, out IOwlWorkspace? workspace))
			{
				if (workspace.IsRelevantFile(path, out ISourceFile? source))
					workspace.RemoveFile(source);

				if (workspace.IsEmpty)
					_workspaces.Remove(workspace);
			}
		}
	}
	#endregion

	#region Helpers
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
			Console.Error.WriteLine($"Created new workspace: {directory ?? filePath}");

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
				lastPackage = packagePath;

			string workspacePath = Path.Combine(directory, "owl.workspace");
			if (File.Exists(workspacePath))
				return new OwlWorkspace(directory);

			directory = Path.GetDirectoryName(directory);
		}

		if (lastPackage is not null)
			return new OwlWorkspace(Path.GetDirectoryName(lastPackage));

		return new OwlWorkspace(null);
	}
	#endregion
}
