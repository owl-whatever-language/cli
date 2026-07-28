namespace OwlDomain.Owl.LSP;

internal interface ILspContext
{
	#region Properties
	LanguageServer Server { get; }
	IReadOnlyCollection<IOwlWorkspace> Workspaces { get; }
	#endregion

	#region Methods
	IOwlWorkspace NewWorkspace();
	bool TryGetWorkspace(string filePath, [NotNullWhen(true)] out ISourceFile? file, [NotNullWhen(true)] out IOwlWorkspace? workspace);
	bool TryGetWorkspace(ISourceFile file, [NotNullWhen(true)] out IOwlWorkspace? workspace);
	void RemoveWorkspace(IOwlWorkspace workspace);
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
	public IOwlWorkspace NewWorkspace()
	{
		using (_lock.WriteLock())
		{
			OwlWorkspace workspace = new();
			_workspaces.Add(workspace);

			return workspace;
		}
	}
	public bool TryGetWorkspace(string filePath, [NotNullWhen(true)] out ISourceFile? file, [NotNullWhen(true)] out IOwlWorkspace? workspace)
	{
		using (_lock.ReadLock())
		{
			foreach (IOwlWorkspace current in _workspaces)
			{
				if (current.ContainsFile(filePath, out file))
				{
					workspace = current;
					return true;
				}
			}

			file = default;
			workspace = default;

			return false;
		}
	}
	public bool TryGetWorkspace(ISourceFile file, [NotNullWhen(true)] out IOwlWorkspace? workspace)
	{
		using (_lock.ReadLock())
		{
			foreach (IOwlWorkspace current in _workspaces)
			{
				if (current.ContainsFile(file))
				{
					workspace = current;
					return true;
				}
			}

			workspace = default;
			return false;
		}
	}
	public void RemoveWorkspace(IOwlWorkspace workspace)
	{
		using (_lock.WriteLock())
		{
			_workspaces.Remove(workspace);
		}
	}
	#endregion
}

internal static class ILspContextExtensions
{
	extension(ILspContext context)
	{
		#region TryGetWorkspace methods
		public bool TryGetWorkspace(TextDocumentItem document, [NotNullWhen(true)] out ISourceFile? file, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetWorkspace(context, document.Uri, out file, out workspace);
		}
		public bool TryGetWorkspace(TextDocumentIdentifier documentId, [NotNullWhen(true)] out ISourceFile? file, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetWorkspace(context, documentId.Uri, out file, out workspace);
		}
		public bool TryGetWorkspace(DocumentUri uri, [NotNullWhen(true)] out ISourceFile? file, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetWorkspace(context, uri.Uri, out file, out workspace);
		}
		public bool TryGetWorkspace(Uri uri, [NotNullWhen(true)] out ISourceFile? file, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return context.TryGetWorkspace(uri.AbsolutePath, out file, out workspace);
		}

		public bool TryGetWorkspace(TextDocumentItem document, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetWorkspace(context, document.Uri, out _, out workspace);
		}
		public bool TryGetWorkspace(TextDocumentIdentifier documentId, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetWorkspace(context, documentId.Uri, out _, out workspace);
		}
		public bool TryGetWorkspace(DocumentUri uri, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetWorkspace(context, uri.Uri, out _, out workspace);
		}
		public bool TryGetWorkspace(Uri uri, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return context.TryGetWorkspace(uri.AbsolutePath, out _, out workspace);
		}
		#endregion

		#region TryGetTree methods
		public bool TryGetTree(TextDocumentItem document, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out ISourceFile? file, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetTree(context, document.Uri.Uri.AbsolutePath, out tree, out file, out workspace);
		}
		public bool TryGetTree(TextDocumentIdentifier documentId, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out ISourceFile? file, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetTree(context, documentId.Uri.Uri.AbsolutePath, out tree, out file, out workspace);
		}
		public bool TryGetTree(DocumentUri uri, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out ISourceFile? file, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetTree(context, uri.Uri.AbsolutePath, out tree, out file, out workspace);
		}
		public bool TryGetTree(Uri uri, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out ISourceFile? file, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetTree(context, uri.AbsolutePath, out tree, out file, out workspace);
		}
		public bool TryGetTree(string path, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out ISourceFile? file, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			if (context.TryGetWorkspace(path, out file, out workspace))
			{
				if (workspace.TryGetTree(file, out tree))
					return true;
			}

			tree = default;
			file = default;
			workspace = default;

			return false;
		}

		public bool TryGetTree(TextDocumentItem document, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetTree(context, document.Uri.Uri.AbsolutePath, out tree, out _, out workspace);
		}
		public bool TryGetTree(TextDocumentIdentifier documentId, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetTree(context, documentId.Uri.Uri.AbsolutePath, out tree, out _, out workspace);
		}
		public bool TryGetTree(DocumentUri uri, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetTree(context, uri.Uri.AbsolutePath, out tree, out _, out workspace);
		}
		public bool TryGetTree(Uri uri, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetTree(context, uri.AbsolutePath, out tree, out _, out workspace);
		}
		public bool TryGetTree(string path, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out IOwlWorkspace? workspace)
		{
			return TryGetTree(context, path, out tree, out _, out workspace);
		}

		public bool TryGetTree(TextDocumentItem document, [NotNullWhen(true)] out ICodeSyntaxTree? tree)
		{
			return TryGetTree(context, document.Uri.Uri.AbsolutePath, out tree, out _, out _);
		}
		public bool TryGetTree(TextDocumentIdentifier documentId, [NotNullWhen(true)] out ICodeSyntaxTree? tree)
		{
			return TryGetTree(context, documentId.Uri.Uri.AbsolutePath, out tree, out _, out _);
		}
		public bool TryGetTree(DocumentUri uri, [NotNullWhen(true)] out ICodeSyntaxTree? tree)
		{
			return TryGetTree(context, uri.Uri.AbsolutePath, out tree, out _, out _);
		}
		public bool TryGetTree(Uri uri, [NotNullWhen(true)] out ICodeSyntaxTree? tree)
		{
			return TryGetTree(context, uri.AbsolutePath, out tree, out _, out _);
		}
		public bool TryGetTree(string path, [NotNullWhen(true)] out ICodeSyntaxTree? tree)
		{
			return TryGetTree(context, path, out tree, out _, out _);
		}

		public bool TryGetTree(TextDocumentItem document, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out ISourceFile? file)
		{
			return TryGetTree(context, document.Uri.Uri.AbsolutePath, out tree, out file, out _);
		}
		public bool TryGetTree(TextDocumentIdentifier documentId, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out ISourceFile? file)
		{
			return TryGetTree(context, documentId.Uri.Uri.AbsolutePath, out tree, out file, out _);
		}
		public bool TryGetTree(DocumentUri uri, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out ISourceFile? file)
		{
			return TryGetTree(context, uri.Uri.AbsolutePath, out tree, out file, out _);
		}
		public bool TryGetTree(Uri uri, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out ISourceFile? file)
		{
			return TryGetTree(context, uri.AbsolutePath, out tree, out file, out _);
		}
		public bool TryGetTree(string path, [NotNullWhen(true)] out ICodeSyntaxTree? tree, [NotNullWhen(true)] out ISourceFile? file)
		{
			if (context.TryGetWorkspace(path, out file, out IOwlWorkspace? workspace))
			{
				if (workspace.TryGetTree(file, out tree))
					return true;
			}

			tree = default;
			file = default;

			return false;
		}
		#endregion
	}
}
