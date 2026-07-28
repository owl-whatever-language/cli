namespace OwlDomain.Owl.Workspaces;

using System.IO;
using Code = Code.CodeAnalysis;

public interface IOwlWorkspace
{
	#region Properties
	IReadOnlyCollection<ISourceFile> Files { get; }
	bool IsStale { get; }
	bool IsEmpty { get; }

	Code.IAnalysisContext CodeContext { get; }
	Code.AnalysisUpdateResult? LastCodeUpdate { get; }
	#endregion

	#region Methods
	bool ContainsFile(string path, [NotNullWhen(true)] out ISourceFile? file);
	bool ContainsFile(ISourceFile file);
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
	private readonly ReaderWriterLockSlim _lock = new();

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
					_lastCodeUpdate is null;
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
	public OwlWorkspace()
	{
		var builtinResult = Owl.Code.Execution.Builtins.BuiltinResolver.Resolve();
		_codeContext = new(builtinResult.ResultScope);
	}
	#endregion

	#region Methods
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
			_lastCodeUpdate = _codeContext.Update(_added, _removed, _changed);

			_added.Clear();
			_removed.Clear();
			_changed.Clear();
		}
	}
	#endregion
}

public static class IOwlWorkspaceExtensions
{
	extension(IOwlWorkspace workspace)
	{
		#region Methods
		public bool TryGetCodeBundle(ISourceFile source, [NotNullWhen(true)] out Code.Syntax.ISyntaxTreeBundle? bundle)
		{
			if (workspace.CodeContext.TryGet(source, out bundle) && bundle.LeastDetailed is not null)
				return true;

			bundle = default;
			return false;
		}
		public bool TryGetTree(ISourceFile source, [NotNullWhen(true)] out Code.Syntax.Concrete.IConcreteSyntaxTree? tree)
		{
			if (workspace.CodeContext.TryGet(source, out Code.Syntax.ISyntaxTreeBundle? bundle) && bundle.LeastDetailed is not null)
			{
				tree = bundle.LeastDetailed;
				return true;
			}

			tree = default;
			return false;
		}
		#endregion
	}
}
