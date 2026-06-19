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
		IReadOnlyList<SyntaxTreeInfo> order = GetTreeOrder(description);

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

			if (info.Shadowed is not null)
				GenerateTreeConverter(context, info.Shadowed, info);

			GenerateTreeVisitor(context, info);
		}
		GenerateSyntaxBundle(context, order);
		GenerateShadowValidation(context, order);
	}
	private static IReadOnlyList<SyntaxTreeInfo> GetTreeOrder(SyntaxDescriptionFile description)
	{
		List<SyntaxTreeInfo> order = [];

		string? rootNamespace = description.OfType<NamespaceDescription>().FirstOrDefault().RootNamespace;
		if (rootNamespace is null)
			return order;

		ShadowingDescription? shadowing = description.OfType<ShadowingDescription>().FirstOrDefault();
		if (shadowing is null || shadowing.Order.Count is 0)
			return order;

		Dictionary<string, SyntaxTreeInfo> lookup = [];

		foreach (Name name in shadowing.Order) // figure out the order
		{
			TreeDescription? tree = description.OfType<TreeDescription>().FirstOrDefault(t => t.Kind == name);
			TokenDescription? token = description.OfType<TokenDescription>().FirstOrDefault(t => t.Kind == name);

			if (tree is null || token is null)
				return order;

			SyntaxTreeInfo? last = order.LastOrDefault();
			SyntaxTreeInfo info = new(rootNamespace, name, tree, token, last, tree.Members, [], [], []);

			order.Add(info);
			lookup.Add(info.Kind.Original, info);
		}

		foreach (SyntaxTreeInfo info in order) // figure out the groups
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

		foreach (SyntaxTreeInfo info in order) // figure out the individual nodes
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

		return order;
	}

	static void GenerateToken(SourceProductionContext context, SyntaxTreeInfo info)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		using (writer.Preamble(info.Namespace))
		{
			using (writer.TypePreamble()) // interface
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

			using (writer.TypePreamble()) // class
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
		TriviaList trailingTrivia,
		ClassificationKind? classification");

						foreach (MemberDescription member in info.TokenClassMembers)
						{
							writer.WriteLine(",");
							writer.Write($"\t{member.Type.TargetType} {member.Name.CamelCase}");
						}

						writer.WriteLine(")");
						writer.Write("\t: base(kind, position, lexeme, value, leadingTrivia, trailingTrivia, classification)");
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

		string source = result.ToString();
		context.AddSource(info.TokenPath, source);
	}
	static void GenerateBaseNode(SourceProductionContext context, SyntaxTreeInfo info)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		using (writer.Preamble(info.Namespace))
		{
			using (writer.TypePreamble()) // interface
			{
				writer.WriteLine($"public partial interface {info.INodeName} : {(info.Shadowed is null ? "ISyntaxNode" : info.Shadowed.INodeName)}");
				using (writer.Braced())
				{
				}
			}

			using (writer.TypePreamble()) // class
			{
				writer.WriteLine($"public abstract partial class {info.BaseNodeName} : BaseSyntaxNode, {info.INodeName}");
				using (writer.Braced())
				{
				}
			}

			using (writer.TypePreamble()) // extensions
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

		string source = result.ToString();
		context.AddSource(info.BaseNodePath, source);
	}
	static void GenerateTree(SourceProductionContext context, SyntaxTreeInfo info)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		using (writer.Preamble(info.Namespace))
		{
			using (writer.TypePreamble()) // interface
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

			using (writer.TypePreamble()) // class
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

		string source = result.ToString();
		context.AddSource(info.TreePath, source);
	}
	static void GenerateGroup(SourceProductionContext context, SyntaxGroupInfo info)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		using (writer.Preamble(info.Namespace))
		{
			using (writer.TypePreamble()) // interface
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

		string source = result.ToString();
		context.AddSource(info.InterfacePath, source);
	}
	static void GenerateNode(SourceProductionContext context, SyntaxNodeInfo info)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

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

								if (info.ClassMembers.Any())
								{
									writer.WriteLine();
									writer.WriteLine("AssignParentToChildren();");
								}
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

		string source = result.ToString();
		context.AddSource(info.Path, source);
	}
	static void GenerateTreeConverter(SourceProductionContext context, SyntaxTreeInfo from, SyntaxTreeInfo to)
	{
		Debug.Assert(to.ConverterName is not null);

		IndentedTextWriter writer = GetWriter(out StringWriter result);

		static void Generate(IndentedTextWriter writer, SyntaxNodeInfo from, SyntaxNodeInfo to)
		{
			string variable = from!.Kind.CamelCase;

			if (to.ClassMembers.Count is 0 && from.ClassMembers.Count is 0)
			{
				writer.WriteLine($"protected virtual {to.ClassName} Convert({from.InterfaceName} {variable}) => new();");
				return;
			}

			writer.WriteLine($"protected abstract {to.ClassName} Convert({from.InterfaceName} {variable});");
		}

		using (writer.Preamble(to.Namespace))
		{
			using (writer.TypePreamble())
			{
				writer.WriteLine($"public abstract partial class {to.ConverterName}");
				using (writer.Braced())
				{
					using (writer.Region("Methods"))
					{
						writer.WriteLine($"protected virtual {to.TreeName} Convert({from.ITreeName} tree)");
						using (writer.Braced())
						{
							writer.WriteLine($"{to.Document.ClassName} {to.Document.CamelName} = Convert(tree.{from.Document.PascalName});");
							writer.WriteLine($"return new(tree.Source, {to.Document.CamelName});");
						}

						foreach (SyntaxNodeInfo target in to.Nodes.Where(n => n.Group is null).OrderBy(n => n.ClassMembers.Count))
						{
							SyntaxNodeInfo? origin = target.Shadowed;
							Debug.Assert(origin is not null);

							Generate(writer, origin!, target);
						}
					}
					if (to.Groups.Any())
					{
						foreach (SyntaxGroupInfo group in to.Groups.Values)
						{
							writer.WriteLine();

							string groupKind = group.Kind.CamelCase;
							string lastName = new Name(group.Name.Parts.Last()).CamelCase;

							SyntaxGroupInfo? targetGroup = group.Shadowed;
							Debug.Assert(targetGroup is not null);

							using (writer.Region($"{group.Name.PascalNatural} methods"))
							{
								writer.WriteLine($"protected virtual {targetGroup!.InterfaceName} Convert({group.InterfaceName} {groupKind})");
								using (writer.Braced())
								{
									writer.WriteLine($"return {groupKind} switch");
									using (writer.Braced(terminate: true))
									{
										foreach (SyntaxNodeInfo target in group.Nodes.Values.OrderBy(n => n.ClassMembers.Count))
										{
											Debug.Assert(target.Shadowed is not null);
											writer.WriteLine($"{target.Shadowed!.InterfaceName} {lastName} => Convert({lastName}),");
										}

										writer.WriteLine();
										writer.WriteLine($"_ => throw new ArgumentException($\"Unknown {lastName} node type ({{{groupKind}.GetType().Name}}).\", nameof({groupKind}))");
									}
								}
								foreach (SyntaxNodeInfo target in group.Nodes.Values.OrderBy(n => n.ClassMembers.Count))
								{
									SyntaxNodeInfo? origin = target.Shadowed;
									Debug.Assert(origin is not null);

									Generate(writer, origin!, target);
								}
							}
						}
					}
				}
			}
		}

		string source = result.ToString();
		context.AddSource($"{to.Directory}/{to.ConverterName}.g.cs", source);
	}
	static void GenerateTreeVisitor(SourceProductionContext context, SyntaxTreeInfo tree)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		using (writer.Preamble(tree.Namespace))
		{
			using (writer.TypePreamble())
			{
				writer.WriteLine($"public abstract partial class {tree.VisitorName}");
				using (writer.Braced())
				{
					using (writer.Region("Methods"))
					{
						writer.WriteLine($"protected virtual void Visit({tree.ITreeName} tree) => Dispatch(tree.{tree.Document.PascalName});");

						writer.WriteLine($"protected virtual void VisitChildren(ISyntaxNode node)");
						using (writer.Braced())
						{
							writer.WriteLine($"foreach (ISyntaxNode child in node.GetChildren())");
							using (writer.Indented())
								writer.WriteLine($"Dispatch(child);");
						}

						writer.WriteLine($"protected virtual void Dispatch(ISyntaxNode node)");
						using (writer.Braced())
						{
							writer.WriteLine("bool visitChildren = node switch");
							using (writer.Braced(terminate: true))
							{
								foreach (SyntaxNodeInfo node in tree.Nodes)
									writer.WriteLine($"{node.InterfaceName} n => Visit(n),");

								if (tree.Nodes.Any())
									writer.WriteLine();

								writer.WriteLine("ISyntaxTrivia trivia => Visit(trivia),");
								writer.WriteLine("TriviaList list => Visit(list),");
								writer.WriteLine($"{tree.ITokenName} token => Visit(token),");

								writer.WriteLine();
								writer.WriteLine("_ => VisitUnknown(node) // Used for syntax lists");
							}

							writer.WriteLine();
							writer.WriteLine($"if (visitChildren)");
							using (writer.Indented())
								writer.WriteLine($"VisitChildren(node);");
						}
					}
					writer.WriteLine();
					using (writer.Region("Node methods"))
					{
						writer.WriteLine($"protected virtual bool Visit(ISyntaxTrivia trivia) => true;");
						writer.WriteLine($"protected virtual bool Visit(TriviaList list) => true;");
						writer.WriteLine($"protected virtual bool Visit({tree.ITokenName} token) => false; // Don't waste time visiting trivia nodes by default");
						if (tree.Nodes.Any())
							writer.WriteLine();

						foreach (SyntaxNodeInfo node in tree.Nodes)
							writer.WriteLine($"protected virtual bool Visit({node.InterfaceName} node) => true;");

						writer.WriteLine();
						writer.WriteLine($"protected virtual bool VisitUnknown(ISyntaxNode node) => true;");
					}
				}
			}
		}

		string text = result.ToString();
		context.AddSource($"{tree.Directory}/{tree.VisitorName}.g.cs", text);
	}

	static void GenerateGlobal(SourceProductionContext context, SyntaxTreeInfo lastTree)
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

	static void GenerateSyntaxBundle(SourceProductionContext context, IReadOnlyList<SyntaxTreeInfo> order)
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
						foreach (SyntaxTreeInfo tree in order)
							writer.WriteLine($"{tree.ITreeName}? {tree.PascalKind} {{ get; }}");
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

		string source = result.ToString();
		context.AddSource("SyntaxTreeBundle.g.cs", source);
	}
	static void GenerateShadowValidation(SourceProductionContext context, IReadOnlyList<SyntaxTreeInfo> order)
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

		string source = result.ToString();
		context.AddSource("ShadowingValidation.g.cs", source);
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
