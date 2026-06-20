using System.CodeDom.Compiler;
using System.IO;

namespace OwlDomain.ParsingTools.Generator.Syntax;

[Generator(LanguageNames.CSharp)]
public class SyntaxNodeGenerator : IIncrementalGenerator
{
	#region Methods
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValuesProvider<AdditionalText> syntaxFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".syntax"));

		context.RegisterSourceOutput(syntaxFiles, Generate);
	}
	#endregion

	#region Functions
	private static void Generate(SourceProductionContext context, AdditionalText syntaxFile)
	{
		SourceText? text = syntaxFile.GetText();
		if (text is null)
			return;

		SyntaxDescriptionFile? description = SyntaxDescriptionFile.Parse(text);
		if (description is null)
			return;

		try
		{
			Generate(context, description);
		}
		catch (Exception exception)
		{
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
			File.WriteAllText("/mnt/data/projects/owldomain/projects/owl/cli/error.txt", exception.StackTrace.ToString());
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
			throw;
		}
	}
	private static void Generate(SourceProductionContext context, SyntaxDescriptionFile description)
	{
		IReadOnlyList<StructuredTreeInfo> order = StructuredTreeInfo.Convert(description);

		GenerateGlobal(context, order.Last());
		foreach (StructuredTreeInfo info in order)
		{
			GenerateToken(context, info.Token);
			GenerateBaseNode(context, info.BaseNode);
			GenerateTree(context, info);

			foreach (StructuredGroupInfo group in info.Groups)
				GenerateGroup(context, group);

			foreach (StructuredNodeInfo node in info.Nodes)
				GenerateNode(context, node);
		}
		GenerateSyntaxBundle(context, order);
		//GenerateShadowValidation(context, order);
	}

	static void GenerateGlobal(SourceProductionContext context, StructuredTreeInfo lastTree)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		using (writer.Preamble())
		{
			foreach (string ns in lastTree.Namespaces)
				writer.WriteLine($"global using {ns};");

			if (lastTree.Namespaces.Any())
				writer.WriteLine();
		}

		string source = result.ToString();
		context.AddSource("global.Syntax.g.cs", source);
	}

	static void GenerateSyntaxBundle(SourceProductionContext context, IReadOnlyList<StructuredTreeInfo> order)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		using (writer.Preamble(order.First().RootNamespace))
		{
			using (writer.TypePreamble()) // interface
			{
				writer.WriteLine($"public partial interface ISyntaxTreeBundle");
				using (writer.Braced())
				{
					using (writer.Region("Properties"))
					{
						writer.WriteLine("ISourceFile Source { get; }");
						foreach (StructuredTreeInfo tree in order)
							writer.WriteLine($"{tree.Interface.Name}? {tree.Kind.Pascal} {{ get; }}");
					}
				}
			}

			using (writer.TypePreamble()) // class
			{
				writer.WriteLine($"public sealed partial class SyntaxTreeBundle : ISyntaxTreeBundle");
				using (writer.Braced())
				{
					using (writer.Region("Fields"))
					{
						foreach (StructuredTreeInfo tree in order)
							writer.WriteLine($"private {tree.Interface.Name}? _{tree.Kind.Camel};");
					}
					writer.WriteLine();
					using (writer.Region("Properties"))
					{
						writer.WriteLine("public ISourceFile Source { get; }");
						foreach (StructuredTreeInfo tree in order)
						{
							writer.WriteLine($"public {tree.Interface.Name}? {tree.Kind.Pascal}");
							using (writer.Braced())
							{
								writer.WriteLine($"get => _{tree.Kind.Camel};");
								writer.WriteLine("set");
								using (writer.Braced())
								{
									writer.WriteLine($"_{tree.Kind.Camel} = value;");

									if (tree.Shadows is not null)
										writer.WriteLine($"_{tree.Shadows.Kind.Camel} = value; // re-use current as lower tree;");

									if (tree.ShadowedBy is not null)
										writer.WriteLine($"_{tree.ShadowedBy.Kind.Camel} = null; // invalidate richer tree;");
								}
							}
						}
					}
					writer.WriteLine();
					using (writer.Region("Constructors"))
					{
						writer.WriteLine($"public SyntaxTreeBundle(ISourceFile source) => Source = source;");
					}
				}
			}
		}

		string source = result.ToString();
		context.AddSource("SyntaxTreeBundle.g.cs", source);
	}
	static void GenerateShadowValidation(SourceProductionContext context, IReadOnlyList<StructuredTreeInfo> order)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		using (writer.Preamble())
		{
			writer.WriteLine("#if DEBUG");
			writer.WriteLine("#pragma warning disable CS0219 // Unused variable.");
			writer.WriteLine();

			using (writer.TypePreamble()) // class
			{
				writer.WriteLine("file static class ShadowValidation");
				using (writer.Braced())
				{
					using (writer.Region("Functions"))
					{
						foreach (StructuredTreeInfo tree in order)
						{
							if (tree.Shadows is null)
								continue;

							writer.WriteLine($"private static void Validate{tree.Kind.Pascal}To{tree.Kind.Pascal}()");
							using (writer.Braced())
							{
								writer.WriteLine($"{tree.Shadows.Token.Interface.Name}? n0 = ({tree.Token.Interface.Name}?)null;");
								for (int i = 0; i < tree.Nodes.Count; i++)
								{
									StructuredNodeInfo node = tree.Nodes[i];
									if (node.Shadows is null)
										throw new InvalidOperationException("A node with a shadow was expected.");

									writer.WriteLine($"{node.Shadows?.Interface.Name}? n{i + 1} = ({node.Interface.Name}?)null;");
								}
							}
						}
					}
				}
			}

			writer.WriteLine("#pragma warning restore CS0219 // Unused variable.");
			writer.WriteLine("#endif");
			writer.WriteLine();
		}

		string source = result.ToString();
		context.AddSource("ShadowingValidation.g.cs", source);
	}

	static void GenerateToken(SourceProductionContext context, StructuredTokenInfo token)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		using (writer.Preamble(token.Tree.Namespace))
		{
			using (writer.InterfacePreamble(token.Interface))
				writer.WriteInterfaceProperties(token.InterfaceMembers);

			using (writer.ClassPreamble(token.Class))
			{
				if (writer.WriteClassProperties(token.ClassMembers))
					writer.WriteLine();

				using (writer.Region("Constructors"))
				{
					#region Full constructors
					writer.Write($"public {token.Class.Name}(");
					writer.WriteSeparated([
						"SyntaxKind kind",
						"IndexedPositionRange position",
						"string? lexeme",
						"object? value",
						"TriviaList leadingTrivia",
						"TriviaList trailingTrivia",
						"ClassificationKind? classification",
						..token.ClassMembers.Select(m => $"{m.Type.TypeName} {m.Name.Camel}")
					]);
					writer.WriteLine(")");

					using (writer.Indented())
						writer.WriteLine(": base(kind, position, lexeme, value, leadingTrivia, trailingTrivia, classification)");

					using (writer.Braced())
					{
					}
					#endregion
					writer.WriteLine();
					#region Fabricated constructors
					writer.Write($"public {token.Class.Name}(");
					writer.WriteSeparated([
						"SyntaxKind kind",
						"IndexedPositionRange position",
						..token.ClassMembers.Select(m => $"{m.Type.TypeName} {m.Name.Camel}")
					]);
					writer.WriteLine(")");

					using (writer.Indented())
						writer.WriteLine(": base(kind, position)");

					using (writer.Braced())
						writer.WriteConstructorAssignments(token.ClassMembers);
					#endregion
				}
			}
		}

		string source = result.ToString();
		context.AddSource(token.Path, source);
	}
	static void GenerateBaseNode(SourceProductionContext context, StructuredTreeNodeInfo node)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		if (node.Members.Any())
			throw new InvalidOperationException("I got lazy so members on base nodes are not emitted yet.");

		using (writer.Preamble(node.Tree.Namespace))
		{
			using (writer.InterfacePreamble(node.Interface))
			{
			}
			using (writer.ClassPreamble(node.Class))
			{
			}
			using (writer.TypePreamble())
			{
				writer.WriteLine($"public static partial class {node.Interface.Name}Extensions");
				using (writer.Braced())
				{
					writer.WriteLine($"extension({node.Interface.Name} node)");
					using (writer.Braced())
					{
						using (writer.Region("Methods"))
						{
							string tokenName = node.Tree.Token.Interface.Name;
							writer.WriteLine($"public IReadOnlyList<{tokenName}> Flatten() => node.Flatten<{tokenName}>();");
						}
					}
				}
			}
		}

		string source = result.ToString();
		context.AddSource(node.Path, source);
	}
	static void GenerateTree(SourceProductionContext context, StructuredTreeInfo info)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);
		StructuredNodeInfo document = info.Nodes.Single(n => n.Name == "document");

		using (writer.Preamble(info.Namespace))
		{
			using (writer.InterfacePreamble(info.Interface))
				writer.WriteInterfaceProperties(info.Members);

			using (writer.ClassPreamble(info.Class))
			{
				if (writer.WriteClassProperties(info.Members))
					writer.WriteLine();

				using (writer.Region("Constructors", lineAfter: true))
				{
					writer.Write($"public {info.Class.Name}(");
					writer.WriteSeparated([
						"ISourceFile source",
						..info.Members.Select(m => $"{m.Type.TypeName} {m.Name.Camel}")
					]);
					writer.WriteLine(")");

					using (writer.Indented())
						writer.WriteLine(": base(source)");

					using (writer.Braced())
						writer.WriteConstructorAssignments(info.Members);
				}

				using (writer.Region("Methods"))
					writer.WriteLine($"public override string ToString() => {document.Name.Pascal}.Print();");
			}
		}

		string source = result.ToString();
		context.AddSource(info.TreePath, source);
	}
	static void GenerateGroup(SourceProductionContext context, StructuredGroupInfo info)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		using (writer.Preamble(info.Namespace))
		{
			using (writer.InterfacePreamble(info.Interface))
				writer.WriteInterfaceProperties(info.Members);
		}

		string source = result.ToString();
		context.AddSource(info.Path, source);
	}
	static void GenerateNode(SourceProductionContext context, StructuredNodeInfo info)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		using (writer.Preamble(info.Namespace))
		{
			using (writer.InterfacePreamble(info.Interface))
				writer.WriteInterfaceProperties(info.InterfaceMembers);

			using (writer.ClassPreamble(info.Class))
			{
				if (writer.WriteClassProperties(info.ClassMembers))
					writer.WriteLine();

				using (writer.Region("Constructors", lineAfter: true))
				{
					writer.Write($"public {info.Class.Name}(");
					writer.WriteSeparated(info.ClassMembers.Select(m => $"{m.Type.TypeName} {m.Name.Camel}"));
					writer.WriteLine(")");
					using (writer.Braced())
					{
						writer.WriteConstructorAssignments(info.ClassMembers);
						if (info.ClassMembers.Any())
							writer.WriteLine();

						writer.WriteLine("AssignParentToChildren();");
					}
				}

				using (writer.Region("Methods"))
				{
					IEnumerable<string> syntaxMembers = info.InterfaceMembers.Where(m => m.Type is IStructuredSyntaxTypeInfo).Select(m => m.Name.Pascal);
					writer.WriteLine($"public override IEnumerable<ISyntaxNode> GetChildren() => [{string.Join(", ", syntaxMembers)}];");
				}
			}
		}

		string source = result.ToString();
		context.AddSource(info.Path, source);
	}
	#endregion

	#region Helpers
	private static IndentedTextWriter GetWriter(out StringWriter result)
	{
		result = new();
		return new(result, "\t");
	}
	#endregion
}
