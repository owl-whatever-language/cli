namespace OwlDomain.Owl.LSP.Handlers.Custom;

internal readonly struct HandlerRequest<TRequest>
	where TRequest : notnull
{
	#region Properties
	public ILspContext Context { get; }
	public IOwlWorkspace Workspace { get; }
	public TRequest Request { get; }
	#endregion

	#region Constructors
	public HandlerRequest(ILspContext context, IOwlWorkspace workspace, TRequest request)
	{
		Context = context;
		Workspace = workspace;
		Request = request;
	}
	#endregion
}

internal readonly struct ResolveRequest<TResolve>
	where TResolve : notnull
{
	#region Properties
	public ILspContext Context { get; }
	public TResolve Request { get; }
	#endregion

	#region Constructors
	public ResolveRequest(ILspContext context, TResolve request)
	{
		Context = context;
		Request = request;
	}
	#endregion
}

internal interface ICustomHandler<TRequest, TResponse>
	where TRequest : notnull
{
	#region Methods
	Task<TResponse?> HandleAsync(HandlerRequest<TRequest> request, CancellationToken cancellation);
	#endregion
}

internal interface ICustomHandler<TRequest, TResponse, TResolve> : ICustomHandler<TRequest, TResponse>
	where TRequest : notnull
	where TResolve : notnull
{
	#region Methods
	Task<TResolve?> ResolveAsync(ResolveRequest<TResolve> request, CancellationToken cancellation);
	#endregion
}

internal abstract class BaseCustomHandler<TRequest, TResponse> : ICustomHandler<TRequest, TResponse>
	where TRequest : notnull
{
	#region Methods
	public virtual Task<TResponse?> HandleAsync(HandlerRequest<TRequest> request, CancellationToken cancellation)
	{
		TResponse? response = Handle(request, cancellation);
		return Task.FromResult(response);
	}
	protected virtual TResponse? Handle(HandlerRequest<TRequest> request, CancellationToken cancellation)
	{
		return default;
	}
	#endregion
}

internal abstract class BaseCustomHandler<TRequest, TResponse, TResolve> : BaseCustomHandler<TRequest, TResponse>, ICustomHandler<TRequest, TResponse, TResolve>
	where TRequest : notnull
	where TResolve : notnull
{
	#region Methods
	public Task<TResolve?> ResolveAsync(ResolveRequest<TResolve> request, CancellationToken cancellation)
	{
		TResolve? response = Resolve(request, cancellation);
		return Task.FromResult(response);
	}
	protected virtual TResolve? Resolve(ResolveRequest<TResolve> request, CancellationToken cancellation) => default;
	#endregion
}
