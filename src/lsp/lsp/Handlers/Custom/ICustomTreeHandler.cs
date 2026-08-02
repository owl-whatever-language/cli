namespace OwlDomain.Owl.LSP.Handlers.Custom;

internal interface ICustomTreeHandler<TRequest, TResponse, TTree>
	where TRequest : notnull
	where TTree : notnull, ISyntaxTree
{
	#region Methods
	Task<TResponse?> HandleAsync(HandlerRequest<TRequest> request, TTree tree, CancellationToken cancellation);
	#endregion
}

internal interface ICustomTreeHandler<TRequest, TResponse, TTree, TResolve> : ICustomTreeHandler<TRequest, TResponse, TTree>
	where TRequest : notnull
	where TTree : notnull, ISyntaxTree
	where TResolve : notnull
{
	#region Methods
	Task<TResolve?> ResolveAsync(ResolveRequest<TResolve> request, CancellationToken cancellation);
	#endregion
}

internal abstract class BaseCustomTreeHandler<TRequest, TResponse, TTree> : ICustomTreeHandler<TRequest, TResponse, TTree>
	where TRequest : notnull
	where TTree : notnull, ISyntaxTree
{
	#region Methods
	public virtual Task<TResponse?> HandleAsync(HandlerRequest<TRequest> request, TTree tree, CancellationToken cancellation)
	{
		TResponse? response = Handle(request, tree, cancellation);
		return Task.FromResult(response);
	}
	protected virtual TResponse? Handle(HandlerRequest<TRequest> request, TTree tree, CancellationToken cancellation)
	{
		return default;
	}
	#endregion
}

internal abstract class BaseCustomTreeHandler<TRequest, TResponse, TTree, TResolve> : BaseCustomTreeHandler<TRequest, TResponse, TTree>, ICustomTreeHandler<TRequest, TResponse, TTree, TResolve>
	where TRequest : notnull
	where TTree : notnull, ISyntaxTree
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
