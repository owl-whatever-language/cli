using System.Text;
using OwlDomain.Owl.Code.Execution.Builtins;

namespace OwlDomain.Owl.Code.Execution;

public sealed class InterpretingResult : IStageResultPerformance, IStageResultDiagnostics
{
	#region Properties
	public string Stage => "interpreting";
	public IDiagnosticBag Diagnostics { get; }
	public IPerformanceResult Performance { get; }
	#endregion

	#region Constructors
	public InterpretingResult(IDiagnosticBag diagnostics, IPerformanceResult performance)
	{
		Diagnostics = diagnostics;
		Performance = performance;
	}
	#endregion
}

public class Interpreter
{
	#region Nested types
	private sealed class ExecutionContext : IExecutionContext { }
	private sealed class ValueScope
	{
		#region Properties
		private ValueScope? Parent { get; }
		private Dictionary<ISymbol, InterpreterValue> Values { get; } = [];
		#endregion

		#region Constructors
		public ValueScope(ValueScope? parent)
		{
			Parent = parent;
		}
		#endregion

		#region Methods
		public void Declare(ILocalVariable variable, InterpreterValue value)
		{
			if (value.Type.CanAssignTo(variable.Type) is false)
				ThrowHelper.ThrowInvalidOperationException($"Expected the value's type '{value.Type}' to be assignable to the variable's type '{variable.Type}'.");

			Values.Add(variable, value);
		}
		public void Declare(IFunctionParameter parameter, InterpreterValue value)
		{
			if (value.Type.CanAssignTo(parameter.Type) is false)
				ThrowHelper.ThrowInvalidOperationException($"Expected the value's type '{value.Type}' to be assignable to the parameters's type '{parameter.Type}'.");

			Values.Add(parameter, value);
		}
		public InterpreterValue Get(ISymbol symbol)
		{
			if (Values.TryGetValue(symbol, out InterpreterValue value))
				return value;

			if (Parent is not null)
				return Parent.Get(symbol);

			ThrowHelper.ThrowInvalidOperationException($"The symbol '{symbol}' hasn't been declared yet.");
			return default;
		}
		#endregion
	}
	private sealed class ReturnControlException(InterpreterValue value) : Exception
	{
		#region Properties
		public InterpreterValue Value { get; } = value;
		#endregion
	}
	#endregion

	#region Properties
	private DiagnosticBag Diagnostics { get; } = [];
	private ValueScope Values { get; set; } = new(null);
	private ExecutionContext Context { get; } = new();
	#endregion

	#region Constructors
	private Interpreter() { }
	#endregion

	#region Functions
	public static InterpretingResult Interpret(IAnnotatedSyntaxTree tree)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			Interpreter interpreter = new();
			interpreter.InterpretTree(tree);

