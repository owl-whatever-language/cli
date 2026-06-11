using System.Text;

namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Functions;

public class FunctionSignature
{
	#region Properties
	public string? Name { get; }
	public IReadOnlyList<FunctionParameterSignature> Parameters { get; }
	public FunctionReturnSignature Return { get; }
	#endregion

	#region Constructors
	public FunctionSignature(string? name, IReadOnlyList<FunctionParameterSignature> parameters, FunctionReturnSignature @return)
	{
		Name = name;
		Parameters = parameters;
		Return = @return;
	}
	public FunctionSignature(string? name, params IReadOnlyList<FunctionParameterSignature> parameters)
	{
		Name = name;
		Parameters = parameters;
		Return = new(SpecialTypes.Void);
	}
	#endregion

	#region Methods
	public override string ToString()
	{
		StringBuilder builder = new();
		builder
			.Append(Name)
			.Append('(');

		for (int i = 0; i < Parameters.Count; i++)
		{
			if (i > 0)
				builder.Append(", ");

			builder.Append(Parameters[i]);
		}

		builder.Append(')');

		if (Return.Type != SpecialTypes.Void)
		{
			builder
			.Append(": ")
			.Append(Return);
		}

		return builder.ToString();
	}
	#endregion
}
