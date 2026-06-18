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

		Generate(context, description);
	}
	private static void Generate(SourceProductionContext context, SyntaxDescriptionFile description)
	{
		string? rootNamespace = description.OfType<NamespaceDescription>().FirstOrDefault().RootNamespace;
		if (rootNamespace is null)
			return;

		ShadowingDescription? shadowing = description.OfType<ShadowingDescription>().FirstOrDefault();
		if (shadowing is null || shadowing.Order.Count is 0)
			return;

		List<SyntaxTreeInfo> order = [];
		Dictionary<string, SyntaxTreeInfo> lookup = [];

		foreach (Name name in shadowing.Order)
		{
			TreeDescription? tree = description.OfType<TreeDescription>().FirstOrDefault(t => t.Kind == name);
			TokenDescription? token = description.OfType<TokenDescription>().FirstOrDefault(t => t.Kind == name);

			if (tree is null || token is null)
				return;

			SyntaxTreeInfo? last = order.LastOrDefault();
			SyntaxTreeInfo info = new(rootNamespace, name, tree, token, last, tree.Members, [], [], []);

			order.Add(info);
			lookup.Add(info.Kind.Original, info);
		}

		foreach (SyntaxTreeInfo info in order)
		{
			foreach (GroupDescription group in description.OfType<GroupDescription>())
			{
				OnKindDescription? modifier = description.OfType<OnKindDescription>().FirstOrDefault(d => d.Name == group.Name && d.Kind == info.Kind);
				SyntaxGroupInfo? shadowed = info.Shadowed?.Groups.GetValueOrDefault(group.Name);

				MemberDescriptionList members = group.Members.Concat(modifier is not null ? modifier.Members : []).ToArray();

				SyntaxGroupInfo groupInfo = new(info, info.Kind, group.Name, shadowed, members, []);
				info.Groups.Add(group.Name.Original, groupInfo);
			}
		}

		foreach (SyntaxTreeInfo info in order)
		{
			foreach (NodeDescription node in description.OfType<NodeDescription>())
			{
				OnKindDescription? modifier = description
					.OfType<OnKindDescription>()
					.Where(d => info.Groups.GetValueOrDefault(d.Name) is null)
					.FirstOrDefault(d => d.Name == node.Name && d.Kind == info.Kind);

				SyntaxGroupInfo? group = info.Groups.GetValueOrDefault(node.Kind.Original);
				SyntaxNodeInfo? shadowed =
					info.Shadowed?.LookupNodes.GetValueOrDefault(node.Name) ??
					info.Shadowed?.LookupNodes.GetValueOrDefault(node.Name + "_" + group.Name.Original);

				if (shadowed is null && info.Shadowed is not null)
					throw new InvalidOperationException($"Couldn't find a shadow node for '{info.Kind.Original}_{node.Name.Original}_{node.Kind.Original}'.");

				MemberDescriptionList members = node.Members
					.Concat(modifier is not null ? modifier.Members : [])
					//.Concat(group is not null ? group.Members : [])
					//.Concat(group?.Shadowed is not null ? group.Shadowed.Members : [])
					.ToArray();

				SyntaxNodeInfo nodeInfo = new(info, group, info.Kind, node.Name, shadowed, members);

				if (group is null)
					info.LookupNodes.Add(nodeInfo.Name.Original, nodeInfo);
				else
				{
					info.LookupNodes.Add(nodeInfo.Name + "_" + group.Name.Original, nodeInfo);
					group.Nodes.Add(nodeInfo.Name.Original, nodeInfo);
				}

				info.Nodes.Add(nodeInfo);
			}
		}

		GenerateGlobal(context, order.Last());
		foreach (SyntaxTreeInfo info in order)
		{
			GenerateToken(context, info);
			GenerateBaseNode(context, info);
			GenerateTree(context, info);

			foreach (SyntaxGroupInfo group in info.Groups.Values)
				GenerateGroup(context, group);

			foreach (SyntaxNodeInfo node in info.Nodes)
				GenerateNode(context, node);
		}
		GenerateSyntaxBundle(context, order);
		GenerateShadowValidation(context, order);
	}

	static void GenerateToken(SourceProductionContext context, SyntaxTreeInfo info)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter writer = new(stringWriter, "\t");

		using (writer.Preamble(info.Namespace))
		{
			using (writer.TypePreamble())
			{
				writer.WriteLine($"public partial interface {info.ITokenName} : {info.INodeName}, {(info.Shadowed is null ? "ISyntaxToken" : info.Shadowed.ITokenName)}");
				using (writer.Braced())
				{
					if (info.TokenInterfaceMembers.Any())
					{
						using (writer.Region("Properties"))
						{
							foreach (MemberDescription member in info.TokenInterfaceMembers)
								writer.WriteLine($"{member.Type.TargetType} {member.Name.PascalCase} {{ get; }}");
						}
					}
				}
			}

			using (writer.TypePreamble())
			{
				writer.WriteLine($"public sealed partial class {info.TokenName} : BaseSyntaxToken, {info.ITokenName}");
				using (writer.Braced())
				{
					if (info.TokenClassMembers.Any())
					{
						using (writer.Region("Properties"))
						{
							foreach (MemberDescription member in info.TokenClassMembers)
								writer.WriteLine($"public {member.Type.TargetType} {member.Name.PascalCase} {{ get; }}");
						}
						writer.WriteLine();
					}

					using (writer.Region("Constructors"))
					{
						#region Full constructor
						writer.Write($@"public {info.TokenName}(
		SyntaxKind kind,
		IndexedPositionRange position,
		string? lexeme,
		object? value,
		TriviaList leadingTrivia,
		TriviaList trailingTrivia");

						foreach (MemberDescription member in info.TokenClassMembers)
						{
							writer.WriteLine(",");
							writer.Write($"\t{member.Type.TargetType} {member.Name.CamelCase}");
						}

						writer.WriteLine(")");
						writer.Write("\t: base(kind, position, lexeme, value, leadingTrivia, trailingTrivia)");
						if (info.TokenClassMembers.Any() is false)
							writer.WriteLine(" {}");
						else
						{
							writer.WriteLine();
							using (writer.Braced())
							{
								foreach (MemberDescription member in info.TokenClassMembers)
									writer.WriteLine($"{member.Name.PascalCase} = {member.Name.CamelCase};");
							}
						}
						#endregion
						writer.WriteLine();
						#region Fabricated constructor
						writer.Write($@"public {info.TokenName}(
		SyntaxKind kind,
		IndexedPositionRange position");

						foreach (MemberDescription member in info.TokenClassMembers)
						{
							writer.WriteLine(",");
							writer.Write($"\t{member.Type.TargetType} {member.Name.CamelCase}");
						}

						writer.WriteLine(")");
						writer.Write("\t: base(kind, position)");
						if (info.TokenClassMembers.Any() is false)
							writer.WriteLine(" {}");
						else
						{
							writer.WriteLine();
							using (writer.Braced())
							{
								foreach (MemberDescription member in info.TokenClassMembers)
									writer.WriteLine($"{member.Name.PascalCase} = {member.Name.CamelCase};");
							}
						}
						#endregion
					}
				}
			}
		}

		string source = stringWriter.ToString();
		context.AddSource(info.TokenPath, source);
	}
	static void GenerateBaseNode(SourceProductionContext context, SyntaxTreeInfo info)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter writer = new(stringWriter, "\t");

		using (writer.Preamble(info.Namespace))
		{
			using (writer.TypePreamble())
			{
				writer.WriteLine($"public partial interface {info.INodeName} : {(info.Shadowed is null ? "ISyntaxNode" : info.Shadowed.INodeName)}");
				using (writer.Braced())
				{
				}
			}

			using (writer.TypePreamble())
			{
				writer.WriteLine($"public abstract partial class {info.BaseNodeName} : BaseSyntaxNode, {info.INodeName}");
				using (writer.Braced())
				{
				}
			}

			using (writer.TypePreamble())
			{
				writer.WriteLine($"public static partial class {info.INodeName}Extensions");
				using (writer.Braced())
				{
					writer.WriteLine($"extension({info.INodeName} node)");
					using (writer.Braced())
					{
						using (writer.Region("Methods"))
						{
							writer.WriteLine($"public IReadOnlyList<{info.ITokenName}> Flatten() => node.Flatten<{info.ITokenName}>();");
						}
					}
				}
			}
		}

		string source = stringWriter.ToString();
		context.AddSource(info.BaseNodePath, source);
	}
	static void GenerateTree(SourceProductionContext context, SyntaxTreeInfo info)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter writer = new(stringWriter, "\t");

		using (writer.Preamble(info.Namespace))
		{
			using (writer.TypePreamble())
			{
				writer.WriteLine($"public partial interface {info.ITreeName} : {(info.Shadowed is null ? "ISyntaxTree" : info.Shadowed.ITreeName)}");
				using (writer.Braced())
				{
					using (writer.Region("Properties"))
					{
						if (info.Shadowed is null)
						{
							writer.WriteLine($"{info.Document.InterfaceName} {info.Document.PascalName} {{ get; }}");
						}
						else
						{
							writer.WriteLine($"new {info.Document.InterfaceName} {info.Document.PascalName} {{ get; }}");
							writer.WriteLine($"{info.Shadowed.Document.InterfaceName} {info.Shadowed.ITreeName}.{info.Shadowed.Document.PascalName} => {info.Document.PascalName};");
						}
					}
				}
			}

			using (writer.TypePreamble())
			{
				writer.WriteLine($"public sealed partial class {info.TreeName} : BaseSyntaxTree, {info.ITreeName}");
				using (writer.Braced())
				{
					using (writer.Region("Properties"))
					{
						writer.WriteLine($"public {info.Document.InterfaceName} {info.Document.PascalName} {{ get; }}");
					}
					writer.WriteLine();
					using (writer.Region("Constructors"))
					{
						writer.WriteLine($"public {info.TreeName}(ISourceFile source, {info.Document.InterfaceName} {info.Document.CamelName}) : base(source)");
						using (writer.Braced())
							writer.WriteLine($"{info.Document.PascalName} = {info.Document.CamelName};");
					}
					writer.WriteLine();
					using (writer.Region("Methods"))
					{
						writer.WriteLine($"public override string ToString() => {info.Document.PascalName}.Print();");
					}
				}
			}
		}

		string source = stringWriter.ToString();
		context.AddSource(info.TreePath, source);
	}
	static void GenerateGroup(SourceProductionContext context, SyntaxGroupInfo info)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter writer = new(stringWriter, "\t");

		using (writer.Preamble(info.Namespace))
		{
			using (writer.TypePreamble())
			{
				writer.Write($"public partial interface {info.InterfaceName} : {info.Tree.INodeName}");
				if (info.Shadowed is not null)
					writer.WriteLine($", {info.Shadowed.InterfaceName}");
				else
					writer.WriteLine();

				using (writer.Braced())
				{
					if (info.InterfaceMembers.Any())
					{
						using (writer.Region("Properties"))
						{
							foreach (MemberDescription member in info.InterfaceMembers)
								writer.WriteLine($"{member.Type.TargetType} {member.Name.PascalCase} {{ get; }}");
						}
					}
				}
			}
		}

		string source = stringWriter.ToString();
		context.AddSource(info.InterfacePath, source);
	}
	static void GenerateNode(SourceProductionContext context, SyntaxNodeInfo info)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter writer = new(stringWriter, "\t");

		using (writer.Preamble(info.Namespace))
		{
			using (writer.TypePreamble()) // interface
			{
				writer.Write($"public partial interface {info.InterfaceName} : {info.BaseInterfaceName}");
				if (info.Shadowed is not null)
					writer.WriteLine($", {info.Shadowed.InterfaceName}");
				else
					writer.WriteLine();

				using (writer.Braced())
				{
					if (info.InterfaceMembers.Any())
					{
						using (writer.Region("Properties"))
						{
							if (info.Shadowed is null)
							{
								foreach (MemberDescription member in info.InterfaceMembers)
									writer.WriteLine($"{member.Type.GetTargetType(info)} {member.Name.PascalCase} {{ get; }}");
							}
							else
							{
								for (int i = 0; i < info.InterfaceMembers.Count; i++)
								{
									MemberDescription member = info.InterfaceMembers[i];
									MemberDescription? shadowed = info.Shadowed.InterfaceMembers.FirstOrDefault(m => m.Name == member.Name);

									if (shadowed is not null && shadowed.Type.IsSyntaxType is false)
										continue;

									if (i > 0)
										writer.WriteLine();

									if (shadowed is null)
										writer.WriteLine($"{member.Type.GetTargetType(info)} {member.Name.PascalCase} {{ get; }}");
									else
									{
										writer.WriteLine($"new {member.Type.GetTargetType(info)} {member.Name.PascalCase} {{ get; }}");
										writer.WriteLine($"{shadowed.Type.GetTargetType(info.Shadowed)} {info.Shadowed.InterfaceName}.{shadowed.Name.PascalCase} => {member.Name.PascalCase};");
									}
								}
							}
						}
					}
				}
			}

			using (writer.TypePreamble()) // class
			{
				writer.WriteLine($"public sealed partial class {info.ClassName} : {info.Tree.BaseNodeName}, {info.InterfaceName}");
				using (writer.Braced())
				{
					if (info.ClassMembers.Any())
					{
						using (writer.Region("Properties"))
						{
							foreach (MemberDescription member in info.ClassMembers)
								writer.WriteLine($"public {member.Type.GetTargetType(info)} {member.Name.PascalCase} {{ get; }}");
						}
						writer.WriteLine();
						using (writer.Region("Constructors"))
						{
							if (info.ClassMembers.Count is 1)
								writer.Write($"public {info.ClassName}(");
							else
								writer.WriteLine($"public {info.ClassName}(");

							for (int i = 0; i < info.ClassMembers.Count; i++)
							{
								if (i > 0)
									writer.WriteLine(",");

								if (info.ClassMembers.Count is not 1)
									writer.Write("\t");

								MemberDescription member = info.ClassMembers[i];
								writer.Write($"{member.Type.GetTargetType(info)} {member.Name.CamelCase}");
							}
							writer.WriteLine(")");
							using (writer.Braced())
							{
								foreach (MemberDescription member in info.ClassMembers)
									writer.WriteLine($"{member.Name.PascalCase} = {member.Name.CamelCase};");
							}
						}
					}
					writer.WriteLine();
					using (writer.Region("Methods"))
					{
						writer.Write("public override IEnumerable<ISyntaxNode> GetChildren() => [");
						for (int i = 0; i < info.SyntaxMembers.Count; i++)
						{
							if (i > 0)
								writer.Write(", ");

							MemberDescription member = info.SyntaxMembers[i];
							writer.Write(member.Name.PascalCase);
						}
						writer.WriteLine("];");
					}
				}
			}
		}

		string source = stringWriter.ToString();
		context.AddSource(info.Path, source);
	}
	static void GenerateGlobal(SourceProductionContext context, SyntaxTreeInfo lastTree)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter writer = new(stringWriter, "\t");

		using (writer.Preamble())
		{
			foreach (string ns in lastTree.Namespaces)
				writer.WriteLine($"global using {ns};");

			if (lastTree.Namespaces.Any())
				writer.WriteLine();
		}

		string source = stringWriter.ToString();
		context.AddSource("global.Syntax.g.cs", source);
	}
	static void GenerateSyntaxBundle(SourceProductionContext context, IReadOnlyList<SyntaxTreeInfo> order)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter writer = new(stringWriter, "\t");

		using (writer.Preamble(order.First().RootNamespace))
		{
			using (writer.TypePreamble())
			{
				writer.WriteLine($"public partial interface ISyntaxTreeBundle");
				using (writer.Braced())
				{
					using (writer.Region("Properties"))
					{
						writer.WriteLine("ISourceFile Source { get; }");
						foreach (SyntaxTreeInfo tree in order)
							writer.WriteLine($"{tree.ITreeName}? {tree.PascalKind} {{ get; }}");
					}
				}
			}

			using (writer.TypePreamble())
			{
				writer.WriteLine($"public sealed partial class SyntaxTreeBundle : ISyntaxTreeBundle");
				using (writer.Braced())
				{
					using (writer.Region("Fields"))
					{
						foreach (SyntaxTreeInfo tree in order)
							writer.WriteLine($"private {tree.ITreeName}? _{tree.CamelKind};");
					}
					writer.WriteLine();
					using (writer.Region("Properties"))
					{
						writer.WriteLine("public ISourceFile Source { get; }");
						for (int i = 0; i < order.Count; i++)
						{
							SyntaxTreeInfo tree = order[i];
							writer.WriteLine($"public {tree.ITreeName}? {tree.PascalKind}");
							using (writer.Braced())
							{
								writer.WriteLine($"get => _{tree.CamelKind};");
								writer.WriteLine("set");
								using (writer.Braced())
								{
									writer.WriteLine($"_{tree.CamelKind} = value;");

									if (i > 0)
										writer.WriteLine($"_{order[i - 1].CamelKind} = value; // re-use current as lower tree;");

									if (i < order.Count - 1)
										writer.WriteLine($"_{order[i + 1].CamelKind} = null; // invalidate richer tree;");
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

		string source = stringWriter.ToString();
		context.AddSource("SyntaxTreeBundle.g.cs", source);
	}
	static void GenerateShadowValidation(SourceProductionContext context, IReadOnlyList<SyntaxTreeInfo> order)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter writer = new(stringWriter, "\t");

		using (writer.Preamble())
		{
			writer.WriteLine("#if DEBUG");
			writer.WriteLine("#pragma warning disable CS0219 // Unused variable.");
			writer.WriteLine();
			using (writer.TypePreamble())
			{
				writer.WriteLine("file static class ShadowValidation");
				using (writer.Braced())
				{
					using (writer.Region("Functions"))
					{
						foreach (SyntaxTreeInfo tree in order)
						{
							if (tree.Shadowed is null)
								continue;

							writer.WriteLine($"private static void Validate{tree.PascalKind}To{tree.Shadowed.PascalKind}()");
							using (writer.Braced())
							{
								writer.WriteLine($"{tree.Shadowed.ITokenName}? n0 = ({tree.ITokenName}?)null;");
								for (int i = 0; i < tree.Nodes.Count; i++)
								{
									SyntaxNodeInfo node = tree.Nodes[i];
									Debug.Assert(node.Shadowed is not null);

									writer.WriteLine($"{node.Shadowed?.InterfaceName}? n{i + 1} = ({node.InterfaceName}?)null;");
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

		string source = stringWriter.ToString();
		context.AddSource("ShadowingValidation.g.cs", source);
	}
	#endregion
}