			return new(interpreter.Diagnostics, performance);
		}
	}
	#endregion

	#region Methods
	private void InterpretTree(IAnnotatedSyntaxTree tree)
	{
		Interpret(tree.Document.Statements);
	}
	private void Interpret(IReadOnlyList<IAnnotatedStatementSyntax> statements)
	{
		foreach (IAnnotatedStatementSyntax statement in statements)
			Interpret(statement);
	}
	private void Interpret(IAnnotatedStatementSyntax statement)
	{
		if (statement is IAnnotatedExpressionStatementSyntax expression)
			_ = Evaluate(expression.Expression);
		else if (statement is IAnnotatedBlockStatementSyntax block)
			Interpret(block.Statements);
		else if (statement is IAnnotatedVariableDeclarationStatementSyntax variable)
		{
			InterpreterValue value = Evaluate(variable.Value);
			Values.Declare(variable.Variable, value);
		}
		else if (statement is IAnnotatedLocalFunctionDeclarationStatementSyntax)
		{ /* We don't need to do anything for function declarations */ }
		else if (statement is IAnnotatedReturnStatementSyntax)
			throw new ReturnControlException(InterpreterValue.Void);
		else if (statement is IAnnotatedValueReturnStatementSyntax @return)
		{
			InterpreterValue value = Evaluate(@return.Value);
			throw new ReturnControlException(value);
		}
	}
	#endregion

	#region Evaluate methods
	private InterpreterValue Evaluate(IAnnotatedExpressionSyntax expression)
	{
		return expression switch
		{
			IAnnotatedStringLiteralExpressionSyntax str => Evaluate(str),
			IAnnotatedInterpolatedStringExpressionSyntax str => Evaluate(str),
			IAnnotatedGetExpressionSyntax get => Evaluate(get),
			IAnnotatedFunctionCallExpressionSyntax call => Evaluate(call),

			_ => ThrowHelper.ThrowInvalidOperationException<InterpreterValue>($"The expression type '{expression.GetType().Name}' is not supported by the interpreter.")
		};
	}
	private InterpreterValue Evaluate(IAnnotatedGetExpressionSyntax expression)
	{
		return expression.Symbol switch
		{
			ILocalVariable or IFunctionParameter => Values.Get(expression.Symbol),
			IFunction function => new(function.AsCallable, function),

			_ => ThrowHelper.ThrowInvalidOperationException<InterpreterValue>($"The get expression's symbol '{expression.Symbol.GetType().Name}' is not supported by the interpreter.")
		};
	}
	private InterpreterValue Evaluate(IAnnotatedStringLiteralExpressionSyntax expression)
	{
		BuiltinType type = (BuiltinType)expression.ResultType;
		string value = expression.Value;

		return type.CreateInstance(value);
	}
	private InterpreterValue Evaluate(IAnnotatedInterpolatedStringExpressionSyntax expression)
	{
		BuiltinType type = (BuiltinType)expression.ResultType;

		StringBuilder builder = new();

		foreach (IAnnotatedStringFragmentSyntax fragment in expression.Fragments)
		{
			InterpreterValue value = fragment switch
			{
				IAnnotatedRegularStringFragmentSyntax regular => type.CreateInstance(regular.Text.Value),
				IAnnotatedEscapedStringFragmentSyntax escaped => type.CreateInstance(escaped.Sequence.Value),
				IAnnotatedEscapedHexStringFragmentSyntax escaped => type.CreateInstance(escaped.Sequence.Value),
				IAnnotatedInterpolatedStringFragmentSyntax interpolated => Evaluate(interpolated.Value),

				_ => ThrowHelper.ThrowInvalidOperationException<InterpreterValue>($"The string fragment type '{fragment.GetType().Name}' is not supported by the interpreter.")
			};

			// Todo(Nightowl): This should be handled through some built-in text conversation support;
			string? textValue = value.Value?.ToString();
			builder.Append(textValue);
		}

		string finalValue = builder.ToString();
		return type.CreateInstance(finalValue);
	}
	private InterpreterValue Evaluate(IAnnotatedFunctionCallExpressionSyntax expression)
	{
		InterpreterValue value = Evaluate(expression.Expression);

		return value.Value switch
		{
			BuiltinFunction function => Evaluate(expression, function),
			IDeclaredFunction function => Evaluate(expression, (IAnnotatedFunctionDeclarationStatementSyntax)function.Declaration),

			_ => ThrowHelper.ThrowInvalidOperationException<InterpreterValue>($"The type '{value.Type}' is not supported for calling by the interpreter.")
		};
	}
	private InterpreterValue Evaluate(IAnnotatedFunctionCallExpressionSyntax expression, IAnnotatedFunctionDeclarationStatementSyntax function)
	{
		bool needsReturnValue = function.Function.Return.Type != SpecialTypes.Void;
		IReadOnlyDictionary<IFunctionParameter, InterpreterValue> arguments = EvaluateArguments(expression, function.Function);

		ValueScope oldValues = Values;
		Values = new(Values);
		try
		{
			foreach (KeyValuePair<IFunctionParameter, InterpreterValue> pair in arguments)
				Values.Declare(pair.Key, pair.Value);

			if (function.Body is IAnnotatedShortFunctionBodySyntax @short)
			{
				InterpreterValue value = Evaluate(@short.Expression);
				return value;
			}

			if (function.Body is IAnnotatedBlockFunctionBodySyntax block)
				Interpret(block.Block);

			if (needsReturnValue)
				ThrowHelper.ThrowInvalidOperationException($"The function '{function.Function}' didn't return with a value.");

			return default;
		}
		catch (ReturnControlException @return)
		{
			if (needsReturnValue && @return.Value.Type == SpecialTypes.Void)
				ThrowHelper.ThrowInvalidOperationException($"The function '{function.Function}' didn't return with a value.");

			return @return.Value;
		}
		finally
		{
			Values = oldValues;
		}
	}
	private InterpreterValue Evaluate(IAnnotatedFunctionCallExpressionSyntax expression, BuiltinFunction function)
	{
		IReadOnlyDictionary<IFunctionParameter, InterpreterValue> byParameter = EvaluateArguments(expression, function);

		InterpreterValue[] ordered = new InterpreterValue[function.Parameters.Count];
		for (int i = 0; i < ordered.Length; i++)
		{
			IFunctionParameter parameter = function.Parameters[i];
			InterpreterValue value = byParameter[parameter];

			ordered[i] = value;
		}

		InterpreterValue result = function.Execute(Context, ordered);
		return result;
	}
	private IReadOnlyDictionary<IFunctionParameter, InterpreterValue> EvaluateArguments(IAnnotatedFunctionCallExpressionSyntax expression, IFunction function)
	{
		// Note(Nightowl): This argument/parameter matching approach was chosen to make it easy to allow default / 'params' like arguments;
		Dictionary<ICallableTypeParameter, List<InterpreterValue>> byParameter = [];

		foreach (IAnnotatedFunctionArgumentSyntax argument in expression.Arguments.Values)
		{
			if (argument.Parameter is null)
				ThrowHelper.ThrowInvalidOperationException($"Expected the argument's ({argument}) parameter to be worked out.");

			InterpreterValue value = argument switch
			{
				IAnnotatedRegularFunctionArgumentSyntax regular => Evaluate(regular.Value),
				IAnnotatedNamedFunctionArgumentSyntax named => Evaluate(named.Value),

				_ => ThrowHelper.ThrowInvalidOperationException<InterpreterValue>($"The argument type '{argument.GetType().Name}' is not supported by the interpreter.")
			};

			if (byParameter.TryGetValue(argument.Parameter, out List<InterpreterValue>? parameterValues) is false)
			{
				parameterValues = [];
				byParameter.Add(argument.Parameter, parameterValues);
			}

			parameterValues.Add(value);
		}

		Dictionary<IFunctionParameter, InterpreterValue> arguments = [];

		foreach (IFunctionParameter parameter in function.Parameters)
		{
			List<InterpreterValue> values = byParameter[parameter.AsCallable];

			if (values.Count is not 1)
				ThrowHelper.ThrowInvalidOperationException($"Expected the parameter '{parameter}' to have exactly one argument.");

			arguments[parameter] = values[0];
		}

		return arguments;
	}
	#endregion
}
