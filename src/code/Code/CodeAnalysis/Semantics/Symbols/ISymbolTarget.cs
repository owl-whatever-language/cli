namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public interface ISymbolTarget : IMutableTarget
{
	#region Properties
	/// <summary>The kind of the symbol target.</summary>
	/// <remarks>This value will likely be used for creating diagnostic messages.</remarks>
	string Kind { get; }

	/// <summary>The symbol that should be used to reference this target.</summary>
	ISymbol Symbol { get; set; }
	#endregion

	#region Methods
	string ToString();
	#endregion
}

public interface INamedSymbolTarget : ISymbolTarget
{
	#region Properties
	/// <summary>The name of the target.</summary>
	string? Name { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public abstract class BaseSymbolTarget : BaseMutableTarget, ISymbolTarget
{
	#region Fields
	private ISymbol? _symbol;
	#endregion

	#region Properties
	/// <inheritdoc/>
	public abstract string Kind { get; }

	/// <inheritdoc/>
	public ISymbol Symbol
	{
		get
		{
			if (_symbol is null)
				ThrowHelper.ThrowInvalidOperationException("The symbol has not been set yet.");

			return _symbol;
		}

		set => Set(ref _symbol, value);
	}
	#endregion

	#region Methods
	protected override void ValidateImmutableState()
	{
		base.ValidateImmutableState();

		if (_symbol is null)
			ThrowHelper.ThrowInvalidOperationException("A symbol target can only be locked once a symbol has been set.");
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Symbol({Kind})";
	public override string ToString() => base.ToString() ?? Kind;
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public abstract class BaseNamedSymbolTarget : BaseSymbolTarget, INamedSymbolTarget
{
	#region Properties
	public string? Name { get; }
	#endregion

	#region Constructors
	public BaseNamedSymbolTarget(string? name) => Name = name;
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Symbol({Kind}) {{ Name = ({Name}) }}";
	#endregion
}

public static class SymbolTargetExtensions
{
	extension<TTarget>(TTarget target) where TTarget : notnull, ISymbolTarget
	{
		#region Methods
		public TTarget WithSymbol(string name)
		{
			target.Symbol = new Symbol(name, target);

			return target;
		}

		public TTarget WithSymbol(string? name, IConcreteSyntaxNode declaration)
		{
			target.Symbol = new DeclaredSymbol(name, target, declaration);

			return target;
		}
		#endregion
	}
	extension<TTarget>(TTarget target) where TTarget : notnull, INamedSymbolTarget
	{
		#region Methods
		public TTarget WithSymbol(IConcreteSyntaxNode declaration)
		{
			target.Symbol = new DeclaredSymbol(target.Name, target, declaration);

			return target;
		}
		#endregion
	}
}
