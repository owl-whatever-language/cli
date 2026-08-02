namespace OwlDomain.Owl.LSP.Handlers.Custom;

internal sealed class CustomTreeHandlerBundle<TRequest, TResponse>
	where TRequest : notnull
{
	#region Fields
	private readonly Func<TRequest, string> _sourceCallback;
	#endregion

	#region Properties
	public ILspContext Context { get; }
	public required ICustomTreeHandler<TRequest, TResponse, ICodeSyntaxTree>? CodeTreeHandler { get; set; }
	public required ICustomTreeHandler<TRequest, TResponse, IConfigSyntaxTree>? ConfigTreeHandler { get; set; }
	#endregion

	#region Constructors
	public CustomTreeHandlerBundle(ILspContext context, Func<TRequest, string> sourceCallback)
	{
		Context = context;
		_sourceCallback = sourceCallback;
	}
	#endregion

	#region Methods
	public async Task<TResponse?> HandleAsync(TRequest request, CancellationToken cancellation)
	{
		string path = _sourceCallback.Invoke(request);

		if (Context.TryGet(path, out IOwlWorkspace? workspace))
		{
			if (workspace.IsCode(path, out ICodeSyntaxTree? code))
			{
				if (CodeTreeHandler is not null)
					return await CodeTreeHandler.HandleAsync(new(Context, workspace, request), code, cancellation);
			}
			else if (workspace.IsConfigGroup(path, out IConfigSyntaxTree? config))
			{
				if (ConfigTreeHandler is not null)
					return await ConfigTreeHandler.HandleAsync(new(Context, workspace, request), config, cancellation);
			}
		}

		return default;
	}
	#endregion
}

internal sealed class CustomTreeHandlerBundle<TRequest, TResponse, TResolve>
	where TRequest : notnull
	where TResolve : notnull
{
	#region Fields
	private readonly Func<TRequest, string> _sourceCallback;
	#endregion

	#region Properties
	public ILspContext Context { get; }
	public ICustomTreeHandler<TRequest, TResponse, ICodeSyntaxTree, TResolve>? CodeTreeHandler { get; set; }
	public ICustomTreeHandler<TRequest, TResponse, IConfigSyntaxTree, TResolve>? ConfigTreeHandler { get; set; }
	#endregion

	#region Constructors
	public CustomTreeHandlerBundle(ILspContext context, Func<TRequest, string> sourceCallback)
	{
		Context = context;
		_sourceCallback = sourceCallback;
	}
	#endregion

	#region Methods
	public async Task<TResponse?> HandleAsync(TRequest request, CancellationToken cancellation)
	{
		string path = _sourceCallback.Invoke(request);

		if (Context.TryGet(path, out IOwlWorkspace? workspace))
		{
			if (workspace.IsCode(path, out ICodeSyntaxTree? code))
			{
				if (CodeTreeHandler is not null)
					return await CodeTreeHandler.HandleAsync(new(Context, workspace, request), code, cancellation);
			}
			else if (workspace.IsConfigGroup(path, out IConfigSyntaxTree? config))
			{
				if (ConfigTreeHandler is not null)
					return await ConfigTreeHandler.HandleAsync(new(Context, workspace, request), config, cancellation);
			}
		}

		return default;
	}

	public async Task<TResolve?> ResolveAsync(TResolve request, CancellationToken cancellation)
	{
		if (CodeTreeHandler is not null)
		{
			TResolve? result = await CodeTreeHandler.ResolveAsync(new(Context, request), cancellation);
			if (result is not null)
				return result;
		}

		if (ConfigTreeHandler is not null)
		{
			TResolve? result = await ConfigTreeHandler.ResolveAsync(new(Context, request), cancellation);
			if (result is not null)
				return result;
		}

		return default;
	}
	#endregion
}
