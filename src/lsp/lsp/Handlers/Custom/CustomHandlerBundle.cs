namespace OwlDomain.Owl.LSP.Handlers.Custom;

internal sealed class CustomHandlerBundle<TRequest, TResponse>
	where TRequest : notnull
{
	#region Fields
	private readonly Func<TRequest, string> _sourceCallback;
	#endregion

	#region Properties
	public ILspContext Context { get; }
	public ICustomHandler<TRequest, TResponse>? CodeHandler { get; set; }
	public ICustomHandler<TRequest, TResponse>? ConfigHandler { get; set; }
	#endregion

	#region Constructors
	public CustomHandlerBundle(ILspContext context, Func<TRequest, string> sourceCallback)
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
			if (workspace.IsCode(path))
			{
				if (CodeHandler is not null)
					return await CodeHandler.HandleAsync(new(Context, workspace, request), cancellation);
			}
			else if (workspace.IsConfigGroup(path))
			{
				if (ConfigHandler is not null)
					return await ConfigHandler.HandleAsync(new(Context, workspace, request), cancellation);
			}
		}

		return default;
	}
	#endregion
}

internal sealed class CustomHandlerBundle<TRequest, TResponse, TResolve>
	where TRequest : notnull
	where TResolve : notnull
{
	#region Fields
	private readonly Func<TRequest, string> _sourceCallback;
	#endregion

	#region Properties
	public ILspContext Context { get; }
	public ICustomHandler<TRequest, TResponse, TResolve>? CodeHandler { get; set; }
	public ICustomHandler<TRequest, TResponse, TResolve>? ConfigHandler { get; set; }
	#endregion

	#region Constructors
	public CustomHandlerBundle(ILspContext context, Func<TRequest, string> sourceCallback)
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
			if (workspace.IsCode(path))
			{
				if (CodeHandler is not null)
					return await CodeHandler.HandleAsync(new(Context, workspace, request), cancellation);
			}
			else if (workspace.IsConfigGroup(path))
			{
				if (ConfigHandler is not null)
					return await ConfigHandler.HandleAsync(new(Context, workspace, request), cancellation);
			}
		}

		return default;
	}

	public async Task<TResolve?> ResolveAsync(TResolve request, CancellationToken cancellation)
	{
		if (CodeHandler is not null)
		{
			TResolve? result = await CodeHandler.ResolveAsync(new(Context, request), cancellation);
			if (result is not null)
				return result;
		}

		if (ConfigHandler is not null)
		{
			TResolve? result = await ConfigHandler.ResolveAsync(new(Context, request), cancellation);
			if (result is not null)
				return result;
		}

		return default;
	}
	#endregion
}
