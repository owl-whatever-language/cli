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
		IReadOnlyList<StructuredTreeInfo> order = StructuredTreeInfo.Convert(description);

		GenerateGlobal(context, order.Last());
		GenerateSyntaxNodeKind(context, order[0]);
		foreach (StructuredTreeInfo info in order)
		{
			GenerateToken(context, info.Token);
			GenerateBaseNode(context, info.BaseNode);
			GenerateTree(context, info);

			foreach (StructuredGroupInfo group in info.Groups)
				GenerateGroup(context, group);

			foreach (StructuredNodeInfo node in info.Nodes)
				GenerateNode(context, node);

			GenerateVisitor(context, info);
			GenerateConverter(context, info);
		}
		GenerateSyntaxBundle(context, order);
		GenerateShadowValidation(context, order);
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
	static void GenerateSyntaxNodeKind(SourceProductionContext context, StructuredTreeInfo tree)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);
		using (writer.Preamble(tree.RootNamespace))
		using (writer.TypePreamble())
		{
			writer.WriteLine("public enum SyntaxNodeEnum");
			using (writer.Braced())
			{
				writer.WriteLine($"Unknown = 0,");
				writer.WriteLine();
				writer.WriteLine("Token,");
				foreach (StructuredNodeInfo node in tree.Nodes.Where(n => n.Group is null))
				{
					writer.Write(node.NameWithGroup.Pascal);
					writer.WriteLine(",");
				}

				if (tree.Groups.Any())
				{
					foreach (StructuredGroupInfo group in tree.Groups)
					{
						writer.WriteLine();
						using (writer.Region(group.Name.Plural.Natural))
						{
							foreach (StructuredNodeInfo node in group.Nodes)
								writer.WriteLine($"{node.NameWithGroup.Pascal},");
						}
					}
				}
			}
		}

		string source = result.ToString();
		context.AddSource("SyntaxNodeEnum.g.cs", source);
	}
	static void GenerateSyntaxBundle(SourceProductionContext context, IReadOnlyList<StructuredTreeInfo> order)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		StructuredTreeInfo leastDetailed = order.First();
		StructuredTreeInfo mostDetailed = order.Last();

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


						writer.WriteLine($"{leastDetailed.Interface.Name}? LeastDetailed {{ get; }}");

						foreach (StructuredTreeInfo tree in order)
							writer.WriteLine($"{tree.Interface.Name}? {tree.Kind.Pascal} {{ get; }}");

						writer.WriteLine($"{mostDetailed.Interface.Name}? MostDetailed {{ get; }}");
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
						writer.WriteLine($"public {leastDetailed.Interface.Name}? LeastDetailed => {leastDetailed.Kind.Pascal};");

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

									StructuredTreeInfo? shadows = tree.Shadows;
									if (shadows is not null)
									{
										writer.WriteLine();
										writer.WriteLine("// re-use current as lower tree(s);");
										while (shadows is not null)
										{
											writer.WriteLine($"_{shadows.Kind.Camel} = value;");
											shadows = shadows.Shadows;
										}
									}

									StructuredTreeInfo? shadowedBy = tree.ShadowedBy;
									if (shadowedBy is not null)
									{
										writer.WriteLine();
										writer.WriteLine("// invalidate richer tree;");
										while (shadowedBy is not null)
										{
											writer.WriteLine($"_{shadowedBy.Kind.Camel} = null;");
											shadowedBy = shadowedBy.ShadowedBy;
										}
									}
								}
							}
						}

						writer.WriteLine($"public {mostDetailed.Interface.Name}? MostDetailed => {mostDetailed.Kind.Pascal};");
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

							writer.WriteLine($"private static void Validate{tree.Kind.Pascal}To{tree.Shadows.Kind.Pascal}()");
							using (writer.Braced())
							{
								writer.WriteLine($"{tree.Shadows.Token.Interface.Name}? n0 = ({tree.Token.Interface.Name}?)null;");
								for (int i = 0; i < tree.Nodes.Count; i++)
								{
									StructuredNodeInfo node = tree.Nodes[i];
									if (node.Shadows is null)
										throw new InvalidOperationException("A node with a shadow was expected.");

									writer.WriteLine($"{node.Shadows.Interface.Name}? n{i + 1} = ({node.Interface.Name}?)null;");
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
				using (writer.Region("Properties", lineAfter: true))
				{
					writer.WriteLine($"public override int Level => {token.Tree.Level};");
					writer.WriteLine($"protected override string? TreeKind => {$"\"{token.Kind.Original}\""};");
					writer.WriteLine("public SyntaxNodeEnum NodeEnum => SyntaxNodeEnum.Token;");

					if (token.ClassMembers.Any())
						writer.WriteLine();

					writer.WriteClassProperties(token.ClassMembers, skipRegion: true);
				}


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
				if (node.Shadows is null)
				{
					using (writer.Region("Properties"))
						writer.WriteLine($"SyntaxNodeEnum NodeEnum {{ get; }}");
				}
			}
			using (writer.ClassPreamble(node.Class))
			{
				using (writer.Region("Properties"))
				{
					writer.WriteLine($"public sealed override int Level => {node.Tree.Level};");
					writer.WriteLine($"public abstract SyntaxNodeEnum NodeEnum {{ get; }}");
				}
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
							string documentName = node.Tree.Document.Interface.Name;
							string treeName = node.Tree.Interface.Name;

							writer.WriteLine($"public IReadOnlyList<{tokenName}> Flatten() => node.Flatten<{tokenName}>();");
							writer.WriteLine($"public {documentName} GetDocument() => node.GetDocument<{documentName}>();");
							writer.WriteLine($"public {treeName} GetTree() => node.GetTree<{treeName}>();");
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
			using (writer.Region("Properties"))
			{
				writer.WriteLine($"new {document.Interface.Name} Document {{ get; }}");

				writer.WriteLine();
				writer.WriteLine("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
				if (document.Shadows is null)
					writer.WriteLine($"ISyntaxDocument ISyntaxTree.Document => Document;");
				else
				{
					Debug.Assert(info.Shadows is not null);
					writer.WriteLine($"{document.Shadows.Interface.Name} {info.Shadows!.Interface.Name}.Document => Document;");
				}
			}

			using (writer.ClassPreamble(info.Class))
			{
				using (writer.Region("Properties", true))
				{
					writer.WriteLine($"public override string Kind => \"{info.Kind.Original}\";");
					writer.WriteLine($"public override int Level => {info.Level};");
				}

				using (writer.Region("Constructors", lineAfter: true))
				{
					writer.Write($"public {info.Class.Name}(");
					writer.WriteSeparated([
						"ISourceFile source",
						$"{document.Interface.Name} document"
					]);
					writer.WriteLine(")");

					using (writer.Indented())
						writer.WriteLine(": base(source, document) { }");
				}
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

		bool isDocument = info.Name == "document";

		using (writer.Preamble(info.Namespace))
		{
			using (writer.InterfacePreamble(info.Interface))
			{
				if (isDocument)
				{
					using (writer.Region("Properties"))
					{
						writer.WriteLine("[DisallowNull]");
						writer.WriteLine($"new {info.Tree.Interface.Name}? Tree {{ get; set; }}");
						writer.WriteLine();

						writer.WriteLine("[DisallowNull]");
						if (info.Tree.Shadows is not null)
							writer.WriteLine($"{info.Tree.Shadows.Interface.Name} {info.Shadows!.Interface.Name}.Tree");
						else
							writer.WriteLine("ISyntaxTree? ISyntaxDocument.Tree");
						using (writer.Braced())
						{
							writer.WriteLine("get => Tree!; // I don't know why C# is complaining, both properties say they allow nullable.");
							writer.WriteLine($"set => Tree = ({info.Tree.Interface.Name})value;");
						}

						if (info.InterfaceMembers.Any())
							writer.WriteLine();

						writer.WriteInterfaceProperties(info.InterfaceMembers, true);
					}
				}
				else
					writer.WriteInterfaceProperties(info.InterfaceMembers);
			}

			using (writer.ClassPreamble(info.Class))
			{
				using (writer.Region("Properties", lineAfter: true))
				{
					if (isDocument)
					{
						writer.WriteLine("[DisallowNull]");
						writer.WriteLine($"public {info.Tree.Interface.Name}? Tree");
						using (writer.Braced())
						{
							writer.WriteLine("get => field;");
							writer.WriteLine("set");
							using (writer.Braced())
							{
								writer.WriteLine($"if (field is not null)");
								using (writer.Indented())
									writer.WriteLine($"throw new ArgumentException(\"The tree has already been assigned.\", nameof(value));");

								writer.WriteLine();
								writer.WriteLine("field = value;");
							}
						}
						writer.WriteLine();
					}

					string treeKind = $"\"{info.Kind.Original}\"";
					string nodeKind = $"\"{info.Name.Original}\"";
					string? groupKind = info.Group is null ? "null" : $"\"{info.Group.Name.Original}\"";

					writer.WriteLine($"public override SyntaxNodeKind NodeKind => new({treeKind}, {nodeKind}, {groupKind});");
					writer.WriteLine($"public override SyntaxNodeEnum NodeEnum => SyntaxNodeEnum.{info.NameWithGroup.Pascal};");

					if (info.ClassMembers.Any())
						writer.WriteLine();

					writer.WriteClassProperties(info.ClassMembers, skipRegion: true);
				}

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
					IEnumerable<string> syntaxMembers = info.ClassMembers.Where(m => m.Type is IStructuredSyntaxTypeInfo).Select(m => m.Name.Pascal);
					writer.WriteLine($"public override IEnumerable<ISyntaxNode> GetChildren() => [{string.Join(", ", syntaxMembers)}];");
				}
			}
		}

		string source = result.ToString();
		context.AddSource(info.Path, source);
	}

	static void GenerateConverter(SourceProductionContext context, StructuredTreeInfo target)
	{
		StructuredTreeInfo? from = target.Shadows;
		if (from is null)
			return;

		IndentedTextWriter writer = GetWriter(out StringWriter result);
		bool canAutoImplementToken = target.Token.ClassMembers.All(m => m.IsNullable);

		#region Generate node conversion
		static void Generate(IndentedTextWriter writer, StructuredNodeInfo from, StructuredNodeInfo to, bool canAutoImplement)
		{
			string variable = from.Kind.Camel;
			IReadOnlyList<StructuredMemberInfo> toMembers = to.ClassMembers.ToArray();
			IReadOnlyList<StructuredMemberInfo> fromMembers = from.ClassMembers.ToArray();

			if (toMembers.Count is 0 && fromMembers.Count is 0)
			{
				writer.WriteLine($"protected virtual {to.Class.Name} Convert({from.Interface.Name} {variable}) => new();");
				return;
			}

			if (canAutoImplement && toMembers.Count == fromMembers.Count)
			{
				writer.WriteLine($"protected virtual {to.Class.Name} Convert({from.Interface.Name} {variable})");
				using (writer.Braced())
				{
					foreach (StructuredMemberInfo member in to.ClassMembers)
					{
						if (member.Type is IStructuredSyntaxTypeInfo)
							writer.WriteLine($"{member.Type.TypeName} {member.Name.Camel} = Convert({variable}.{member.Name.Pascal});");
						else
							writer.WriteLine($"{member.Type.TypeName} {member.Name.Camel} = {variable}.{member.Name.Pascal};");
					}

					writer.WriteLine();
					writer.Write("return new(");
					writer.WriteSeparated(toMembers.Select(m => m.Name.Camel));
					writer.WriteLine(");");
				}

				return;
			}

			writer.WriteLine($"protected abstract {to.Class.Name} Convert({from.Interface.Name} {variable});");
		}
		#endregion

		using (writer.Preamble(target.Namespace))
		{
			using (writer.TypePreamble())
			{
				writer.WriteLine($"public abstract partial class {target.ConverterType}");
				using (writer.Braced())
				{
					using (writer.Region("Methods"))
					{
						writer.WriteLine($"public virtual {target.Class.Name} Convert({from.Interface.Name} tree)");
						using (writer.Braced())
						{
							writer.WriteLine($"{target.Document.Class.Name} {target.Document.Name.Camel} = Convert(tree.{from.Document.Name.Pascal});");
							writer.WriteLine($"return new(tree.Source, {target.Document.Name.Camel});");
						}

						foreach (StructuredNodeInfo node in target.Nodes.Where(n => n.Group is null).OrderBy(n => n.ClassMembers.Count()))
						{
							StructuredNodeInfo? origin = node.Shadows;
							Debug.Assert(origin is not null);

							Generate(writer, origin!, node, canAutoImplementToken);
						}
					}
					writer.WriteLine();
					using (writer.Region("Token methods"))
					{
						#region Full
						writer.Write($"protected virtual {target.Token.Class.Name} Convert(");
						writer.WriteSeparated([
							$"{from.Token.Interface.Name} token",
							"ClassificationKind? classification",
							.. target.Token.ClassMembers.Select(m => $"{m.Type.TypeName} {m.Name.Camel}")
						]);
						writer.WriteLine(")");
						using (writer.Braced())
						{
							writer.Write("return new(");
							writer.WriteSeparated([
								"token.Kind","token.Position","token.Lexeme","token.Value","token.LeadingTrivia","token.TrailingTrivia",
								"classification",
								.. target.Token.ClassMembers.Select(m => m.Name.Camel)
							]);
							writer.WriteLine(");");
						}
						#endregion

						writer.WriteLine();

						#region Inherit classification
						writer.Write($"protected virtual {target.Token.Class.Name} Convert(");
						writer.WriteSeparated([
							$"{from.Token.Interface.Name} token",
							..target.Token.ClassMembers.Select(m => $"{m.Type.TypeName} {m.Name.Camel}")
						]);
						writer.WriteLine(")");
						using (writer.Braced())
						{
							writer.Write("return Convert(");
							writer.WriteSeparated([
								"token","token.Classification",
								..target.Token.ClassMembers.Select(m => m.Name.Camel)
							]);
							writer.WriteLine(");");
						}
						#endregion

						#region Auto implementation
						if (canAutoImplementToken && target.Token.ClassMembers.Any())
						{
							writer.WriteLine();
							writer.WriteLine($"protected virtual {target.Token.Class.Name} Convert({from.Token.Interface.Name} token)");
							using (writer.Braced())
							{
								writer.Write("return Convert(");
								writer.WriteSeparated([
									"token","token.Classification",
									..target.Token.ClassMembers
										.Select(m1 => from.Token.ClassMembers.Any(m2 => m2.Name == m1.Name) ? $"token.{m1.Name.Pascal}" : "default")
								]);
								writer.WriteLine(");");
							}
						}
						#endregion
					}

					if (target.Groups.Any())
					{
						foreach (StructuredGroupInfo targetGroup in target.Groups)
						{
							writer.WriteLine();

							StructuredGroupInfo? group = targetGroup.Shadows;
							Debug.Assert(group is not null);

							string groupKind = group!.Kind.Camel;
							string lastName = new Name(group!.Name.Parts.Last()).Camel;

							using (writer.Region($"{targetGroup.Name.Natural} methods"))
							{
								writer.WriteLine($"protected virtual {targetGroup.Interface.Name} Convert({group.Interface.Name} {groupKind})");
								using (writer.Braced())
								{
									writer.WriteLine($"return {groupKind} switch");
									using (writer.Braced(terminate: true))
									{
										foreach (StructuredNodeInfo node in targetGroup.Nodes.OrderBy(n => n.ClassMembers.Count()))
										{
											Debug.Assert(node.Shadows is not null);
											writer.WriteLine($"{node.Shadows!.Interface.Name} {lastName} => Convert({lastName}),");
										}

										writer.WriteLine();
										writer.WriteLine($"_ => throw new ArgumentException($\"Unknown {lastName} node type ({{{groupKind}.GetType().Name}}).\", nameof({groupKind}))");
									}
								}
								foreach (StructuredNodeInfo node in targetGroup.Nodes.OrderBy(n => n.ClassMembers.Count()))
								{
									StructuredNodeInfo? origin = node.Shadows;
									Debug.Assert(origin is not null);

									Generate(writer, origin!, node, canAutoImplementToken);
								}
							}
						}
					}

					IReadOnlyList<StructuredMemberInfo> listMembers = target.GetListMembers();
					IReadOnlyList<StructuredMemberInfo> separatedListMembers = target.GetSeparatedListMembers();

					if (listMembers.Any() || separatedListMembers.Any())
					{
						writer.WriteLine();
						using (writer.Region("List methods"))
						{
							foreach (StructuredMemberInfo targetMember in listMembers)
							{
								StructuredListSyntaxTypeInfo targetList = (StructuredListSyntaxTypeInfo)targetMember.Type;
								StructuredListSyntaxTypeInfo fromList = (StructuredListSyntaxTypeInfo)targetMember.Shadows[0].Type;

								string fromType = fromList.InterfaceType;
								string toType = targetList.ImplementationType;

								string fromValueType = fromList.ValueType;
								string targetValueType = targetList.ValueType;

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

							if (listMembers.Any() && separatedListMembers.Any())
								writer.WriteLine();

							foreach (StructuredMemberInfo targetMember in separatedListMembers)
							{
								StructuredSeparatedListSyntaxTypeInfo targetList = (StructuredSeparatedListSyntaxTypeInfo)targetMember.Type;
								StructuredSeparatedListSyntaxTypeInfo fromList = (StructuredSeparatedListSyntaxTypeInfo)targetMember.Shadows[0].Type;

								string fromType = fromList.InterfaceType;
								string toType = targetList.ImplementationType;

								string fromValueType = fromList.ValueType;
								string targetValueType = targetList.ValueType;

								string fromSepType = fromList.SeparatorType;
								string targetSepType = targetList.SeparatorType;

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
											writer.WriteLine($"{targetValueType} value = Convert(list.Values[valueIndex]);");
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
											writer.WriteLine($"{targetSepType} separator = Convert(list.Separators[sepIndex]);");
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
		context.AddSource(target.ConverterPath, source);
	}
	static void GenerateVisitor(SourceProductionContext context, StructuredTreeInfo tree)
	{
		IndentedTextWriter writer = GetWriter(out StringWriter result);

		using (writer.Preamble(tree.Namespace))
		{
			using (writer.TypePreamble())
			{
				writer.WriteLine($"public abstract partial class {tree.VisitorType}");
				using (writer.Braced())
				{
					using (writer.Region("Methods", lineAfter: true))
					{
						writer.WriteLine($"public virtual void Visit({tree.Interface.Name} tree) => Dispatch(tree.{tree.Document.Name.Pascal});");

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
								foreach (StructuredNodeInfo node in tree.Nodes)
									writer.WriteLine($"{node.Interface.Name} n => Visit(n),");

								if (tree.Nodes.Any())
									writer.WriteLine();

								writer.WriteLine("ISyntaxTrivia trivia => Visit(trivia),");
								writer.WriteLine("TriviaList list => Visit(list),");
								writer.WriteLine($"{tree.Token.Interface.Name} token => Visit(token),");

								writer.WriteLine();
								writer.WriteLine("_ => VisitUnknown(node) // Used for syntax lists");
							}

							writer.WriteLine();
							writer.WriteLine($"if (visitChildren)");
							using (writer.Indented())
								writer.WriteLine($"VisitChildren(node);");
						}
					}
					using (writer.Region("Node methods"))
					{
						writer.WriteLine($"protected virtual bool Visit(ISyntaxTrivia trivia) => true;");
						writer.WriteLine($"protected virtual bool Visit(TriviaList list) => true;");
						writer.WriteLine($"protected virtual bool Visit({tree.Token.Interface.Name} token) => false; // Don't waste time visiting trivia nodes by default");
						if (tree.Nodes.Any())
							writer.WriteLine();

						foreach (StructuredNodeInfo node in tree.Nodes)
							writer.WriteLine($"protected virtual bool Visit({node.Interface.Name} node) => true;");

						writer.WriteLine();
						writer.WriteLine($"protected virtual bool VisitUnknown(ISyntaxNode node) => true;");
					}
				}
			}
		}

		string source = result.ToString();
		context.AddSource(tree.VisitorPath, source);
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
