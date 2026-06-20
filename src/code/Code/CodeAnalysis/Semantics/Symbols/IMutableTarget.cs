namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public interface IMutableTarget
{
	#region Properties
	/// <summary>Whether the target might still be mutated.</summary>
	bool IsMutable { get; }
	#endregion
}

public abstract class BaseMutableTarget : IMutableTarget
{
	#region Fields
	private bool _isMutable = true;
	#endregion

	#region Properties
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
	#endregion


	#region Methods
	/// <summary>Throws the <see cref="InvalidOperationException"/> if the target is no longer mutable.</summary>
	protected void ThrowIfImmutable()
	{
		if (IsMutable is false)
			ThrowHelper.ThrowInvalidOperationException();
	}

	/// <summary>Valid the state of the target to ensure it can be marked as immutable.</summary>
	protected virtual void ValidateImmutableState() { }

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

public static class IMutableTargetExtensions
{
	extension<T>(T target) where T : notnull, BaseMutableTarget
	{
		#region Methods
		/// <summary>Makes the symbol target immutable and returns it.</summary>
		/// <returns>The locked target.</returns>
		public T Lock()
		{
			target.IsMutable = false;
			return target;
		}
		#endregion
	}
}