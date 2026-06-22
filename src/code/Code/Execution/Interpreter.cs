using System.Text;

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

public sealed class Interpreter : IDiagnosticProvider
{
	#region Nested types
	private sealed class RuntimeException : Exception
	{
		#region Properties
		public IFinalSyntaxNode? Node { get; }
		#endregion

		#region Constructors
		public RuntimeException(IFinalSyntaxNode? node = null)
		{
			Node = node;
		}
		#endregion
	}
	private sealed class FunctionReturnException : Exception
	{
		#region Properties
		public object? Value { get; }
		#endregion

		#region Constructors
		public FunctionReturnException(object? value) => Value = value;
		#endregion
	}
	private sealed class VariableStore
	{
		#region Properties
		public VariableStore? Parent { get; }
		public Dictionary<INamedSymbolTarget, object?> Values { get; } = [];
		#endregion

		#region Constructors
		public VariableStore(VariableStore? parent = null)
		{
			Parent = parent;
		}
		#endregion

		#region Methods
		public bool TryGetValue(INamedSymbolTarget variable, out object? value)
		{
			if (Values.TryGetValue(variable, out value))
				return true;

			if (Parent is not null)
				return Parent.TryGetValue(variable, out value);

			value = default;
			return false;
		}
		#endregion
	}
	private readonly struct VariableScope(Interpreter interpreter) : IDisposable
	{
		#region Methods
		public void Dispose() => interpreter.ExitVariableScope();
		#endregion
	}
	#endregion

	#region Properties
	public string Name => "interpreter";
	private DiagnosticBag Diagnostics { get; } = [];
	private VariableStore Variables { get; set; } = new();
	private ISourceFile? CurrentSource { get; set; }
	#endregion

	#region Constructors
	private Interpreter() { }
	#endregion

	#region Functions
	public static InterpretingResult Interpret(IFinalSyntaxTree tree)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			Interpreter interpreter = new();
			interpreter.InterpretCore(tree);

