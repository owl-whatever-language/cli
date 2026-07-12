namespace OwlDomain.Owl.Code.Execution.Builtins;

internal sealed class BuiltinContext
{
	#region Fields
	private readonly Dictionary<Type, BuiltinType> _lookup = [];
	#endregion

	#region Properties
	public SymbolScope TargetScope { get; set; }
	#endregion

	#region Indexers
	public BuiltinType this[Type type] => _lookup[type];
	#endregion

	#region Constructors
	public BuiltinContext(SymbolScope target)
	{
		TargetScope = target;
	}
	#endregion

	#region Resolve methods
	public void ResolveCore() => Core.CoreBuiltins.Resolve(this);
	public void ResolveStandard() => Standard.StandardBuiltins.Resolve(this);
	#endregion

	#region Type methods
	public BuiltinType AddType<TBacking, TType>(string name, Func<TBacking, TType> constructor)
		where TBacking : notnull
	{
		BuiltinType type = new(name);
		_lookup.Add(typeof(TType), type);

		InterpreterValue Create(object? value)
		{
			if (value is null)
				ThrowHelper.ThrowInvalidOperationException($"Couldn't create a '{name}' instance from a null value.");

			TBacking typed = (TBacking)value;

			TType result = constructor.Invoke(typed);
			return new(type, result);
		}

		type.BackingConstructor = Create;
		TargetScope.Add(type);

		return type;
	}
	public BuiltinType AddType(Type typeInfo, string name, BuiltinType.BackingConstructorDelegate<object?> constructor)
	{
		BuiltinType type = new(name);
		_lookup.Add(typeInfo, type);

		type.BackingConstructor = constructor;
		TargetScope.Add(type);

		return type;
	}
	#endregion

	#region Function methods
	private IReadOnlyList<BuiltinFunctionParameter> Convert(IReadOnlyList<(Type, string)> parameters)
	{
		List<BuiltinFunctionParameter> result = [];

		foreach ((Type typeInfo, string name) in parameters)
		{
			IType type = _lookup[typeInfo];
			BuiltinFunctionParameter parameter = new(result.Count, type, name);
			result.Add(parameter);
		}

		return result;
	}
	public BuiltinFunction AddFunction(string name, IReadOnlyList<(Type, string)> parameters, Action<IReadOnlyList<InterpreterValue>> callback)
	{
		BuiltinFunction function = new(name, Convert(parameters), new(SpecialTypes.Void), (c, p) =>
		{
			callback.Invoke(p);
			return InterpreterValue.Void;
		});

		TargetScope.Add(function);
		return function;
	}
	public BuiltinFunction AddFunction(string name, IReadOnlyList<(Type, string)> parameters, Action<IExecutionContext, IReadOnlyList<InterpreterValue>> callback)
	{
		BuiltinFunction function = new(name, Convert(parameters), new(SpecialTypes.Void), (c, p) =>
		{
			callback.Invoke(c, p);
			return InterpreterValue.Void;
		});

		TargetScope.Add(function);
		return function;
	}
	public BuiltinFunction AddFunction(string name, IReadOnlyList<(Type, string)> parameters, IType returnType, Func<IReadOnlyList<InterpreterValue>, InterpreterValue> callback)
	{
		BuiltinFunction function = new(name, Convert(parameters), new(returnType), (c, p) => callback.Invoke(p));

		TargetScope.Add(function);
		return function;
	}
	public BuiltinFunction AddFunction(string name, IReadOnlyList<(Type, string)> parameters, IType returnType, Func<IExecutionContext, IReadOnlyList<InterpreterValue>, InterpreterValue> callback)
	{
		BuiltinFunction function = new(name, Convert(parameters), new(returnType), callback.Invoke);

		TargetScope.Add(function);
		return function;
	}
	#endregion

