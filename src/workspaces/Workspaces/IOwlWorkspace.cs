namespace OwlDomain.Owl.Workspaces;

using System.IO;
using Code = Code.CodeAnalysis;
using Config = Config.CodeAnalysis;
using ConfigUnit = Config.CodeAnalysis.Syntax.Concrete.DocumentUnits;

public interface IOwlWorkspace
{
	#region Properties
	IReadOnlyCollection<ISourceFile> Files { get; }
	bool IsStale { get; }
	bool IsEmpty { get; }

	Config.IAnalysisContext WorkspaceContext { get; }
	Config.AnalysisUpdateResult? LastWorkspaceUpdate { get; }

	Config.IAnalysisContext ConfigContext { get; }
	Config.AnalysisUpdateResult? LastConfigUpdate { get; }

	Config.IAnalysisContext PackageContext { get; }
	Config.AnalysisUpdateResult? LastPackageUpdate { get; }

	Code.IAnalysisContext CodeContext { get; }
	Code.AnalysisUpdateResult? LastCodeUpdate { get; }
	#endregion

	#region Methods
	bool IsRelevant(string path, out ISourceFile? source);
	bool IsRelevantFile(string path, [NotNullWhen(true)] out ISourceFile? source);
	void AddFile(ISourceFile file);
	void UpdateFile(ISourceFile file);
	void RemoveFile(ISourceFile file);

	[MemberNotNull(nameof(LastCodeUpdate))]
	void Analyse();
	#endregion
}

public sealed class OwlWorkspace : IOwlWorkspace
{
	#region Fields
	private readonly string? _directory;
	private readonly ReaderWriterLockSlim _lock = new();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Config.AnalysisContext _workspaceContext, _configContext, _packageContext;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Config.AnalysisUpdateResult? _lastWorkspaceUpdate, _lastConfigUpdate, _lastPackageUpdate;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Code.AnalysisContext _codeContext;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Code.AnalysisUpdateResult? _lastCodeUpdate;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly HashSet<ISourceFile> _files = [];
	private readonly HashSet<ISourceFile> _added = [];
	private readonly HashSet<ISourceFile> _changed = [];
	private readonly HashSet<ISourceFile> _removed = [];
	#endregion

	#region Properties
	public IReadOnlyCollection<ISourceFile> Files
	{
		get
		{
			using (_lock.ReadLock())
				return _files.ToArray();
		}
	}

	public Config.IAnalysisContext WorkspaceContext => _workspaceContext;
	public Config.AnalysisUpdateResult? LastWorkspaceUpdate
	{
		get
		{
			using (_lock.ReadLock())
				return _lastWorkspaceUpdate;
		}

		private set
		{
			using (_lock.WriteLock())
				_lastWorkspaceUpdate = value;
		}
	}

	public Config.IAnalysisContext ConfigContext => _configContext;
	public Config.AnalysisUpdateResult? LastConfigUpdate
	{
		get
		{
			using (_lock.ReadLock())
				return _lastConfigUpdate;
		}

		private set
		{
			using (_lock.WriteLock())
				_lastConfigUpdate = value;
		}
	}

	public Config.IAnalysisContext PackageContext => _packageContext;
	public Config.AnalysisUpdateResult? LastPackageUpdate
	{
		get
		{
			using (_lock.ReadLock())
				return _lastPackageUpdate;
		}

		private set
		{
			using (_lock.WriteLock())
				_lastPackageUpdate = value;
		}
	}

	public Code.IAnalysisContext CodeContext => _codeContext;
	public Code.AnalysisUpdateResult? LastCodeUpdate
	{
		get
		{
			using (_lock.ReadLock())
				return _lastCodeUpdate;
		}

		private set
		{
			using (_lock.WriteLock())
				_lastCodeUpdate = value;
		}
	}

	public bool IsStale
	{
		get
		{
			using (_lock.ReadLock())
			{
				return
					_added.Any() ||
					_changed.Any() ||
					_removed.Any() ||
					_lastWorkspaceUpdate is null ||
					_lastConfigUpdate is null ||
					_lastPackageUpdate is null ||
					_lastCodeUpdate is null
				;
			}
		}
	}
	public bool IsEmpty
	{
		get
		{
			using (_lock.ReadLock())
				return _files.Count is 0;
		}
	}
	#endregion

	#region Constructors
	public OwlWorkspace(string? directory)
	{
		_directory = directory;

		_workspaceContext = new();
		_configContext = new();
		_packageContext = new();

		var builtinResult = Owl.Code.Execution.Builtins.BuiltinResolver.Resolve();
		_codeContext = new(builtinResult.ResultScope);
	}
	#endregion