			return new(interpreter.Diagnostics, performance);
		}
	}
	#endregion

	#region Methods
	private void InterpretCore(IFinalSyntaxTree tree)
	{
		CurrentSource = tree.Source;
		try
		{
			Interpret(tree.Document);
		}
		catch (RuntimeException exception)
		{
			if (exception.Node is not null)
				Console.Error.WriteLine($"Runtime error occurred on {exception.Node.Position}, check the diagnostics for more details.");

			// Todo(Nightowl): Print some stack trace information or something;
		}
	}
	private void Interpret(IFinalDocumentSyntax document) => Interpret(document.Statements);
	#endregion

	#region Statement methods
	private void Interpret(IReadOnlyList<IFinalStatementSyntax> statements)
	{
		foreach (IFinalStatementSyntax statement in statements)
			Interpret(statement);
	}
	private void Interpret(IFinalStatementSyntax statement)
	{
		switch (statement)
		{
			case IFinalLocalFunctionDeclarationStatementSyntax:
			case IFinalFunctionDeclarationStatementSyntax:
				return; // Note(Nightowl): Don't interpret function declarations;

			case IFinalExpressionStatementSyntax expression:
				_ = Evaluate(expression.Expression);
				return;

			case IFinalVariableDeclarationStatementSyntax declaration:
				Interpret(declaration);
				return;

			case IFinalBlockStatementSyntax block:
				Interpret(block.Statements);
				return;

			default:
				ThrowHelper.ThrowNotSupportedException($"The statement '{statement.GetType().Name}' is not supported by the interpreter, this is likely a bug.");
				return;
		}
	}
	private void Interpret(IFinalVariableDeclarationStatementSyntax declaration)
	{
		object? value = Evaluate(declaration.Value);

		if (Variables.Values.TryAdd(declaration.Variable, value) is false)
		{
			AddError("duplicate_variable", declaration.Name.Position, $"The '{declaration.Variable.Name}' variable has already been set.");
			throw new RuntimeException(declaration);
		}
	}
	#endregion

	#region Evaluate methods
	private object? Evaluate(IFinalExpressionSyntax expression)
	{
		return expression switch
		{
			IFinalStringLiteralExpressionSyntax str => Evaluate(str),
			IFinalInterpolatedStringExpressionSyntax str => Evaluate(str),
			IFinalGetExpressionSyntax get => Evaluate(get),
			IFinalFunctionCallExpressionSyntax call => Evaluate(call),

			_ => ThrowHelper.ThrowNotSupportedException<object>($"The expression '{expression.GetType().Name}' is not supported by the interpreter, this is likely a bug.")
		};
	}
	private object? Evaluate(IFinalStringLiteralExpressionSyntax expression) => expression.Value;
	private object? Evaluate(IFinalInterpolatedStringExpressionSyntax expression)
	{
		StringBuilder builder = new();

		foreach (IFinalStringFragmentSyntax fragment in expression.Fragments)
		{
			object? value = Evaluate(fragment);
			string? str = value is string s ? s : value?.ToString();

			builder.Append(str);
		}

		return builder.ToString();
	}
	private object? Evaluate(IFinalStringFragmentSyntax fragment)
	{
		return fragment switch
		{
			IFinalRegularStringFragmentSyntax regular => regular.Text.Value,
			IFinalEscapedStringFragmentSyntax escaped => escaped.Sequence.Value,
			IFinalEscapedHexStringFragmentSyntax escaped => escaped.Sequence.Value,
			IFinalInterpolatedStringFragmentSyntax interpolated => Evaluate(interpolated.Value),

			_ => ThrowHelper.ThrowNotSupportedException<object>($"The string fragment '{fragment.GetType().Name}' is not supported by the interpreter, this is likely a bug.")
		};
	}
	private object? Evaluate(IFinalGetExpressionSyntax expression)
	{
		INamedSymbolTarget? lookup = expression.Symbol?.Target switch
		{
			ILocalVariable variable => variable,
			IFunctionParameter parameter => parameter,

			_ => null,
		};

		if (lookup is not null)
		{
			if (Variables.TryGetValue(lookup, out object? value))
				return value;

			AddError("variable_not_defined", expression.Position, $"The value '{lookup.Name}' has not been defined yet.");
			throw new RuntimeException(expression);
		}

		return expression.Symbol?.Target;
	}
	private object? Evaluate(IFinalFunctionCallExpressionSyntax expression)
	{
		object? value = Evaluate(expression.Expression);
		if (value is IFunction function)
			value = function.Callable;

		if (value is not ICallable callable)
		{
			AddError("not_callable", expression.Expression.Position, $"The value ({value?.GetType().Name}) '{value}' cannot be called.");
			throw new RuntimeException(expression.Start);
		}

		Debug.Assert(callable.Function is not null);

		object?[] arguments = new object?[callable.Parameters.Count];
		for (int i = 0; i < expression.Arguments.Values.Count; i++)
		{
			IFinalFunctionArgumentSyntax argument = expression.Arguments.Values[i];

			if (argument is IFinalRegularFunctionArgumentSyntax regular)
			{
				arguments[i] = Evaluate(regular.Value);
				continue;
			}

			if (argument is IFinalNamedFunctionArgumentSyntax named)
			{
				string? name = named.Name.Value as string;
				Debug.Assert(name is not null);

				int index = callable.Parameters.First(p => p.Name == name).Index;
				arguments[index] = Evaluate(named.Value);

				continue;
			}

			ThrowHelper.ThrowNotSupportedException($"The function argument '{argument.GetType().Name}' is not supported by the interpreter, this is likely a bug.");
		}

		object? result = Evaluate(callable, arguments);
		return result;
	}
	private object? Evaluate(ICallable callable, IReadOnlyList<object?> arguments)
	{
		if (callable.Function == SpecialFunctions.Print)
		{
			string? text = (string?)arguments[0];
			Debug.Assert(text is not null);
			Console.WriteLine(text);

			return null;
		}

		if (callable.Function?.Symbol is IDeclaredSymbol declared && declared.Declaration is IFinalFunctionDeclarationStatementSyntax declaration)
			return Evaluate(declaration, arguments);

		ThrowHelper.ThrowNotSupportedException($"The callable '{callable.GetType().Name}' is not supported by the interpreter, this is likely a bug.");
		return default;
	}

	private object? Evaluate(IFinalFunctionDeclarationStatementSyntax declaration, IReadOnlyList<object?> arguments)
	{
		Debug.Assert(arguments.Count == declaration.Parameters.Values.Count);

		using (Scope())
		{
			for (int i = 0; i < arguments.Count; i++)
			{
				IFunctionParameter parameter = declaration.Parameters.Values[i].Parameter;
				Variables.Values.Add(parameter, arguments[i]);
			}

			try
			{
				Interpret(declaration.Body);
			}
			catch (FunctionReturnException @return)
			{
				return @return.Value;
			}

			if (declaration.Return is not IFinalEmptyFunctionReturnSyntax)
			{
				AddError("expected_return", declaration.Name.Position, "The function declared a return type but it did not return a value.");
				throw new RuntimeException(declaration);
			}

			return null;
		}
	}
	private void Interpret(IFinalFunctionBodySyntax body)
	{
		switch (body)
		{
			case IFinalEmptyFunctionBodySyntax:
				return;

			case IFinalShortFunctionBodySyntax @short:
				object? value = Evaluate(@short.Expression);
				throw new FunctionReturnException(value);

			case IFinalBlockFunctionBodySyntax block:
				Interpret(block.Block);
				return;

			default:
				ThrowHelper.ThrowNotSupportedException($"The function body '{body.GetType().Name}' is not supported by the interpreter, this is likely a bug.");
				return;
		}
	}
	#endregion

	#region Variable methods
	private VariableScope Scope()
	{
		NewVariableScope();
		return new(this);
	}
	private void NewVariableScope()
	{
		Variables = new(Variables);
	}
	private void ExitVariableScope()
	{
		if (Variables.Parent is null)
			ThrowHelper.ThrowInvalidOperationException("Cannot exit the root variable scope.");

		Variables = Variables.Parent;
	}
	#endregion

	#region Diagnostic helpers
	private void AddError(string id, IndexedPositionRange position, string message, StackTrace? stackTrace = null)
	{
		AddDiagnostic(DiagnosticKind.Error, id, position, message, stackTrace);
	}
	private void AddDiagnostic(DiagnosticKind kind, string id, IndexedPositionRange position, string message, StackTrace? stackTrace = null)
	{
		Debug.Assert(CurrentSource is not null);

		Diagnostics.Add(new Diagnostic()
		{
			Provider = this,
			Kind = kind,
			Id = id,
			StackTrace = stackTrace,

			Location = new DiagnosticSourceLocation(CurrentSource, position),
			Message = message
		});
	}
	#endregion
}
