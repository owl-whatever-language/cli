using System.Text;

namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Callables;

public interface ICallable : ITypeInfo
{
	#region Properties
	IFunction? Function { get; }
	IReadOnlyList<ICallableParameter> Parameters { get; }
	ICallableReturn Return { get; }
	bool IsMutable { get; }
	#endregion
}

public sealed class Callable : ICallable
{
	#region Properties
	public IFunction? Function { get; }
	public IReadOnlyList<ICallableParameter> Parameters { get; }
	public ICallableReturn Return { get; }
	public bool IsMutable => Return.IsMutable || Parameters.Any(p => p.IsMutable);
	#endregion

	#region Constructors
	public Callable(IFunction? function, IReadOnlyList<ICallableParameter> parameters, ICallableReturn @return)
	{
		Function = function;
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

	public override string ToString()
	{
		StringBuilder builder = new();
		builder.Append("callable(");

		for (int i = 0; i < Parameters.Count; i++)
		{
			if (i > 0)
				builder.Append(", ");

			builder.Append(Parameters[i].Type?.ToString() ?? "???");
		}

		builder.Append(')');
		if (Return is not null)
		{
			builder.Append(": ");
			builder.Append(Return.Type?.ToString() ?? "???");
		}

		return builder.ToString();
	}
	#endregion
}