	#region Void methods
	public BuiltinFunction AddFunction(string name, Action callback)
	{
		return AddFunction(
			name,
			[],
			(c, p) => callback.Invoke()
		);
	}
	public BuiltinFunction AddFunction<T1>(string name, string param1, Action<T1> callback)
	{
		return AddFunction(
			name,
			[
				(typeof(T1), param1)
			],
			(c, p) => callback.Invoke((T1)p[0].Value!)
		);
	}
	public BuiltinFunction AddFunction<T1, T2>(string name, string param1, string param2, Action<T1, T2> callback)
	{
		return AddFunction(
			name,
			[
				(typeof(T1), param1),
				(typeof(T2), param2),
			],
			(c, p) => callback.Invoke(
				(T1)p[0].Value!,
				(T2)p[1].Value!
			)
		);
	}
	#endregion

	#region Return methods
	public BuiltinFunction AddFunction<TOut>(string name, Func<TOut> callback)
	{
		IType resultType = _lookup[typeof(TOut)];

		return AddFunction(
			name,
			[],
			resultType,
			(c, p) =>
			{
				TOut result = callback.Invoke();

				return new(resultType, result);
			});
	}
	public BuiltinFunction AddFunction<T1, TOut>(string name, string param1, Func<T1, TOut> callback)
	{
		IType resultType = _lookup[typeof(TOut)];

		return AddFunction(
			name,
			[
				(typeof(T1), param1)
			],
			resultType,
			(c, p) =>
			{
				TOut result = callback.Invoke((T1)p[0].Value!);

				return new(resultType, result);
			});
	}
	public BuiltinFunction AddFunction<T1, T2, TOut>(string name, string param1, string param2, Func<T1, T2, TOut> callback)
	{
		IType resultType = _lookup[typeof(TOut)];

		return AddFunction(
			name,
			[
				(typeof(T1), param1),
				(typeof(T2), param2)
			],
			resultType,
			(c, p) =>
			{
				TOut result = callback.Invoke(
					(T1)p[0].Value!,
					(T2)p[1].Value!
				);

				return new(resultType, result);
			});
	}
	public BuiltinFunction AddFunction<T1, T2, T3, TOut>(string name, string param1, string param2, string param3, Func<T1, T2, T3, TOut> callback)
	{
		IType resultType = _lookup[typeof(TOut)];

		return AddFunction(
			name,
			[
				(typeof(T1), param1),
				(typeof(T2), param2),
				(typeof(T3), param3)
			],
			resultType,
			(c, p) =>
			{
				TOut result = callback.Invoke(
					(T1)p[0].Value!,
					(T2)p[1].Value!,
					(T3)p[2].Value!
				);

				return new(resultType, result);
			});
	}
	#endregion

	#region Operator methods
	public void AddBinary<TLeft, TRight, TOut>(BuiltinType type, OperatorKind kind, Func<TLeft, TRight, TOut> callback)
	{
		IType outType = _lookup[typeof(TOut)];

		BuiltinFunction function = new(
		  kind.ToString(),
		  Convert([
			  (typeof(TLeft), "left"),
			  (typeof(TRight), "right"),
		  ]),
		  new(outType),
		  (c, p) =>
		  {
			  TOut result = callback.Invoke((TLeft)p[0].Value!, (TRight)p[1].Value!);
			  return new(outType, result);
		  });

		type.Operations.Add(function);
	}
	#endregion

	#region Property methods
	public void AddProperty<TDeclaring, TValue>(string name, Func<TDeclaring, TValue> getter)
	{
		BuiltinType declaringType = _lookup[typeof(TDeclaring)];
		IType type = _lookup[typeof(TValue)];

		InterpreterValue Getter(InterpreterValue instance)
		{
			TDeclaring declaring = (TDeclaring)instance.Value!;
			TValue value = getter.Invoke(declaring);

			return new(type, value);
		}

		BuiltinTypeProperty property = new(declaringType, type, name, Getter);
		declaringType.Properties.Add(property);
	}
	#endregion

}
