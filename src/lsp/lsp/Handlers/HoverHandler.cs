using System.CodeDom.Compiler;
using System.IO;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Hover;
using OwlDomain.Owl.Code.CodeAnalysis.Passes.LocalCapture;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Loops;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated.FunctionBodies;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated.Statements;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.FunctionBodies;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Nodes;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Statements;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Declared;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class HoverHandler(ILspContext context) : HoverHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.HoverProvider = true;
	}
	protected override Task<HoverResponse?> Handle(HoverParams request, CancellationToken cancellation)
	{
		if (_context.TryGetTree(request.TextDocument, out ICodeSyntaxTree? tree) is false)
			return Task.FromResult<HoverResponse?>(null);

		ISyntaxNode? target = tree.Document.Search<ISyntaxToken>(request.Position);
		if (target is null)
			return Task.FromResult<HoverResponse?>(null);


		target = CorrectTarget(target);


		using (StringWriter stringWriter = new())
		using (IndentedTextWriter writer = new(stringWriter, "  "))
		{
			WriteHover(writer, target);

			string output = stringWriter.ToString();
			if (string.IsNullOrWhiteSpace(output) is false)
			{
				return Task.FromResult<HoverResponse?>(new()
				{
					Contents = new()
					{
						Kind = MarkupKind.Markdown,
						Value = output
					}
				});
			}
		}

		return Task.FromResult<HoverResponse?>(null);
	}
	#endregion

	#region Helpers
	private ISyntaxNode CorrectTarget(ISyntaxNode target)
	{
		if (target is ISyntaxToken token)
		{
			if (token.Parent is IConcreteWhileStatementSyntax)
				return token.Parent;

			if (token.Parent is IConcreteVariableDeclarationStatementSyntax)
				return token.Parent;

			if (token.Parent is IConcreteFunctionDeclarationSignatureSyntax signature && signature.Parent is not null)
				return signature.Parent;

			if (token.Parent is IConcreteFunctionBodySyntax body && body.Parent is not null)
				return body.Parent;
		}

		return target;
	}
	private void WriteHover(IndentedTextWriter writer, ISyntaxNode target)
	{
		if (target.TryGetDeclaredSymbol(out IDeclaredSymbol? symbol))
			WriteDeclaration(writer, symbol);
		else if (target is IDeclaredToken token)
		{
			if (token.Symbol is IDeclaredSymbol declared)
				WriteDeclaration(writer, declared);
			else if (token.Symbol is not null)
				WriteDeclaration(writer, token.Symbol);
		}

		if (target is IAnnotatedSyntaxNode node)
			WriteAnnotations(writer, node);
	}

	private void WriteDeclaration(IndentedTextWriter writer, IDeclaredSymbol symbol)
	{
		if (symbol.Declaration is IAnnotatedFunctionDeclarationStatementSyntax function && function.Signature.Keyword is not null)
		{
			writer.WriteLine($"## Declaration (function)");

			writer.WriteLine("```owl");
			writer.WriteLine($"fun {symbol.GetDebugText().ToPlainText()}");
			writer.WriteLine("```");
			writer.WriteLine();

			return;
		}

		WriteDeclaration(writer, (ISymbol)symbol);
	}
	private void WriteDeclaration(IndentedTextWriter writer, ISymbol symbol)
	{
		string? kind = symbol switch
		{
			ILocalVariable => "variable",
			IFunction => "function",
			IFunctionParameter => "parameter",
			ITypeProperty => "property",
			ITypeMethod => "method",
			IType => "type",
			ILoopLabel => "loop label",

			_ => null,
		};

		if (kind is not null)
			writer.WriteLine($"## Declaration ({kind})");
		else
			writer.WriteLine($"## Declaration");

		writer.WriteLine("```owl");
		writer.WriteLine(symbol.GetDebugText().ToPlainText());
		writer.WriteLine("```");
		writer.WriteLine();
	}
	private void WriteAnnotations(IndentedTextWriter writer, IAnnotatedSyntaxNode node)
	{
		TryWriteScopeDeclaration(writer, node);
		TryWriteLocalCapture(writer, node);
	}

	private void TryWriteScopeDeclaration(IndentedTextWriter writer, IAnnotatedSyntaxNode node)
	{
		IReadOnlyCollection<ISymbol> symbols = GetDeclaredSymbols(node);
		if (symbols.Count is 0)
			return;

		writer.WriteLine("### Declares");

		foreach (ISymbol symbol in symbols)
		{
			Debug.Assert(symbol is not null);
			writer.Write($"`{symbol.Name}` ");
		}

		writer.WriteLine();
	}
	private IReadOnlyCollection<ISymbol> GetDeclaredSymbols(IAnnotatedSyntaxNode node)
	{
		HashSet<ISymbol> symbols = [];

		void Add(ISymbol symbol)
		{
			if (symbol.Name is not null)
				symbols.Add(symbol);
		}
		void AddRange(IEnumerable<ISymbol> range)
		{
			foreach (ISymbol symbol in range)
				Add(symbol);
		}
		void TryAddScope(IAnnotatedSyntaxNode node)
		{
			if (node.TryGetDeclaredScope(out IDeclaredSymbolScope? scope))
				AddRange(scope.GetNamed(includeParents: false).FromCurrent);
		}

		TryAddScope(node);

		if (node is IAnnotatedWhileStatementSyntax @while)
			TryAddScope(@while.Body);
		else if (node is IAnnotatedFunctionDeclarationStatementSyntax function)
		{
			if (function.Body is IAnnotatedBlockFunctionBodySyntax body)
				TryAddScope(body.Block);
			else
				TryAddScope(function.Body);
		}

		if (node is IAnnotatedToken token)
		{
			if (token.Parent is IAnnotatedIfStatementSyntax @if && @if.Keyword == token)
				TryAddScope(@if.TrueClause);
			else if (token.Parent is IAnnotatedIfElseStatementSyntax @ifElse)
			{
				if (@ifElse.Keyword == token)
					TryAddScope(@ifElse.TrueClause);
				else if (@ifElse.Else == token)
					TryAddScope(ifElse.FalseClause);
			}
		}

		return symbols;
	}

	private void TryWriteLocalCapture(IndentedTextWriter writer, IAnnotatedSyntaxNode node)
	{
		if (node is not IAnnotatedFunctionDeclarationStatementSyntax function)
			return;

		LocalCaptureAnnotation capture = function.GetLocalCapture();
		if (capture.Variables.Any(v => v.Variable.Name is not null) is false)
			return;

		writer.WriteLine("### Captures");

		foreach (IUsedVariableInfo usage in capture.Variables)
		{
			if (usage.Variable.Name is null)
				continue;

			writer.Write($"`{usage.Variable.Name}` ");
		}

		writer.WriteLine();
		writer.WriteLine();
	}
	#endregion
}
