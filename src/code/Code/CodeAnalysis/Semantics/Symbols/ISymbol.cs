namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public interface ISymbol
{
	#region Properties
	string? Name { get; }
	ISymbolTarget Target { get; }
	#endregion
}

public interface IDeclaredSymbol : ISymbol
{
	#region Properties
	IConcreteSyntaxNode Declaration { get; set; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class Symbol : ISymbol
{
	#region Properties
	/// <inheritdoc/>
	public string Name { get; }

	/// <inheritdoc/>
	public ISymbolTarget Target { get; }
	#endregion

	#region Constructors
	public Symbol(string name, ISymbolTarget target)
	{
		Name = name;
		Target = target;
	}
	#endregion

	#region Helpers
	public override string ToString() => Target.ToString() ?? Name;
	private string DebuggerDisplay() => $"Symbol({Name}) -> {Target.Kind}";
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class DeclaredSymbol : IDeclaredSymbol
{
	#region Properties
	/// <inheritdoc/>
	public string? Name { get; }

	/// <inheritdoc/>
	public ISymbolTarget Target { get; }

	/// <inheritdoc/>
	public IConcreteSyntaxNode Declaration
	{
		get;
		set
		{
			if (field is not null) // Note(Nightowl): First set from the constructor;
			{
				if (value.NodeKind.WithGroup != field.NodeKind.WithGroup)
					ThrowHelper.ThrowArgumentException(nameof(value), "The symbol declaration can only be shadowed by a node of the same kind.");

				if (value.Level <= field.Level)
					ThrowHelper.ThrowArgumentException(nameof(value), "The symbol declaration can only be shadowed by a node with a higher level.");
			}

			field = value;
		}
	}
	#endregion

	#region Constructors
	public DeclaredSymbol(string? name, ISymbolTarget target, IConcreteSyntaxNode declaration)
	{
		Name = name;
		Target = target;
		Declaration = declaration;
	}
	#endregion

	#region Helpers
	public override string ToString() => Target.ToString() ?? Name ?? "???";
	private string DebuggerDisplay() => $"Symbol({Name}) -> {Target.Kind}";
	#endregion
}
