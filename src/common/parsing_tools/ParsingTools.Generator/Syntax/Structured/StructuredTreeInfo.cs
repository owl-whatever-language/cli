namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

internal sealed class StructuredTreeInfo : IStructuredShadowedInfo<StructuredTreeInfo>
{
	#region Fields
	private StructuredNodeInfo? _document;
	#endregion

	#region Properties
	public Name Kind { get; }
	public string RootNamespace { get; }
	public string Namespace { get; }
	public StructuredTokenInfo Token { get; }
	public StructuredTreeNodeInfo BaseNode { get; }
	public StructuredInterfaceInfo Interface { get; }
	public StructuredClassInfo Class { get; }
	public IReadOnlyList<StructuredMemberInfo> Members { get; }
	public StructuredTreeInfo? Shadows { get; }
	public StructuredTreeInfo? ShadowedBy { get; private set; }
	public List<StructuredNodeInfo> Nodes { get; } = [];
	public List<StructuredGroupInfo> Groups { get; } = [];
	public IReadOnlyCollection<StructuredNodeInfo> DirectNodes => Nodes.Where(n => n.Group is null).ToArray();
	public StructuredNodeInfo Document => _document ??= DirectNodes.Single(n => n.Name == "document");
	public IReadOnlyList<string> Namespaces => GetAllNamespaces().Distinct().OrderBy(n => n).ToArray();
	public string Directory => Kind.Pascal;
	public string TreePath => $"{Directory}/{Class.Name}.g.cs";
	public SyntaxDescriptionFile Descriptions { get; }
	#endregion

	#region Constructors
	public StructuredTreeInfo(
		SyntaxDescriptionFile descriptions,
		Name kind,
		string rootNamespace,
		string @namespace,
		StructuredTokenInfo token,
		StructuredTreeNodeInfo baseNode,
		StructuredInterfaceInfo @interface,
		StructuredClassInfo @class,
		IReadOnlyList<StructuredMemberInfo> members,
		StructuredTreeInfo? shadowed)
	{
		Descriptions = descriptions;
		Kind = kind;
		RootNamespace = rootNamespace;
		Namespace = @namespace;

		Token = token;
		token.Tree = this;

		BaseNode = baseNode;
		baseNode.Tree = this;

		Interface = @interface;
		Class = @class;

		Members = members;

		Shadows = shadowed;
		shadowed?.ShadowedBy = this;
	}
	#endregion

	#region Methods
	private IEnumerable<string> GetAllNamespaces()
	{
		yield return RootNamespace;
		yield return Namespace;

		foreach (StructuredGroupInfo group in Groups)
			yield return group.Namespace;

		foreach (StructuredNodeInfo node in Nodes)
			yield return node.Namespace;

		if (Shadows is not null)
		{
			foreach (string used in Shadows.GetAllNamespaces())
				yield return used;
		}
	}
	public IStructuredTypeInfo GetTargetName(Name key) => GetTargetName(key.Original);
	public IStructuredTypeInfo GetTargetName(string key)
	{
		if (key is "token")
			return new StructuredSyntaxTypeInfo(Token.Interface.Name, Token.Class.Name);

		if (key.EndsWith("?") && Name.Keywords.Contains(key.Substring(0, key.Length - 1)))
			return new StructuredTypeInfo(key);

		if (Name.Keywords.Contains(key))
			return new StructuredTypeInfo(key);

		Name keyName = new(key);
		StructuredGroupInfo? group = Groups.FirstOrDefault(g => g.MatchesName(keyName));
		if (group is not null)
			return new StructuredSyntaxTypeInfo(group.Interface.Name, group.Interface.Name);

		StructuredNodeInfo? node = Nodes.FirstOrDefault(n => n.MatchesName(keyName));
		if (node is not null)
			return new StructuredSyntaxTypeInfo(node.Interface.Name, node.Class.Name);

		return new StructuredTypeInfo(key);
	}
	#endregion

