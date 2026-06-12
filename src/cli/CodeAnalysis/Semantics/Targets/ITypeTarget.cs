namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Targets;

public interface ITypeTarget : ISymbolTarget
{
	#region Properties
	ITypeInfo? Type { get; }
	#endregion
}

public static class ITypeTargetExtensions
{
	extension(ISymbolTarget? target)
	{
		#region Properties
		public ITypeInfo? Type => target is ITypeTarget typed ? typed.Type : null;
		public ITypeInfo? ReturnType
		{
			get
			{
				return target switch
				{
					IFunctionInfo function => function.Signature?.Return.Type,
					FunctionType function => function.Function.Signature?.Return.Type,

					_ => null,
				};
			}
		}
		#endregion
	}

	extension(ISymbol? symbol)
	{
		#region Properties
		public ITypeInfo? Type => symbol?.Target.Type;
		public ITypeInfo? ReturnType => symbol?.Target.ReturnType;
		#endregion
	}

	extension(ISemanticSyntaxToken? token)
	{
		#region Properties
		public ITypeInfo? Type => token?.Symbol.Type;
		public ITypeInfo? ReturnType => token?.Symbol.ReturnType;
		#endregion
	}
}
