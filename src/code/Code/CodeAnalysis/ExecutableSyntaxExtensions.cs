namespace OwlDomain.Owl.Code.CodeAnalysis;

public static class ExecutableSyntaxExtensions
{
	extension(SyntaxNodeEnum syntax)
	{
		#region Properties
		public bool IsExecutable
		{
			get
			{
				switch (syntax)
				{
					case SyntaxNodeEnum.IfStatement:
					case SyntaxNodeEnum.IfElseStatement:
					case SyntaxNodeEnum.WhileStatement:
					case SyntaxNodeEnum.BlockStatement:
					case SyntaxNodeEnum.ExpressionStatement:
					case SyntaxNodeEnum.VariableDeclarationStatement:
					case SyntaxNodeEnum.ReturnStatement:
					case SyntaxNodeEnum.ValueReturnStatement:
						return true;

					default:
						return false;
				}
			}
		}
		#endregion
	}

	extension(IConcreteStatementSyntax statement)
	{
		#region Properties
		public bool IsExecutable => statement.NodeEnum.IsExecutable;
		#endregion
	}

	extension(IConcreteDocumentSyntax document)
	{
		#region Properties
		public bool HasExecutableStatements => document.Statements.Any(get_IsExecutable);
		public bool IsExecutable => document.Statements.Count is 0 || document.HasExecutableStatements;
		#endregion
	}

	extension(IConcreteSyntaxTree tree)
	{
		#region Properties
		public bool HasExecutableStatements => tree.Document.HasExecutableStatements;
		public bool IsExecutable => tree.Document.IsExecutable;
		#endregion
	}
}