	#region Functions
	public static void Create(
		TreeDescription description,
		StructuredTreeInfo? lastTree,
		out StructuredInterfaceInfo treeInterface,
		out StructuredClassInfo treeClass)
	{
		Name kind = description.Kind;

		treeInterface = new(
			$"I{kind.Pascal}SyntaxTree",
			[lastTree is null ? "ISyntaxTree" : lastTree.Interface.Name],
			lastTree?.Interface);

		treeClass = new(
			$"{kind.Pascal}SyntaxTree",
			ClassType.Sealed,
			["BaseSyntaxTree", treeInterface.Name],
			lastTree?.Class);
	}
	public static IReadOnlyList<StructuredTreeInfo> Convert(SyntaxDescriptionFile description)
	{
		List<StructuredTreeInfo> order = [];

		string? rootNamespace = description.OfType<NamespaceDescription>().FirstOrDefault().RootNamespace;
		if (rootNamespace is null)
			return order;

		ShadowingDescription? shadowing = description.OfType<ShadowingDescription>().FirstOrDefault();
		if (shadowing is null || shadowing.Order.Count is 0)
			return order;

		List<StructuredTreeInfo> trees = [];

		foreach (Name kind in shadowing.Order)
		{
			TreeDescription tree = description.OfType<TreeDescription>().Single(d => d.Kind == kind);
			TokenDescription token = description.OfType<TokenDescription>().Single(d => d.Kind == kind);
			NodeDescription document = description.OfType<NodeDescription>().Single(d => d.Name == "document" && d.Kind == "node");

			StructuredTreeInfo? lastTree = trees.LastOrDefault();

			StructuredTreeNodeInfo baseNode = CreateBaseNode(tree, lastTree);
			StructuredTokenInfo tokenInfo = CreateToken(token, lastTree, baseNode);
			Create(tree, lastTree, out StructuredInterfaceInfo treeInterface, out StructuredClassInfo treeClass);

			List<StructuredMemberInfo> members = [
				new(
					document.Name,
					treeInterface.Name,
					new StructuredTypeInfo($"I{kind.Pascal}{document.Name.Pascal}Syntax"),
					[lastTree?.Members.Single(m => m.Name == document.Name)]
				)
			];

			string @namespace = rootNamespace + "." + kind.Pascal;
			StructuredTreeInfo info = new(description, kind, rootNamespace, @namespace, tokenInfo, baseNode, treeInterface, treeClass, members, lastTree);
			trees.Add(info);
		}

		foreach (StructuredTreeInfo tree in trees)
		{
			foreach (GroupDescription group in description.OfType<GroupDescription>())
			{
				OnKindDescription? modifier = description.GetModifier(tree.Kind, group.Name);
				OnKindDescription? shadow = tree.Shadows is null ? null : description.GetModifier(tree.Shadows.Kind, group.Name);

				PopulateGroups(group, modifier, shadow, tree);
			}
		}

		foreach (StructuredTreeInfo tree in trees)
		{
			foreach (NodeDescription node in description.OfType<NodeDescription>())
			{
				OnKindDescription? modifier = description.GetModifier(tree.Kind, node.Name);
				if (modifier is not null && tree.Groups.Any(g => g.MatchesName(node.Name)))
					modifier = null;

				modifier ??= description.GetModifier(tree.Kind, new(node.Name, node.Kind));

				PopulateNodes(node, modifier, tree);
			}
		}

		return trees;
	}
	private static void PopulateGroups(GroupDescription description, OnKindDescription? modifier, OnKindDescription? shadowModifier, StructuredTreeInfo tree)
	{
		Name name = description.Name;
		Name kind = tree.Kind;

		StructuredGroupInfo? shadows = tree.Shadows?.Groups.FirstOrDefault(g => g.Name == name);

		List<StructuredMemberInfo> members = [];

		StructuredInterfaceInfo @interface = new(
			$"I{kind.Pascal}{name.Pascal}Syntax",
			[tree.BaseNode.Interface.Name, shadows?.Interface.Name],
			shadows?.Interface);

		List<MemberDescription> fromModifiers =
		[
			..modifier?.Members??[],
			..shadowModifier?.Members ?? []
		];

		foreach (MemberDescription member in description.Members.Concat(fromModifiers))
		{
			IStructuredTypeInfo type = CreateType(tree, member.Type);
			IEnumerable<StructuredMemberInfo?> memberShadows =
			[
				shadows?.Members.FirstOrDefault(m => m.Name == member.Name),
			];

			StructuredMemberInfo info = new(member.Name, @interface.Name, type, memberShadows);
			members.Add(info);
		}

		string @namespace = $"{tree.Namespace}.{name.Plural.Pascal}";
		StructuredGroupInfo group = new(name, tree, @namespace, @interface, members, shadows);

		tree.Groups.Add(group);
	}
	private static void PopulateNodes(NodeDescription description, OnKindDescription? modifier, StructuredTreeInfo tree)
	{
		//public Name NameWithGroup => _nameWithGroup ??= new(Name, Group?.Name ?? default);
		//public Name NameWithTree => _nameWithTree ??= new(Tree.Kind, NameWithGroup);

		Name name = description.Name;

		StructuredGroupInfo? group = tree.Groups.FirstOrDefault(g => g.MatchesName(description.Kind));

		Name nameWithGroup = new(name, group?.Name);
		Name nameWithTree = new(tree.Kind, nameWithGroup);

		StructuredNodeInfo? shadows =
		group?.Shadows?.Nodes.FirstOrDefault(n => n.MatchesName(name, nameWithGroup, nameWithTree)) ??
			tree.Shadows?.Nodes.FirstOrDefault(n => n.MatchesName(name, nameWithGroup, nameWithTree));

		if (shadows is null && tree.Shadows is not null)
			throw new InvalidOperationException("Tried to populate a node without a shadow, when the tree had one.");

		List<StructuredMemberInfo> members = [];

		string @namespace = group?.Namespace ?? tree.Namespace;

		string coreName = $"{tree.Kind.Pascal}{name.Pascal}{group?.Name.Pascal}Syntax";

		StructuredInterfaceInfo @interface = new(
			"I" + coreName,
			[
				group is not null ? group.Interface.Name : tree.BaseNode.Interface.Name,
				shadows?.Interface.Name
			],
			shadows?.Interface);

		StructuredClassInfo @class = new(
			coreName,
			ClassType.Abstract,
			[tree.BaseNode.Class.Name, @interface.Name],
			shadows?.Class);


		foreach (MemberDescription member in description.Members.Concat(modifier?.Members ?? []))
		{
			IStructuredTypeInfo type = CreateType(tree, member.Type);
			IEnumerable<StructuredMemberInfo?> memberShadows =
			[
				group?.Members.FirstOrDefault(m => m.Name == member.Name),
				shadows?.Members.FirstOrDefault(m => m.Name == member.Name),
			];

			StructuredMemberInfo info = new(member.Name, @interface.Name, type, memberShadows);
			members.Add(info);
		}

		StructuredNodeInfo node = new(name, tree, group, @namespace, @interface, @class, members, shadows);

		group?.Nodes.Add(node);
		tree.Nodes.Add(node);
	}
	private static IStructuredTypeInfo CreateType(StructuredTreeInfo tree, ITypeDescription description)
	{
		if (description is ListTypeDescription list)
		{
			string valueType = tree.GetTargetName(list.ValueType).TypeName;
			return new StructuredListSyntaxTypeInfo(valueType);
		}

		if (description is SeparatedListTypeDescription separated)
		{
			string valueType = tree.GetTargetName(separated.ValueType).TypeName;
			string separatorType = tree.GetTargetName(separated.SeparatorType).TypeName;
			return new StructuredSeparatedListSyntaxTypeInfo(valueType, separatorType);
		}

		if (description is TypeDescription type)
			return tree.GetTargetName(type.TargetType);

		return new StructuredTypeInfo("???");
	}
	private static IStructuredTypeInfo CreateType(ITypeDescription description)
	{
		if (description is TypeDescription type)
		{
			return new StructuredTypeInfo(type.TargetType);
		}

		return new StructuredTypeInfo("???");
	}
	public static StructuredTokenInfo CreateToken(
		TokenDescription description,
		StructuredTreeInfo? lastTree,
		StructuredTreeNodeInfo baseNode)
	{
		Name kind = description.Kind;

		List<StructuredMemberInfo> members = [];

		StructuredInterfaceInfo tokenInterface = new(
			$"I{kind.Pascal}Token",
			[baseNode.Interface.Name, lastTree is null ? "ISyntaxToken" : lastTree.Token.Interface.Name],
			lastTree?.Token.Interface);

		StructuredClassInfo tokenClass = new(
			$"{kind.Pascal}Token",
			ClassType.Sealed,
			["BaseSyntaxToken", tokenInterface.Name],
			lastTree?.Token.Class);

		foreach (MemberDescription member in description.Members)
		{
			IStructuredTypeInfo type = CreateType(member.Type);
			IEnumerable<StructuredMemberInfo?> memberShadows =
			[
				lastTree?.Token.Members.FirstOrDefault(m => m.Name == member.Name),
			];

			StructuredMemberInfo info = new(member.Name, tokenInterface.Name, type, memberShadows);
			members.Add(info);
		}


		return new(tokenInterface, tokenClass, members, lastTree?.Token);
	}
	public static StructuredTreeNodeInfo CreateBaseNode(TreeDescription description, StructuredTreeInfo? lastTree)
	{
		Name kind = description.Kind;

		List<StructuredMemberInfo> members = [];

		StructuredInterfaceInfo baseNodeInterface = new(
			$"I{kind.Pascal}SyntaxNode",
			[lastTree is null ? "ISyntaxNode" : lastTree.BaseNode.Interface.Name],
			lastTree?.BaseNode.Interface);

		StructuredClassInfo baseNodeClass = new(
			$"Base{kind.Pascal}SyntaxNode",
			ClassType.Abstract,
			["BaseSyntaxNode", baseNodeInterface.Name],
			lastTree?.BaseNode.Class);

		foreach (MemberDescription member in description.Members)
		{
			IStructuredTypeInfo type = CreateType(member.Type);
			IEnumerable<StructuredMemberInfo?> memberShadows =
			[
				lastTree?.Token.Members.FirstOrDefault(m => m.Name == member.Name),
			];

			StructuredMemberInfo info = new(member.Name, baseNodeInterface.Name, type, memberShadows);
			members.Add(info);
		}

		return new(baseNodeInterface, baseNodeClass, members, lastTree?.BaseNode);
	}
	#endregion
}