	#region Methods
	public bool IsRelevant(string path, out ISourceFile? source)
	{
		if (IsRelevantFile(path, out source))
			return true;

		if (_directory is null)
			return false;

		string relative = Path.GetRelativePath(_directory, path);
		if (relative == path)
			return false;

		if (relative.StartsWith(".." + Path.DirectorySeparatorChar))
			return false;

		return true;
	}
	public bool IsRelevantFile(string path, [NotNullWhen(true)] out ISourceFile? source)
	{
		FileInfo target = new(path);
		using (_lock.ReadLock())
		{
			source = _files
				.Where(f => f.Path is not null)
				.FirstOrDefault(f => new FileInfo(f.Path!).FullName == target.FullName); // Note(Nightowl): This should be checking the canonical path... I think, not fully sure;

			return source is not null;
		}
	}
	public bool ContainsFile(string path, [NotNullWhen(true)] out ISourceFile? file)
	{
		FileInfo target = new(path);
		using (_lock.ReadLock())
		{
			file = _files
				.Where(f => f.Path is not null)
				// Note(Nightowl): This should be checking the canonical path... I think, not fully sure;
				.FirstOrDefault(f => new FileInfo(f.Path!).FullName == target.FullName);

			return file is not null;
		}
	}
	public bool ContainsFile(ISourceFile file)
	{
		using (_lock.ReadLock())
			return _files.Contains(file);
	}
	public void AddFile(ISourceFile file)
	{
		using (_lock.WriteLock())
		{
			_files.Add(file);

			if (_removed.Contains(file))
			{
				_removed.Remove(file);
				_added.Add(file);
			}
			else if (_changed.Contains(file))
			{
				// Note(Nightowl): Do nothing, keep the file as changed;
			}
			else
				_added.Add(file);
		}
	}
	public void UpdateFile(ISourceFile file)
	{
		using (_lock.WriteLock())
		{
			if (_files.Contains(file) is false)
				ThrowHelper.ThrowArgumentException(nameof(file), $"The given file ({file.Path}) hasn't been added yet, and so it cannot be marked as being updated.");

			if (_added.Contains(file))
			{
				// Note(Nightowl): Do nothing, keep the file as added;
			}
			else if (_removed.Contains(file))
			{
				_removed.Remove(file);
				_added.Add(file);
			}
			else
				_changed.Add(file);
		}
	}
	public void RemoveFile(ISourceFile file)
	{
		using (_lock.WriteLock())
		{
			_files.Remove(file);

			if (_added.Contains(file))
				_added.Remove(file);
			else if (_changed.Contains(file))
			{
				_changed.Remove(file);
				_removed.Add(file);
			}
			else
				_removed.Add(file);
		}
	}

	public void Analyse()
	{
		using (_lock.WriteLock())
		{
			bool workspaceChanged = AnalyseWorkspace(false);
			bool configChanged = AnalyseConfigs(workspaceChanged);
			bool packageChanged = AnalysePackages(configChanged);
			AnalyseCode(packageChanged);

			_added.Clear();
			_removed.Clear();
			_changed.Clear();
		}
	}
	private bool AnalyseWorkspace(bool fullUpdate)
	{
		Config.AnalysisUpdate update = new()
		{
			Added = _added.OnlyWorkspace().ToArray(),
			Removed = _removed.OnlyWorkspace().ToArray(),
			Changed = (fullUpdate ? _changed.Concat(_workspaceContext.Bundles.Select(b => b.Source)) : _changed).OnlyWorkspace().ToArray()
		};

		if (_lastWorkspaceUpdate is not null && update.IsEmpty)
			return false;

		_lastWorkspaceUpdate = _workspaceContext.Update(update);
		return true;
	}
	private bool AnalyseConfigs(bool fullUpdate)
	{
		Config.AnalysisUpdate update = new()
		{
			Added = _added.OnlyConfig().ToArray(),
			Removed = _removed.OnlyConfig().ToArray(),
			Changed = (fullUpdate ? _changed.Concat(_configContext.Bundles.Select(b => b.Source)) : _changed).OnlyConfig().ToArray()
		};

		if (_lastConfigUpdate is not null && update.IsEmpty)
			return false;

		_lastConfigUpdate = _configContext.Update(update);
		return true;
	}
	private bool AnalysePackages(bool fullUpdate)
	{
		Config.AnalysisUpdate update = new()
		{
			Added = _added.OnlyPackage().ToArray(),
			Removed = _removed.OnlyPackage().ToArray(),
			Changed = (fullUpdate ? _changed.Concat(_packageContext.Bundles.Select(b => b.Source)) : _changed).OnlyPackage().ToArray()
		};

		if (_lastPackageUpdate is not null && update.IsEmpty)
			return false;

		_lastPackageUpdate = _packageContext.Update(update);
		return true;
	}
	private bool AnalyseCode(bool fullUpdate)
	{
		Code.AnalysisUpdate update = new()
		{
			Added = _added.OnlyCode().ToArray(),
			Removed = _removed.OnlyCode().ToArray(),
			Changed = (fullUpdate ? _changed.Concat(_codeContext.Bundles.Select(b => b.Source)) : _changed).OnlyCode().ToArray()
		};

		if (_lastCodeUpdate is not null && update.IsEmpty)
			return false;

		_lastCodeUpdate = _codeContext.Update(update);
		return true;
	}
	#endregion
}

