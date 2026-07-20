using OwlDomain.Owl.Code.CodeAnalysis.Semantics;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols.Scopes;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;
using OwlDomain.Owl.Code.Execution.Builtins;

namespace OwlDomain.Owl.CLI.Actions;

public class BuiltinCommand : Command
{
	#region Constructors
	public BuiltinCommand() : base("builtins", "Shows information about the builtin symbols.")
	{
		SetAction(parsing =>
		{
			BuiltinResolutionResult builtinResult = BuiltinResolver.Resolve();

			TextFragmentLineCollection lines = [];

			for (int i = 0; i < builtinResult.Children.Count; i++)
			{
				if (i > 0)
					lines.AddLine();

				IBuiltinResolutionStageResult stage = builtinResult.Children[i];

				ISymbolScope scope = stage.ResultScope;
				lines.AddLine($"// {scope.Name} symbols").AddClassification(ClassificationKind.Comment);

				ISymbolGroup symbols = scope.GetAllNamed(includeParent: false);

				bool hadGroup = false;
				bool wasLastType = false;

				foreach (IGrouping<ClassificationKind?, ISymbol> byKind in symbols.GroupBy(s => s.Classification))
				{
					bool lastHadMultiple = false;

					foreach (IGrouping<string, ISymbol> byName in byKind.GroupBy(s => s.Name).OrderByDescending(g => g.Count()))
					{
						if (hadGroup && (lastHadMultiple || wasLastType))
							lines.AddLine();
						else
							hadGroup = true;

						foreach (ISymbol symbol in byName)
							Add(lines, symbol);

						lastHadMultiple = byName.Skip(1).Any();
						wasLastType = byKind.Key == ClassificationKind.Type;
					}

				}
			}

			Rows output = lines.TrimLines().Style(OwlStyling.Default);
			Panel panel = new Panel(new Padder(output)).Header("Builtin symbols");
			AnsiConsole.Write(panel);
			AnsiConsole.WriteLine();
		});
	}
	#endregion

	#region Helpers
	private static void Add(TextFragmentLineCollection lines, ISymbol symbol)
	{
		TextFragment indent = new("\t", ClassificationKind.Indentation);
		TextFragment space = new(" ", ClassificationKind.Whitespace);
		TextFragment colon = new(":", ClassificationKind.Punctuation);

		TextFragmentLine Get()
		{
			TextFragmentLine line = new(null);

			if (symbol is ITypeProperty property)
			{
				line.Add(indent);
				line.Add("property", ClassificationKind.Keyword);
				line.Add(space);
				line.Add(symbol.Name, ClassificationKind.TypeProperty);
				line.Add(colon);
				line.Add(space);
				line.AddRange(property.Type.GetDebugText());
			}
			else if (symbol is ITypeMethod method)
			{
				line.Add(indent);
				line.AddRange(method.Function.GetDebugText());
			}
			else
				line.AddRange(symbol.GetDebugText());

			return line;
		}

		if (symbol is INamedType type)
		{
			lines.AddLine(("type", ClassificationKind.Keyword), space, (type.Name, type.Classification));
			lines.AddLine(("{", ClassificationKind.Punctuation));

			bool hadGroup = false;
			foreach (ITypeProperty property in type.Members.OfType<ITypeProperty>())
			{
				Add(lines, property);
				hadGroup = true;
			}

			bool isFirst = true;
			bool lastHadMultiple = false;
			foreach (IGrouping<string?, ITypeMethod> byName in type.Members.OfType<ITypeMethod>().GroupBy(m => m.Name).OrderByDescending(g => g.Count()))
			{
				if (isFirst && hadGroup)
					lines.AddLine();
				else if (hadGroup && lastHadMultiple)
					lines.AddLine();
				else
					hadGroup = true;

				isFirst = false;

				foreach (ITypeMethod member in byName)
					Add(lines, member);

				lastHadMultiple = byName.Skip(1).Any();
			}

			isFirst = true;
			lastHadMultiple = false;
			foreach (IGrouping<OperatorKind, IBinaryOperator> byKind in type.BinaryOperators.GroupBy(b => b.Kind).OrderByDescending(g => g.Count()))
			{
				if (isFirst && hadGroup)
					lines.AddLine();
				else if (hadGroup && lastHadMultiple)
					lines.AddLine();
				else
					hadGroup = true;

				isFirst = false;

				foreach (IBinaryOperator binary in byKind)
				{
					lastHadMultiple = byKind.Skip(1).Any();
					TextFragmentLine binaryLine = GetBinary(binary);
					lines.Add(binaryLine);
				}
			}

			lines.AddLine(("}", ClassificationKind.Punctuation));
			return;
		}
		TextFragmentLine GetBinary(IBinaryOperator binary)
		{
			TextFragmentLine line = new(null)
			{
				{ indent },
				{ "operator", ClassificationKind.Keyword },
				{ TextFragment.Space },
			};

			line.AddRange(binary);

			return line;
		}

		TextFragmentLine line = Get();

		if (line.Any())
			lines.Add(line);
	}
	#endregion
}
