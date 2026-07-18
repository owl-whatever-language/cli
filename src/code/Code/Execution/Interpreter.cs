using System.Text;
using OwlDomain.Owl.Code.Execution.Builtins;
using OwlDomain.Owl.Code.Execution.Builtins.Core;

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

public class Interpreter : IDiagnosticProvider
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
		public void Store(ISymbol symbol, InterpreterValue value)
		{
			if (Values.ContainsKey(symbol))
			{
				Values[symbol] = value;
				return;
			}

			if (Parent is not null)
			{
				Parent.Store(symbol, value);
				return;
			}

			ThrowHelper.ThrowInvalidOperationException($"The symbol '{symbol.Name}' hasn't been declared yet.");
		}
		#endregion
	}
	private sealed class ReturnControlException(InterpreterValue value) : Exception
	{
		#region Properties
		public InterpreterValue Value { get; } = value;
		#endregion
	}
	private sealed class InterpreterException : Exception
	{
		#region Properties
		public IAnnotatedSyntaxNode Node { get; }
		#endregion

		#region Constructors
		public InterpreterException(IAnnotatedSyntaxNode node, Exception inner) : base(null, inner)
		{
			Node = node;
		}
		#endregion
	}
	#endregion

	#region Properties
	public string Name => "interpreter";
	private DiagnosticBag Diagnostics { get; } = [];
	private ValueScope Values { get; set; } = new(null);
	private ExecutionContext Context { get; } = new();
	private IAnnotatedSyntaxNode Node { get; set; }
	#endregion

	#region Constructors
	private Interpreter(IAnnotatedSyntaxNode node) => Node = node;
	#endregion

	#region Functions
	public static InterpretingResult Interpret(IAnnotatedSyntaxTree tree)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			Interpreter interpreter = new(tree.Document);
			interpreter.InterpretTree(tree);

			return new(interpreter.Diagnostics, performance);
		}
	}
	#endregion

	#region Methods
	private void InterpretTree(IAnnotatedSyntaxTree tree)
	{
		if (Debugger.IsAttached)
		{
			Interpret(tree.Document.Statements);
			return;
		}

		try
		{
			Interpret(tree.Document.Statements);
		}
		catch (InterpreterException exception)
		{
			Diagnostics
				.BuildError(this, "runtime_exception")
				.Add(exception.Node, lines => lines.AddLine(exception.InnerException?.Message));
		}
		catch (Exception exception)
		{
			Diagnostics
				.BuildError(this, "interpreter_exception")
				.Add(Node, lines => lines.AddLine(exception.InnerException?.Message));
		}
	}
	private void Interpret(IReadOnlyList<IAnnotatedStatementSyntax> statements)
	{
		foreach (IAnnotatedStatementSyntax statement in statements)
			Interpret(statement);
	}
	private void InterpretValueScope(IAnnotatedStatementSyntax body)
	{
		var old = Values;
		Values = new(Values);
		try
		{
			Interpret(body);
		}
		finally
		{
			Values = old;
		}
	}
	private void Interpret(IAnnotatedStatementSyntax statement)
	{
		if (statement.IsExecutable is false)
			return;

		Node = statement;

		if (statement is IAnnotatedExpressionStatementSyntax expression)
			_ = Evaluate(expression.Expression);
		else if (statement is IAnnotatedBlockStatementSyntax block)
			Interpret(block.Statements);
		else if (statement is IAnnotatedVariableDeclarationStatementSyntax variable)
		{
			InterpreterValue value = Evaluate(variable.Value);
			Values.Declare(variable.Variable, value);
		}
		else if (statement is IAnnotatedReturnStatementSyntax)
			throw new ReturnControlException(InterpreterValue.Void);
		else if (statement is IAnnotatedValueReturnStatementSyntax @return)
		{
			InterpreterValue value = Evaluate(@return.Value);
			throw new ReturnControlException(value);
		}
		else if (statement is IAnnotatedWhileStatementSyntax @while)
			Interpret(@while);
		else if (statement is IAnnotatedIfStatementSyntax @if)
			Interpret(@if);
		else if (statement is IAnnotatedIfElseStatementSyntax ifElse)
			Interpret(ifElse);
		else
			ThrowHelper.ThrowInvalidOperationException($"Unhandled statement type ({statement.GetType().Name}).");
	}
	private void Interpret(IAnnotatedWhileStatementSyntax @while)
	{
		while (EvaluateCondition(@while.Condition))
			InterpretValueScope(@while.Body);
	}
	private void Interpret(IAnnotatedIfStatementSyntax @if)
	{
		if (EvaluateCondition(@if.Condition))
			InterpretValueScope(@if.TrueClause);
	}
	private void Interpret(IAnnotatedIfElseStatementSyntax @if)
	{
		if (EvaluateCondition(@if.Condition))
			InterpretValueScope(@if.TrueClause);
		else
			InterpretValueScope(@if.FalseClause);
	}
	#endregion

	#region Evaluate methods
	private bool EvaluateCondition(IAnnotatedExpressionSyntax condition)
	{
		InterpreterValue result = Evaluate(condition);
		if (result.Value is CoreBuiltins.Bool typed)
			return typed.Value;

		ThrowHelper.ThrowInvalidOperationException($"Expected the result of the condition ({result.Value?.GetType()}) be a boolean.");
		return default;
	}
	private InterpreterValue Evaluate(IAnnotatedExpressionSyntax expression)
	{
		Node = expression;

		return expression switch
		{
			IAnnotatedStringLiteralExpressionSyntax str => Evaluate(str),
			IAnnotatedInterpolatedStringExpressionSyntax str => Evaluate(str),

			IAnnotatedIntegerLiteralExpressionSyntax num => Evaluate(num),
			IAnnotatedDecimalLiteralExpressionSyntax num => Evaluate(num),
			IAnnotatedBooleanLiteralExpressionSyntax num => Evaluate(num),

			IAnnotatedBinaryExpressionSyntax binary => Evaluate(binary),
			IAnnotatedAssignmentExpressionSyntax assignment => Evaluate(assignment),
			IAnnotatedCompoundAssignmentExpressionSyntax assignment => Evaluate(assignment),

			IAnnotatedGetExpressionSyntax get => Evaluate(get),
			IAnnotatedGroupedExpressionSyntax grouped => Evaluate(grouped.Expression),
			IAnnotatedMemberAccessExpressionSyntax member => Evaluate(member),
			IAnnotatedFunctionCallExpressionSyntax call => Evaluate(call),

			_ => ThrowHelper.ThrowInvalidOperationException<InterpreterValue>($"The expression type '{expression.GetType().Name}' is not supported by the interpreter.")
		};
	}
	private InterpreterValue Evaluate(IAnnotatedBinaryExpressionSyntax expression)
	{
		if (expression.Operation is null)
			ThrowHelper.ThrowInvalidOperationException("Expected the operator function to be available.");

		BuiltinBinaryOperator operation = (BuiltinBinaryOperator)expression.Operation;

		InterpreterValue left = Evaluate(expression.Left);

		// Note(Nightowl): A little cheat-y for short-circuiting;
		if (expression.Operator.Kind == SyntaxKind.DoubleAmpersand) // and
		{
			if (left.Value is CoreBuiltins.Bool l && l.Value is false)
				return left;
		}
		else if (expression.Operator.Kind == SyntaxKind.DoublePipe) // or
		{
			if (left.Value is CoreBuiltins.Bool l && l.Value)
				return left;
		}

		InterpreterValue right = Evaluate(expression.Right);

		InterpreterValue result = operation.AsFunction.Execute(Context, [left, right]);
		return result;
	}
	private InterpreterValue Evaluate(IAnnotatedAssignmentExpressionSyntax assignment)
	{
		InterpreterValue value = Evaluate(assignment.Value);
		Values.Store(assignment.Symbol, value);

		return value;
	}
	private InterpreterValue Evaluate(IAnnotatedCompoundAssignmentExpressionSyntax assignment)
	{
		if (assignment.Operation is null)
			ThrowHelper.ThrowInvalidOperationException("Expected the operator function to be available.");

		BuiltinBinaryOperator operation = (BuiltinBinaryOperator)assignment.Operation;

		InterpreterValue current = Evaluate(assignment.Expression);
		InterpreterValue value = Evaluate(assignment.Value);

		value = operation.AsFunction.Execute(Context, [current, value]);
		Values.Store(assignment.Symbol, value);

		return value;
	}
	#endregion

	#region Access methods
	private InterpreterValue Evaluate(IAnnotatedGetExpressionSyntax expression)
	{
		return expression.Symbol switch
		{
			ILocalVariable or IFunctionParameter => Values.Get(expression.Symbol),
			IFunction function => new(function.AsCallable, function),

			_ => ThrowHelper.ThrowInvalidOperationException<InterpreterValue>($"The get expression's symbol '{expression.Symbol.GetType().Name}' is not supported by the interpreter.")
		};
	}
	private InterpreterValue Evaluate(IAnnotatedMemberAccessExpressionSyntax expression)
	{
		return expression.Symbol switch
		{
			BuiltinTypeProperty property => property.Getter.Invoke(Evaluate(expression.Expression)),
			BuiltinTypeMethod => InterpreterValue.Void, // Note(Nightowl): Will be ignored by the interpreter right now anyway;

			_ => ThrowHelper.ThrowInvalidOperationException<InterpreterValue>($"The member access expression's symbol '{expression.Symbol.GetType().Name}' is not supported by the interpreter.")
		};
	}
	#endregion

	#region Literal expression methods
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
	private InterpreterValue Evaluate(IAnnotatedIntegerLiteralExpressionSyntax expression)
	{
		if (expression.Value is null)
			ThrowHelper.ThrowInvalidOperationException("Expected the integer literal to have a value.");

		BuiltinType type = (BuiltinType)expression.ResultType;

		long value = expression.Value.Value;
		return type.CreateInstance(value);
	}
	private InterpreterValue Evaluate(IAnnotatedDecimalLiteralExpressionSyntax expression)
	{
		if (expression.Value is null)
			ThrowHelper.ThrowInvalidOperationException("Expected the decimal literal to have a value.");

		BuiltinType type = (BuiltinType)expression.ResultType;

		decimal value = expression.Value.Value;
		return type.CreateInstance(value);
	}
	private InterpreterValue Evaluate(IAnnotatedBooleanLiteralExpressionSyntax expression)
	{
		if (expression.Value is null)
			ThrowHelper.ThrowInvalidOperationException("Expected the boolean literal to have a value.");

		BuiltinType type = (BuiltinType)expression.ResultType;

		bool value = expression.Value.Value;
		return type.CreateInstance(value);
	}
	#endregion

	#region Method call methods
	private InterpreterValue Evaluate(
		IAnnotatedFunctionCallExpressionSyntax call,
		IAnnotatedMemberAccessExpressionSyntax access,
		ITypeMethod target)
	{
		// Note(Nightowl): Later this value will actually have to be used;
		_ = Evaluate(call.Expression);
		InterpreterValue instance = Evaluate(access.Expression);

		if (target is not BuiltinTypeMethod method)
		{
			ThrowHelper.ThrowInvalidOperationException($"Unsupported method type ({target.GetType().Name}).");
			return default;
		}

		IReadOnlyDictionary<IFunctionParameter, InterpreterValue> arguments = EvaluateArguments(call, method.Function);

		InterpreterValue[] ordered = new InterpreterValue[arguments.Count + 1];
		ordered[0] = instance;

		for (int i = 1; i < ordered.Length; i++)
		{
			IFunctionParameter parameter = method.Function.Parameters[i - 1];
			InterpreterValue value = arguments[parameter];

			ordered[i] = value;
		}
		try
		{
			InterpreterValue result = method.Function.Execute(Context, ordered);
			return result;
		}
		catch (Exception exception) when (exception is not InterpreterException)
		{
			throw new InterpreterException(Node, exception);
		}
	}
	#endregion

	#region Function call methods
	private InterpreterValue Evaluate(IAnnotatedFunctionCallExpressionSyntax expression)
	{
		if (expression.Expression is IAnnotatedMemberAccessExpressionSyntax access && access.Symbol is ITypeMethod method)
			return Evaluate(expression, access, method);

		IFunction function = expression.Callable switch
		{
			ICallableFunction callable => callable.Function,

			_ => ThrowHelper.ThrowInvalidOperationException<IFunction>($"Unknown callable type ({expression.Callable?.GetType().Name}).")
		};

		IReadOnlyDictionary<IFunctionParameter, InterpreterValue> arguments = EvaluateArguments(expression, function);

		InterpreterValue result = Evaluate(function, arguments);
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
	private InterpreterValue Evaluate(IFunction function, IReadOnlyDictionary<IFunctionParameter, InterpreterValue> values)
	{
		return function switch
		{
			BuiltinFunction builtin => Evaluate(builtin, values),
			IDeclaredFunction declared => Evaluate((IAnnotatedFunctionDeclarationStatementSyntax)declared.Declaration, values),

			_ => ThrowHelper.ThrowInvalidOperationException<InterpreterValue>($"Unknown function type ({function.GetType().Name}).")
		};
	}
	private InterpreterValue Evaluate(BuiltinFunction function, IReadOnlyDictionary<IFunctionParameter, InterpreterValue> values)
	{
		InterpreterValue[] ordered = new InterpreterValue[function.Parameters.Count];
		for (int i = 0; i < ordered.Length; i++)
		{
			IFunctionParameter parameter = function.Parameters[i];
			InterpreterValue value = values[parameter];

			ordered[i] = value;
		}

		try
		{
			InterpreterValue result = function.Execute(Context, ordered);
			return result;
		}
		catch (Exception exception) when (exception is not InterpreterException)
		{
			throw new InterpreterException(Node, exception);
		}
	}
	private InterpreterValue Evaluate(IAnnotatedFunctionDeclarationStatementSyntax function, IReadOnlyDictionary<IFunctionParameter, InterpreterValue> values)
	{
		bool needsReturnValue = function.Function.Return.Type.IsVoid is false;

		ValueScope oldValues = Values;
		Values = new(Values);
		try
		{
			foreach (KeyValuePair<IFunctionParameter, InterpreterValue> pair in values)
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
	#endregion
}
