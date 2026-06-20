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

				SyntaxNodeInfo nodeInfo = new(info, group, info.Kind, node.Name, shadowed, members, modifier?.Members ?? []);

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
							if (info.Shadowed is null)
							{
								foreach (MemberDescription member in info.InterfaceMembers)
									writer.WriteLine($"{member.Type.GetTargetType(info.Tree)} {member.Name.PascalCase} {{ get; }}");
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
										writer.WriteLine($"{member.Type.GetTargetType(info.Tree)} {member.Name.PascalCase} {{ get; }}");
									else
									{
										writer.WriteLine($"new {member.Type.GetTargetType(info.Tree)} {member.Name.PascalCase} {{ get; }}");
										writer.WriteLine($"{shadowed.Type.GetTargetType(info.Shadowed.Tree)} {info.Shadowed.InterfaceName}.{shadowed.Name.PascalCase} => {member.Name.PascalCase};");
									}
								}
							}
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
							if (info.Shadowed is null && info.Group is null)
							{
								foreach (MemberDescription member in info.InterfaceMembers)
									writer.WriteLine($"{member.Type.GetTargetType(info)} {member.Name.PascalCase} {{ get; }}");
							}
							else
							{
								for (int i = 0; i < info.InterfaceMembers.Count; i++)
								{
									MemberDescription member = info.InterfaceMembers[i];
									MemberDescription? shadowed = info.Group?.InterfaceMembers.FirstOrDefault(m => m.Name == member.Name);
									Isyntaxtre
									Name? shadowedTypeName = null;

									if (shadowed is not null)
										shadowedTypeName = info.Group?.Name;
									else
									{
										shadowed = info.Shadowed?.InterfaceMembers.FirstOrDefault(m => m.Name == member.Name);
										shadowedTypeName = info.Shadowed?.Name;
									}

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
		bool canAutoImplementToken = to.TokenClassMembers.All(m => m.IsNullable);

		#region Generate node conversion
		static void Generate(IndentedTextWriter writer, SyntaxNodeInfo from, SyntaxNodeInfo to, bool canAutoImplement)
		{
			string variable = from!.Kind.CamelCase;

			if (to.ClassMembers.Count is 0 && from.ClassMembers.Count is 0)
			{
				writer.WriteLine($"protected virtual {to.ClassName} Convert({from.InterfaceName} {variable}) => new();");
				return;
			}

			if (canAutoImplement && to.ClassMembers.Count == from.ClassMembers.Count)
			{
				writer.WriteLine($"protected virtual {to.ClassName} Convert({from.InterfaceName} {variable})");
				using (writer.Braced())
				{
					foreach (MemberDescription member in to.ClassMembers)
					{
						if (member.Type.IsSyntaxType)
							writer.WriteLine($"{member.Type.GetTargetType(to)} {member.Name.CamelCase} = Convert({variable}.{member.Name.PascalCase});");
						else
							writer.WriteLine($"{member.Type.GetTargetType(to)} {member.Name.CamelCase} = {variable}.{member.Name.PascalCase};");
					}

					writer.WriteLine();
					writer.WriteLine("return new(");
					using (writer.Indented())
					{
						for (int i = 0; i < to.ClassMembers.Count; i++)
						{
							if (i > 0)
								writer.WriteLine(",");

							MemberDescription member = to.ClassMembers[i];
							writer.Write(member.Name.CamelCase);
						}
					}
					writer.WriteLine(");");
				}

				return;
			}

			writer.WriteLine($"protected abstract {to.ClassName} Convert({from.InterfaceName} {variable});");
		}
		#endregion

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

							Generate(writer, origin!, target, canAutoImplementToken);
						}
					}
					writer.WriteLine();
					using (writer.Region("Token methods"))
					{
						#region Full
						writer.WriteLine($"protected virtual {to.TokenName} Convert(");
						using (writer.Indented())
						{
							writer.WriteLine($"{from.ITokenName} token,");
							writer.Write("ClassificationKind? classification");
							foreach (MemberDescription member in to.TokenClassMembers)
							{
								writer.WriteLine(",");
								writer.Write($"{member.Type.TargetType} {member.Name.CamelCase}");
							}
							writer.WriteLine(")");
						}
						using (writer.Braced())
						{
							writer.WriteLine("return new(");
							using (writer.Indented())
							{
								writer.WriteLine("token.Kind,");
								writer.WriteLine("token.Position,");
								writer.WriteLine("token.Lexeme,");
								writer.WriteLine("token.Value,");
								writer.WriteLine("token.LeadingTrivia,");
								writer.WriteLine("token.TrailingTrivia,");
								writer.Write("classification");

								foreach (MemberDescription member in to.TokenClassMembers)
								{
									writer.WriteLine(",");
									writer.Write(member.Name.CamelCase);
								}
								writer.WriteLine(");");
							}
						}
						#endregion

						writer.WriteLine();

						#region Inherit classification
						writer.WriteLine($"protected virtual {to.TokenName} Convert(");
						using (writer.Indented())
						{
							writer.Write($"{from.ITokenName} token");
							foreach (MemberDescription member in to.TokenClassMembers)
							{
								writer.WriteLine(",");
								writer.Write($"{member.Type.TargetType} {member.Name.CamelCase}");
							}

							writer.WriteLine(")");
						}
						using (writer.Braced())
						{
							writer.WriteLine("return Convert(");
							using (writer.Indented())
							{
								writer.WriteLine("token,");
								writer.Write("token.Classification");

								foreach (MemberDescription member in to.TokenClassMembers)
								{
									writer.WriteLine(",");
									writer.Write(member.Name.CamelCase);
								}
							}
							writer.WriteLine(");");
						}
						#endregion

						#region Auto implementation
						if (canAutoImplementToken)
						{
							writer.WriteLine();
							writer.WriteLine($"protected virtual {to.TokenName} Convert({from.ITokenName} token)");
							using (writer.Braced())
							{
								writer.WriteLine("return Convert(");
								using (writer.Indented())
								{
									writer.WriteLine("token,");
									writer.Write("token.Classification");

									for (int i = 0; i < to.TokenClassMembers.Count; i++)
									{
										MemberDescription member = to.TokenClassMembers[i];
										writer.WriteLine(",");
										writer.Write(i < from.TokenClassMembers.Count ? $"token.{member.Name.PascalCase}" : "default");
									}
								}
								writer.WriteLine(");");
							}
						}
						#endregion
					}

					if (to.Groups.Any())
					{
						foreach (SyntaxGroupInfo targetGroup in to.Groups.Values)
						{
							writer.WriteLine();

							SyntaxGroupInfo? group = targetGroup.Shadowed;
							Debug.Assert(group is not null);

							string groupKind = group!.Kind.CamelCase;
							string lastName = new Name(group.Name.Parts.Last()).CamelCase;

							using (writer.Region($"{targetGroup.Name.PascalNatural} methods"))
							{
								writer.WriteLine($"protected virtual {targetGroup.InterfaceName} Convert({group.InterfaceName} {groupKind})");
								using (writer.Braced())
								{
									writer.WriteLine($"return {groupKind} switch");
									using (writer.Braced(terminate: true))
									{
										foreach (SyntaxNodeInfo target in targetGroup.Nodes.Values.OrderBy(n => n.ClassMembers.Count))
										{
											Debug.Assert(target.Shadowed is not null);
											writer.WriteLine($"{target.Shadowed!.InterfaceName} {lastName} => Convert({lastName}),");
										}

										writer.WriteLine();
										writer.WriteLine($"_ => throw new ArgumentException($\"Unknown {lastName} node type ({{{groupKind}.GetType().Name}}).\", nameof({groupKind}))");
									}
								}
								foreach (SyntaxNodeInfo target in targetGroup.Nodes.Values.OrderBy(n => n.ClassMembers.Count))
								{
									SyntaxNodeInfo? origin = target.Shadowed;
									Debug.Assert(origin is not null);

									Generate(writer, origin!, target, canAutoImplementToken);
								}
							}
						}
					}

					IReadOnlyList<ListTypeDescription> listTypes = to.ListTypes;
					IReadOnlyList<SeparatedListTypeDescription> separatedListTypes = to.SeparatedListTypes;

					if (listTypes.Any() || separatedListTypes.Any())
					{
						using (writer.Region("List methods"))
						{
							foreach (ListTypeDescription listType in listTypes)
							{
								string fromType = listType.GetTargetType(from);
								string toType = listType.GetImplementationType(to);

								string fromValueType = from.GetTargetName(listType.ValueType.Original);
								string targetValueType = to.GetTargetName(listType.ValueType.Original);

								writer.WriteLine($"protected virtual {toType} Convert({fromType} list)");
								using (writer.Braced())
								{
									writer.WriteLine($"{targetValueType}[] values = new {targetValueType}[list.Count];");
									writer.WriteLine();
									writer.WriteLine($"for (int i = 0; i < list.Count; i++)");
									using (writer.Indented())
										writer.WriteLine($"values[i] = Convert(list[i]);");
									writer.WriteLine();
									writer.WriteLine("return new(values);");
								}
							}

							if (listTypes.Any() && separatedListTypes.Any())
								writer.WriteLine();

							foreach (SeparatedListTypeDescription listType in separatedListTypes)
							{
								string fromType = listType.GetTargetType(from);
								string toType = listType.GetImplementationType(to);

								string fromValueType = from.GetTargetName(listType.ValueType.Original);
								string fromSepType = from.GetTargetName(listType.SeparatorType.Original);

								string targetValueType = to.GetTargetName(listType.ValueType.Original);
								string targetSepType = to.GetTargetName(listType.SeparatorType.Original);

								writer.WriteLine($"protected virtual {toType} Convert({fromType} list)");
								using (writer.Braced())
								{
									writer.WriteLine($"ISyntaxNode[] nodes = new ISyntaxNode[list.Nodes.Count];");
									writer.WriteLine($"{targetValueType}[] values = new {targetValueType}[list.Values.Count];");
									writer.WriteLine($"{targetSepType}[] separators = new {targetSepType}[list.Separators.Count];");
									writer.WriteLine();
									writer.WriteLine($"for (int i = 0; i < list.Nodes.Count; i++)");
									using (writer.Braced())
									{
										writer.WriteLine("// This implementation preserves the original order of the nodes");
										writer.WriteLine("int valueIndex = list.Values.IndexOf(list.Nodes[i]);");
										writer.WriteLine("if (valueIndex >= 0)");
										using (writer.Braced())
										{
											writer.WriteLine($"{targetValueType} value = Convert(list.Values[i]);");
											writer.WriteLine("values[valueIndex] = value;");
											writer.WriteLine("nodes[i] = value;");
										}
										writer.WriteLine("else");
										using (writer.Braced())
										{
											writer.WriteLine("int sepIndex = list.Separators.IndexOf(list.Nodes[i]);");
											writer.WriteLine("if (sepIndex < 0)");
											using (writer.Indented())
												writer.WriteLine("throw new InvalidOperationException(\"Couldn't find a value or a separator index for the current node, is this an implementation problem somewhere?\");");
											writer.WriteLine();
											writer.WriteLine($"{targetSepType} separator = Convert(list.Separators[i]);");
											writer.WriteLine("separators[sepIndex] = separator;");
											writer.WriteLine("nodes[i] = separator;");
										}
									}
									writer.WriteLine();
									writer.WriteLine("return new(nodes, values, separators);");
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
