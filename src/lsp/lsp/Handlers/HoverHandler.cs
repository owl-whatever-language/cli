using System.CodeDom.Compiler;
using System.IO;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Hover;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Loops;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated.FunctionBodies;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated.Nodes;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated.Statements;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Declared.Nodes;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Declared.Statements;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.Expressions;

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

		ISyntaxToken? token = tree.Document.Search<ISyntaxToken>(request.Position);
		if (token is null)
			return Task.FromResult<HoverResponse?>(null);

		ISymbol? symbol = token.Symbol;

		if (symbol is null && token.Parent is ISemanticBinaryExpressionSyntax binary && binary.Operator == token)
			symbol = binary.Operation?.AsFunction;
		else if (symbol is null && token.Parent is IDeclaredFunctionDeclarationSignatureSyntax signature && signature.Keyword == token)
			symbol = ((IDeclaredFunctionDeclarationStatementSyntax?)signature.Parent)?.Function;

		using (StringWriter stringWriter = new())
		using (IndentedTextWriter writer = new(stringWriter, "  "))
		{
			WriteHover(writer, token, symbol);

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
	private void WriteHover(IndentedTextWriter writer, ISyntaxToken token, ISymbol? symbol)
	{
		if (symbol is not null)
			WriteDeclaration(writer, symbol);

		if (token is IAnnotatedSyntaxNode node)
			WriteAnnotations(writer, node);
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
	}
	private void TryWriteScopeDeclaration(IndentedTextWriter writer, IAnnotatedSyntaxNode node)
	{
		IReadOnlyCollection<ISymbol> symbols = GetDeclaredSymbols(node);
		if (symbols.Count is 0)
			return;

		writer.WriteLine("### Scope declaration");

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

		if (node is IAnnotatedToken token)
		{
			if (token.IsDeclarationName() && token.Symbol is IDeclaredSymbol declared && declared is IAnnotatedSyntaxNode typed)
				TryAddScope(typed);

			if (token.Parent is IAnnotatedWhileStatementSyntax @while && token == @while.Keyword)
				TryAddScope(@while.Body);

			else if (token.Parent is IAnnotatedFunctionDeclarationSignatureSyntax signature && (token == signature.Keyword || token == signature.Name))
			{
				var function = (IAnnotatedFunctionDeclarationStatementSyntax?)signature.Parent;
				Debug.Assert(function is not null);

				TryAddScope(function);

				if (function.Body is IAnnotatedBlockFunctionBodySyntax body)
					TryAddScope(body.Block);
				else
					TryAddScope(function.Body);
			}
			else if (token.Parent is IAnnotatedIfStatementSyntax @if && @if.Keyword == token)
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
	#endregion
}
