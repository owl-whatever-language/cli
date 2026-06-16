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
			SyntaxTreeInfo info = new(rootNamespace, name, tree, token, last, tree.Members, [], []);

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

				SyntaxGroupInfo groupInfo = new(info, info.Kind, group.Name, shadowed, members);
				info.Groups.Add(group.Name.Original, groupInfo);
			}
		}

		foreach (SyntaxTreeInfo info in order)
		{
			foreach (NodeDescription node in description.OfType<NodeDescription>())
			{
				OnKindDescription? modifier = description.OfType<OnKindDescription>().FirstOrDefault(d => d.Name == node.Name && d.Kind == info.Kind);
				SyntaxGroupInfo? group = info.Groups.GetValueOrDefault(node.Kind.Original);
				SyntaxNodeInfo? shadowed = info.Shadowed?.Nodes.GetValueOrDefault(node.Name);

				MemberDescriptionList members = node.Members
					.Concat(modifier is not null ? modifier.Members : [])
					.Concat(group is not null ? group.Members : [])
					.Concat(group?.Shadowed is not null ? group.Shadowed.Members : [])
					.ToArray();

				SyntaxNodeInfo nodeInfo = new(info, group, info.Kind, node.Name, shadowed, members);
				info.Nodes.Add(nodeInfo.Name.Original, nodeInfo);
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

			foreach (SyntaxNodeInfo node in info.Nodes.Values)
				GenerateNode(context, node);
		}
		GenerateSyntaxBundle(context, order);
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
					if (info.OnlyTokenMembers.Any())
					{
						using (writer.Region("Properties"))
						{
							foreach (MemberDescription member in info.OnlyTokenMembers)
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
					if (info.AllTokenMembers.Any())
					{
						using (writer.Region("Properties"))
						{
							foreach (MemberDescription member in info.AllTokenMembers)
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

						foreach (MemberDescription member in info.AllTokenMembers)
						{
							writer.WriteLine(",");
							writer.Write($"\t{member.Type.TargetType} {member.Name.CamelCase}");
						}

						writer.WriteLine(")");
						writer.Write("\t: base(kind, position, lexeme, value, leadingTrivia, trailingTrivia)");
						if (info.AllTokenMembers.Any() is false)
							writer.WriteLine(" {}");
						else
						{
							writer.WriteLine();
							using (writer.Braced())
							{
								foreach (MemberDescription member in info.AllTokenMembers)
									writer.WriteLine($"{member.Name.PascalCase} = {member.Name.CamelCase};");
							}
						}
						#endregion
						writer.WriteLine();
						#region Fabricated constructor
						writer.Write($@"public {info.TokenName}(
		SyntaxKind kind,
		IndexedPositionRange position");

						foreach (MemberDescription member in info.AllTokenMembers)
						{
							writer.WriteLine(",");
							writer.Write($"\t{member.Type.TargetType} {member.Name.CamelCase}");
						}

						writer.WriteLine(")");
						writer.Write("\t: base(kind, position)");
						if (info.AllTokenMembers.Any() is false)
							writer.WriteLine(" {}");
						else
						{
							writer.WriteLine();
							using (writer.Braced())
							{
								foreach (MemberDescription member in info.AllTokenMembers)
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
			using (writer.TypePreamble())
			{
				writer.Write($"public partial interface {info.InterfaceName} : {info.BaseInterfaceName}");
				if (info.Shadowed is not null)
					writer.WriteLine($", {info.Shadowed.InterfaceName}");
				else
					writer.WriteLine();

				using (writer.Braced())
				{
					if (info.Members.Any())
					{
						using (writer.Region("Properties"))
						{
							if (info.Shadowed is null)
							{
								foreach (MemberDescription member in info.Members)
									writer.WriteLine($"{member.Type.GetTargetType(info)} {member.Name.PascalCase} {{ get; }}");
							}
							else
							{
								for (int i = 0; i < info.Members.Count; i++)
								{
									if (i > 0)
										writer.WriteLine();

									MemberDescription member = info.Members[i];
									MemberDescription? shadowed = info.Shadowed.Members.FirstOrDefault(m => m.Name == member.Name);
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

			using (writer.TypePreamble())
			{
				writer.WriteLine($"public sealed partial class {info.ClassName} : {info.Tree.BaseNodeName}, {info.InterfaceName}");
				using (writer.Braced())
				{
					if (info.AllMembers.Any())
					{
						using (writer.Region("Properties"))
						{
							foreach (MemberDescription member in info.AllMembers)
								writer.WriteLine($"public {member.Type.GetTargetType(info)} {member.Name.PascalCase} {{ get; }}");
						}
						writer.WriteLine();
						using (writer.Region("Constructors"))
						{
							if (info.AllMembers.Count is 1)
								writer.Write($"public {info.ClassName}(");
							else
								writer.WriteLine($"public {info.ClassName}(");

							for (int i = 0; i < info.AllMembers.Count; i++)
							{
								if (i > 0)
									writer.WriteLine(",");

								if (info.AllMembers.Count is not 1)
									writer.Write("\t");

								MemberDescription member = info.AllMembers[i];
								writer.Write($"{member.Type.GetTargetType(info)} {member.Name.CamelCase}");
							}
							writer.WriteLine(")");
							using (writer.Braced())
							{
								foreach (MemberDescription member in info.AllMembers)
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
					using (writer.Region("Properties"))
					{
						writer.WriteLine("public ISourceFile Source { get; }");
						foreach (SyntaxTreeInfo tree in order)
						{
							if (tree.Shadowed is null)
							{
								writer.WriteLine($"public {tree.ITreeName}? {tree.PascalKind} {{ get; set; }}");
								continue;
							}

							writer.WriteLine($"public {tree.ITreeName}? {tree.PascalKind}");
							using (writer.Braced())
							{
								writer.WriteLine("get => field;");
								writer.WriteLine("set");
								using (writer.Braced())
								{
									writer.WriteLine("field = value;");
									writer.WriteLine($"{tree.Shadowed.PascalKind} = value;");
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
	#endregion
}
