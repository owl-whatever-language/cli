namespace OwlDomain.ParsingTools.Semantics.Symbols;

/// <summary>
/// 	Represents the target of a symbol.
/// </summary>
public interface ISymbolTarget
{
	#region Properties
	/// <summary>The kind of the symbol target.</summary>
	/// <remarks>This value will likely be used for creating diagnostic messages.</remarks>
	string Kind { get; }

	/// <summary>Whether the target might still be mutated.</summary>
	bool IsMutable { get; }

	/// <summary>The symbol that should be used to reference this target.</summary>
	ISymbol Symbol { get; }
	#endregion
}

/// <summary>
/// 	Represents the target of a symbol.
/// </summary>
/// <typeparam name="TDeclaration">The type of the abstract syntax node that declared the symbol.</typeparam>
public interface ISymbolTarget<TDeclaration> : ISymbolTarget
	where TDeclaration : notnull, IAbstractSyntaxNode
{
	#region Properties
	/// <summary>The symbol that should be used to reference this target.</summary>
	new ISymbol<TDeclaration> Symbol { get; }
	ISymbol ISymbolTarget.Symbol => Symbol;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for the target of a symbol.
/// </summary>
/// <typeparam name="TDeclaration">The type of the abstract syntax node that declared the symbol.</typeparam>
public abstract class BaseSymbolTarget<TDeclaration> : ISymbolTarget<TDeclaration>
	where TDeclaration : notnull, IAbstractSyntaxNode
{
	#region Fields
	private ISymbol<TDeclaration>? _symbol;
	private bool _isMutable = true;
	#endregion

	#region Properties
	/// <inheritdoc/>
	public abstract string Kind { get; }

	/// <inheritdoc/>
	public bool IsMutable
	{
		get => _isMutable;
		set
		{
			if (value is true)
				ThrowHelper.ThrowArgumentException(nameof(value), "Making a symbol target mutable is not allowed.");

			if (value == _isMutable)
				return;

			ValidateImmutableState();
			_isMutable = false;
		}
	}

	/// <inheritdoc/>
	public ISymbol<TDeclaration> Symbol
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
	/// <summary>Marks the target as immutable.</summary>
	public void Lock() => IsMutable = false;

	/// <summary>Throws the <see cref="InvalidOperationException"/> if the target is no longer mutable.</summary>
	protected void ThrowIfImmutable()
	{
		if (IsMutable is false)
			ThrowHelper.ThrowInvalidOperationException();
	}

	/// <summary>Valid the state of the target to ensure it can be marked as immutable.</summary>
	/// <remarks>By default this will check that the symbol has been assigned.</remarks>
	protected virtual void ValidateImmutableState()
	{
		if (_symbol is null)
			ThrowHelper.ThrowInvalidOperationException("A symbol target can only be locked once a symbol has been set.");
	}

	/// <summary>Tries to set the given <paramref name="value"/> to the given <paramref name="field"/>.</summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="field">The field to store the <paramref name="value"/> in.</param>
	/// <param name="value">The value to set the <paramref name="field"/> to.</param>
	/// <param name="property">The name of the property that is being set.</param>
	/// <exception cref="InvalidOperationException">Thrown if the <paramref name="field"/> has already been set.</exception>
	protected void Set<T>(ref T? field, T? value, [CallerMemberName] string property = "<property>")
	{
		ThrowIfImmutable();

		if (field is not null)
			ThrowHelper.ThrowInvalidOperationException($"The {property} has already been set.");

		field = value;
	}
	#endregion
}

/// <summary>
/// 	Represents the base implementation for the target of a symbol.
/// </summary>
public abstract class BaseSymbolTarget : BaseSymbolTarget<IAbstractSyntaxNode> { }

/// <summary>
/// 	Contains various extensions related to the <see cref="ISymbolTarget"/>.
/// </summary>
public static class SymbolTargetExtensions
{
	extension<TTarget>(TTarget target) where TTarget : notnull, BaseSymbolTarget
	{
		#region Methods
		/// <summary>Sets the target's symbol to a symbol with only a name and no declaration.</summary>
		/// <param name="name">The name to give to the target.</param>
		/// <returns>The target that the method was called on.</returns>
		public TTarget WithSymbol(string name)
		{
			target.Symbol = new Symbol(name, target);

			return target;
		}

		/// <summary>Makes the symbol target immutable and returns it.</summary>
		/// <returns>The locked target.</returns>
		public TTarget Locked()
		{
			target.IsMutable = false;
			return target;
		}
		#endregion
	}

	extension<TDeclaration>(BaseSymbolTarget<TDeclaration> target) where TDeclaration : notnull, IAbstractSyntaxNode
	{
		/// <summary>Sets the target's symbol to a symbol with only a name and no declaration.</summary>
		/// <param name="name">The name to give to the target.</param>
		/// <param name="declaration">The node that declared the symbol.</param>
		/// <returns>The target that the method was called on.</returns>
		public BaseSymbolTarget<TDeclaration> WithSymbol(string? name, TDeclaration declaration)
		{
			target.Symbol = new Symbol<TDeclaration>(name, declaration, target);

			return target;
		}
	}
}