public static class IOwlWorkspaceExtensions
{
	extension(IOwlWorkspace workspace)
	{
		#region Methods
		public bool IsConfigGroup(string path, [NotNullWhen(true)] out Config.Syntax.ISyntaxTreeBundle? bundle)
		{
			if (IsWorkspace(workspace, path, out bundle))
				return true;

			if (IsConfig(workspace, path, out bundle))
				return true;

			if (IsPackage(workspace, path, out bundle))
				return true;

			return false;
		}

		public bool IsConfigGroup(string path, [NotNullWhen(true)] out Config.Syntax.Concrete.IConcreteSyntaxTree? tree)
		{
			if (IsConfigGroup(workspace, path, out Config.Syntax.ISyntaxTreeBundle? bundle))
			{
				tree = bundle.LeastDetailed;
				return tree is not null;
			}

			tree = default;
			return false;
		}

		public bool IsWorkspace(string path, [NotNullWhen(true)] out Config.Syntax.ISyntaxTreeBundle? bundle)
		{
			if (workspace.IsRelevantFile(path, out ISourceFile? source))
			{
				if (Config.IAnalysisContextExtensions.TryGet(workspace.WorkspaceContext, source, out bundle) && bundle.LeastDetailed is not null)
					return true;
			}

			bundle = default;
			return false;
		}
		public bool IsWorkspace(string path, [NotNullWhen(true)] out ConfigUnit.IConcreteWorkspaceDocumentUnitSyntax? document)
		{
			if (IsWorkspace(workspace, path, out Config.Syntax.ISyntaxTreeBundle? bundle))
			{
				Debug.Assert(bundle.LeastDetailed is not null);
				document = (ConfigUnit.IConcreteWorkspaceDocumentUnitSyntax)bundle.LeastDetailed.Document;

				return true;
			}

			document = default;
			return false;
		}
		public bool IsWorkspace(string path) => IsWorkspace(workspace, path, out Config.Syntax.ISyntaxTreeBundle? _);

		public bool IsConfig(string path, [NotNullWhen(true)] out Config.Syntax.ISyntaxTreeBundle? bundle)
		{
			if (workspace.IsRelevantFile(path, out ISourceFile? source))
			{
				if (Config.IAnalysisContextExtensions.TryGet(workspace.ConfigContext, source, out bundle) && bundle.LeastDetailed is not null)
					return true;
			}

			bundle = default;
			return false;
		}
		public bool IsConfig(string path, [NotNullWhen(true)] out ConfigUnit.IConcreteConfigDocumentUnitSyntax? document)
		{
			if (IsConfig(workspace, path, out Config.Syntax.ISyntaxTreeBundle? bundle))
			{
				Debug.Assert(bundle.LeastDetailed is not null);
				document = (ConfigUnit.IConcreteConfigDocumentUnitSyntax)bundle.LeastDetailed.Document;

				return true;
			}

			document = default;
			return false;
		}
		public bool IsConfig(string path) => IsConfig(workspace, path, out Config.Syntax.ISyntaxTreeBundle? _);

		public bool IsPackage(string path, [NotNullWhen(true)] out Config.Syntax.ISyntaxTreeBundle? bundle)
		{
			if (workspace.IsRelevantFile(path, out ISourceFile? source))
			{
				if (Config.IAnalysisContextExtensions.TryGet(workspace.PackageContext, source, out bundle) && bundle.LeastDetailed is not null)
					return true;
			}

			bundle = default;
			return false;
		}
		public bool IsPackage(string path, [NotNullWhen(true)] out ConfigUnit.IConcretePackageDocumentUnitSyntax? document)
		{
			if (IsPackage(workspace, path, out Config.Syntax.ISyntaxTreeBundle? bundle))
			{
				Debug.Assert(bundle.LeastDetailed is not null);
				document = (ConfigUnit.IConcretePackageDocumentUnitSyntax)bundle.LeastDetailed.Document;

				return true;
			}

			document = default;
			return false;
		}
		public bool IsPackage(string path) => IsPackage(workspace, path, out Config.Syntax.ISyntaxTreeBundle? _);

		public bool IsCode(string path, [NotNullWhen(true)] out Code.Syntax.ISyntaxTreeBundle? bundle)
		{
			if (workspace.IsRelevantFile(path, out ISourceFile? source))
			{
				if (Code.IAnalysisContextExtensions.TryGet(workspace.CodeContext, source, out bundle) && bundle.LeastDetailed is not null)
					return true;
			}

			bundle = default;
			return false;
		}
		public bool IsCode(string path, [NotNullWhen(true)] out Code.Syntax.Concrete.IConcreteSyntaxTree? tree)
		{
			if (IsCode(workspace, path, out Code.Syntax.ISyntaxTreeBundle? bundle))
			{
				Debug.Assert(bundle.LeastDetailed is not null);
				tree = bundle.LeastDetailed;

				return true;
			}

			tree = default;
			return false;
		}
		public bool IsCode(string path) => IsCode(workspace, path, out Code.Syntax.ISyntaxTreeBundle? _);
		#endregion
	}
}
