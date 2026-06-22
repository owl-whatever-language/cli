namespace OwlDomain.Owl.CLI;

using Segment = AnsiMarkupSegment;
using Segments = IReadOnlyList<AnsiMarkupSegment>;

public class DebugTreePrinter : DefaultDebugTreePrinter
{
	#region Constructors
	public DebugTreePrinter(IClassificationStyles styles) : base(styles)
	{
	}
	#endregion

	#region Functions
	public static void Print(IDebugTree tree, IClassificationStyles? styles = null)
	{
		Tree styled = Convert(tree, styles);
		AnsiConsole.Write(styled);
	}
	public static Tree Convert(IDebugTree tree, IClassificationStyles? styles = null)
	{
		styles ??= OwlClassificationStyles.Instance;
		DefaultDebugTreePrinter printer = new DebugTreePrinter(styles);

		return printer.Convert(tree);
	}

	public static void Print(IDebugTreeObject obj, IClassificationStyles? styles = null)
	{
		Tree styled = Convert(obj, styles);
		AnsiConsole.Write(styled);
	}
	public static Tree Convert(IDebugTreeObject obj, IClassificationStyles? styles = null)
	{
		styles ??= OwlClassificationStyles.Instance;
		DefaultDebugTreePrinter printer = new DebugTreePrinter(styles);

		return printer.Convert(obj);
	}
	#endregion

	#region Methods
	protected override Segments ConvertSimple(object? value)
	{
		if (value is ISymbol symbol)
			return Convert(symbol);

		if (value is ISymbolTarget target)
			return Convert(target);

		if (value is ITypeInfo type)
			return Convert(type);

		return base.ConvertSimple(value);
	}
	#endregion

	#region Symbol methods
	[return: NotNullIfNotNull(nameof(symbol))]
	protected Segments? Convert(ISymbol symbol) => Convert(symbol.Target);

	[return: NotNullIfNotNull(nameof(target))]
	protected Segments? Convert(ISymbolTarget? target)
	{
		return target switch
		{
			IFunction function => Convert(function),
			ITypeInfo type => Convert(type),
			ILocalVariable variable => Convert(variable),
			IFunctionParameter parameter => Convert(parameter),
			ICallableParameter parameter => parameter.Parameter is null ? [New("???", ClassificationKind.Type)] : Convert(parameter.Parameter),

			ISymbolTarget => [New(target.GetType().Name)],
			_ => null,
		};
	}

	[return: NotNullIfNotNull(nameof(type))]
	protected Segments? Convert(ITypeInfo? type)
	{
		return type switch
		{
			INamedTypeInfo named => Convert(named),
			ICallable callable => Convert(callable),

			ITypeInfo => [New(type.GetType().Name)],
			_ => null
		};
	}
	protected Segments Convert(INamedTypeInfo type) => [New(type.Name ?? "???", ClassificationKind.Type)];
	protected Segments Convert(ILocalVariable variable)
	{
		List<Segment> segments = [];

		if (variable.Type is not null)
			segments.AddRange(Convert(variable.Type));

		segments.Add(New(" " + (variable.Name ?? "???"), ClassificationKind.Variable));

		return segments;
	}
	protected Segments Convert(IFunctionParameter parameter)
	{
		List<Segment> segments = [];

		if (parameter.Type is not null)
			segments.AddRange(Convert(parameter.Type));

		segments.Add(New(" " + (parameter.Name ?? "???"), ClassificationKind.Parameter));

		return segments;
	}
	protected Segments Convert(IFunction function)
	{
		if (function.Callable is null)
			return [New(function.Name ?? "???", ClassificationKind.Function)];

		ICallable callable = function.Callable;

		List<Segment> segments = [];

		if (callable.Function?.Name is not null)
			segments.Add(New(callable.Function.Name, ClassificationKind.Function));

		segments.Add(New("(", ClassificationKind.Punctuation));

		for (int i = 0; i < callable.Parameters.Count; i++)
		{
			if (i > 0)
				segments.Add(New(", ", ClassificationKind.Punctuation));

			ICallableParameter parameter = callable.Parameters[i];

			if (parameter.Type is null)
				segments.Add(New("???", ClassificationKind.Type));
			else
				segments.AddRange(Convert(parameter.Type));

			if (parameter.Name is not null)
				segments.Add(New(" " + parameter.Name, ClassificationKind.Parameter));
		}

		segments.Add(New(")", ClassificationKind.Punctuation));

		if (callable.Return is not null && callable.Return.Type != SpecialTypes.Void)
		{
			segments.Add(New(": ", ClassificationKind.Punctuation));
			if (callable.Return.Type is null)
				segments.Add(New("???", ClassificationKind.Type));
			else
				segments.AddRange(Convert(callable.Return.Type));
		}

		return segments;
	}
	protected Segments Convert(ICallable callable)
	{
		List<Segment> segments = [];

		segments.Add(New("callable", ClassificationKind.Type));
		segments.Add(New("(", ClassificationKind.Punctuation));

		for (int i = 0; i < callable.Parameters.Count; i++)
		{
			if (i > 0)
				segments.Add(New(", ", ClassificationKind.Punctuation));

			ICallableParameter parameter = callable.Parameters[i];

			if (parameter.Type is null)
				segments.Add(New("???", ClassificationKind.Type));
			else
				segments.AddRange(Convert(parameter.Type));
		}

		segments.Add(New(")", ClassificationKind.Punctuation));

		if (callable.Return is not null && callable.Return.Type != SpecialTypes.Void)
		{
			segments.Add(New(": ", ClassificationKind.Punctuation));
			if (callable.Return.Type is null)
				segments.Add(New("???", ClassificationKind.Type));
			else
				segments.AddRange(Convert(callable.Return.Type));
		}

		return segments;
	}
	#endregion
}
