namespace OwlDomain.Owl.LSP.Handlers.Custom;

internal sealed class CustomTreeHandlerBundle<TRequest, TResponse>
	where TRequest : notnull
{
	#region Fields
	private readonly Func<TRequest, string> _sourceCallback;
	#endregion

	#region Properties
	public ILspContext Context { get; }
	public required ICustomTreeHandler<TRequest, TResponse, ICodeSyntaxTree>? CodeHandler { get; init; }
	public required ICustomTreeHandler<TRequest, TResponse, IConfigSyntaxTree>? ConfigHandler { get; init; }
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
				if (CodeHandler is not null)
					return await CodeHandler.HandleAsync(new(Context, workspace, request), code, cancellation);
			}
			else if (workspace.IsConfigGroup(path, out IConfigSyntaxTree? config))
			{
				if (ConfigHandler is not null)
					return await ConfigHandler.HandleAsync(new(Context, workspace, request), config, cancellation);
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
	public required ICustomTreeHandler<TRequest, TResponse, ICodeSyntaxTree, TResolve>? CodeHandler { get; init; }
	public required ICustomTreeHandler<TRequest, TResponse, IConfigSyntaxTree, TResolve>? ConfigHandler { get; init; }
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
				if (CodeHandler is not null)
					return await CodeHandler.HandleAsync(new(Context, workspace, request), code, cancellation);
			}
			else if (workspace.IsConfigGroup(path, out IConfigSyntaxTree? config))
			{
				if (ConfigHandler is not null)
					return await ConfigHandler.HandleAsync(new(Context, workspace, request), config, cancellation);
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
