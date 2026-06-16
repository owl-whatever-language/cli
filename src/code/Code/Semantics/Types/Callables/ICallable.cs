namespace OwlDomain.OWL.Code.Semantics.Types.Callables;

public interface ICallable : ITypeInfo
{
	#region Properties
	IReadOnlyList<ICallableParameter> Parameters { get; }
	ICallableReturn Return { get; }
	#endregion
}

public sealed class Callable : ICallable
{
	#region Properties
	public string? Name { get; }
	public IReadOnlyList<ICallableParameter> Parameters { get; }
	public ICallableReturn Return { get; }
	#endregion

	#region Constructors
	public Callable(string? name, IReadOnlyList<ICallableParameter> parameters, ICallableReturn @return)
	{
		Name = name;
		Parameters = parameters;
		Return = @return;
	}
	public Callable(IReadOnlyList<ICallableParameter> parameters, ICallableReturn @return) : this(null, parameters, @return) { }
	#endregion

	#region Methods
	public bool CanAssignTo(ITypeInfo target)
	{
		if (target is not ICallable other)
			return false;

		if (Parameters.Count != other.Parameters.Count)
			return false;

		for (int i = 0; i < Parameters.Count; i++)
		{
			if (Parameters[i].CanAssignTo(other.Parameters[i]) is false)
				return false;
		}

		return Return.CanAssignTo(other.Return);
	}
	#endregion
}
