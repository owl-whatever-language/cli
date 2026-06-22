using System.Text;

namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;

public interface IFunction : INamedSymbolTarget
{
	#region Properties
	ICallable? Callable { get; set; }
	#endregion
}

public sealed class Function : BaseNamedSymbolTarget, IFunction
{
	#region Properties
	public override string Kind => "function";
	public ICallable? Callable
	{
		get;
		set => Set(ref field, value);
	}
	#endregion

	#region Constructors
	public Function(string? name) : base(name) { }
	#endregion

	#region Methods
	public Function WithCallable(ICallable callable)
	{
		Callable = callable;
		return this;
	}
	protected override void ValidateImmutableState()
	{
		if (Callable?.IsMutable is true)
			ThrowHelper.ThrowInvalidOperationException("Can't lock a function until its callable has been locked.");

		base.ValidateImmutableState();
	}
	public override string ToString()
	{
		StringBuilder builder = new();
		builder.Append(Name ?? "???");

		if (Callable is null)
			return builder.ToString();

		builder.Append('(');

		for (int i = 0; i < Callable.Parameters.Count; i++)
		{
			if (i > 0)
				builder.Append(", ");

			ICallableParameter parameter = Callable.Parameters[i];
			builder.Append(parameter.Type?.ToString() ?? "???");

			if (parameter.Name is not null)
				builder.Append(' ').Append(parameter.Name);
		}

		builder.Append(')');
		if (Callable?.Return is not null)
		{
			builder.Append(": ");
			builder.Append(Callable.Return.Type?.ToString() ?? "???");
		}

		return builder.ToString();
	}
	#endregion
}
