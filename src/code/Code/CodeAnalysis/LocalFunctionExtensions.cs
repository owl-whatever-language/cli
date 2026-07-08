namespace OwlDomain.Owl.Code.CodeAnalysis;

public static class LocalFunctionExtensions
{
	extension(IConcreteFunctionDeclarationSignatureSyntax signature)
	{
		#region Properties
		public bool IsLocal => signature.Keyword?.IsFabricated is false;
		#endregion
	}

	extension(IConcreteFunctionDeclarationStatementSyntax function)
	{
		#region Properties
		public bool IsLocal => function.Signature.IsLocal;
		#endregion
	}

	extension(IDeclaredFunction function)
	{
		#region Properties
		public bool IsLocal => function.Declaration.IsLocal;
		#endregion
	}

	extension(IFunction function)
	{
		#region Methods
		public bool IsLocal<T>([NotNullWhen(true)] out T? declaration) where T : notnull, IConcreteFunctionDeclarationStatementSyntax
		{
			if (function is IDeclaredFunction declared && declared.Declaration.IsLocal)
			{
				declaration = (T)declared.Declaration;
				return true;
			}

			declaration = default;
			return false;
		}
		#endregion
	}
}
