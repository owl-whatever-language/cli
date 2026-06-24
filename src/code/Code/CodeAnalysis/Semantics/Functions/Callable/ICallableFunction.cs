namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Callable;

public interface ICallableFunction : ICallableType
{
	#region Properties
	IFunction Function { get; }
	new IReadOnlyList<ICallableFunctionParameter> Parameters { get; }
	new ICallableFunctionReturn Return { get; }

	IReadOnlyList<ICallableTypeParameter> ICallableType.Parameters => Parameters;
	ICallableTypeReturn ICallableType.Return => Return;
	#endregion
}

public sealed class CallableFunction : ICallableFunction
{
	#region Properties
	public IFunction Function { get; }
	public IReadOnlyList<ICallableFunctionParameter> Parameters { get; }
	public ICallableFunctionReturn Return { get; }
	#endregion

	#region Constructors
	public CallableFunction(IFunction function)
	{
		Function = function;
		Parameters = function.Parameters.Select(p => p.AsCallable).ToArray();
		Return = function.Return.AsCallable;
	}
	#endregion

	#region Methods
	public bool CanAssignTo(IType target) => Equals(target);
	public bool Equals([NotNullWhen(true)] ICallableType? other)
	{
		if (other is null)
			return false;

		if (Parameters.Count != other.Parameters.Count)
			return false;

		for (int i = 0; i < Parameters.Count; i++)
		{
			if (Parameters[i].Type.Equals(other.Parameters[i].Type) is false)
				return false;
		}

		return Return.Type.Equals(other.Return.Type);
	}
	public bool Equals([NotNullWhen(true)] IType? other) => Equals(other as ICallableType);
	public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as ICallableType);

	public override int GetHashCode()
	{
		HashCode code = new();
		code.Add(Parameters.Count);

		foreach (ICallableFunctionParameter parameter in Parameters)
			code.Add(parameter.Type);

		code.Add(Return.Type);

		return code.ToHashCode();
	}
	#endregion
}
