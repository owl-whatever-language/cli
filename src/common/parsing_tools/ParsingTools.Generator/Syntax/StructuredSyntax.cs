namespace OwlDomain.ParsingTools.Generator.Syntax;

internal sealed record class SyntaxTreeInfo(
	string RootNamespace,
	Name Kind,
	TreeDescription Tree,
	TokenDescription Token,
	SyntaxTreeInfo? Shadowed,
	MemberDescriptionList Members,
	Dictionary<string, SyntaxGroupInfo> Groups,
	Dictionary<string, SyntaxNodeInfo> Nodes)
{
	#region Properties
	public string Namespace => $"{RootNamespace}.{PascalKind}";
	public string PascalKind => Kind.PascalCase;
	public string TreeName => $"{PascalKind}SyntaxTree";
	public string NodeName => $"{PascalKind}SyntaxNode";
	public string BaseNodeName => "Base" + NodeName;
	public string TokenName => $"{PascalKind}Token";
	public string ITreeName => "I" + TreeName;
	public string INodeName => "I" + NodeName;
	public string ITokenName => "I" + TokenName;

	public string Directory => PascalKind;
	public string TreePath => $"{Directory}/{TreeName}.g.cs";
	public string BaseNodePath => $"{Directory}/{BaseNodeName}.g.cs";
	public string TokenPath => $"{Directory}/{TokenName}.g.cs";
	public IReadOnlyList<string> Namespaces => GetAllNamespaces().Distinct().OrderBy(n => n).ToArray();
	public SyntaxNodeInfo Document => Nodes["document"];
	public MemberDescriptionList OnlyTokenMembers => Token.Members;
	public MemberDescriptionList AllTokenMembers => Shadowed is not null ? [.. Shadowed.AllTokenMembers, .. Members, .. Token.Members] : [.. Members, .. Token.Members];
	#endregion

	#region Functions
	public string GetTargetName(string key)
	{
		if (key is "token")
			return ITokenName;

		if (Groups.TryGetValue(key, out SyntaxGroupInfo group))
			return group.InterfaceName;

		return Nodes[key].InterfaceName;
	}
	private IEnumerable<string> GetAllNamespaces()
	{
		yield return RootNamespace;
		yield return Namespace;

		foreach (SyntaxGroupInfo group in Groups.Values)
			yield return group.Namespace;

		foreach (SyntaxNodeInfo node in Nodes.Values)
			yield return node.Namespace;

		if (Shadowed is not null)
		{
			foreach (string used in Shadowed.GetAllNamespaces())
				yield return used;
		}
	}
	#endregion
}

internal sealed record class SyntaxGroupInfo(
	SyntaxTreeInfo Tree,
	Name Kind,
	Name Name,
	SyntaxGroupInfo? Shadowed,
	MemberDescriptionList Members)
{
	#region Properties
	public string PascalKind => Kind.PascalCase;
	public string PascalName => Name.PascalCase;
	public string Namespace => $"{Tree.Namespace}.{PascalName}s";
	public string InterfaceName => $"I{PascalKind}{PascalName}Syntax";
	public string Directory => $"{Tree.Directory}/Nodes/{PascalName}s";
	public string InterfacePath => $"{Directory}/{InterfaceName}.g.cs";
	#endregion
}

internal sealed record class SyntaxNodeInfo(
	SyntaxTreeInfo Tree,
	SyntaxGroupInfo? Group,
	Name Kind,
	Name Name,
	SyntaxNodeInfo? Shadowed,
	MemberDescriptionList Members)
{
	#region Properties
	public string Namespace => Group is not null ? Group.Namespace : Tree.Namespace + ".Nodes";
	public string PascalKind => Kind.PascalCase;
	public string PascalName => Name.PascalCase;
	public string CamelName => Name.CamelCase;
	public string ClassName => Group is null ? $"{Tree.PascalKind}{PascalName}Syntax" : $"{Tree.PascalKind}{PascalName}{Group.PascalName}Syntax";
	public string InterfaceName => "I" + ClassName;
	public string Directory => Group is not null ? Group.Directory : $"{PascalKind}/Nodes";
	public string Path => $"{Directory}/{ClassName}.g.cs";
	public string BaseInterfaceName => Group is not null ? Group.InterfaceName : Tree.INodeName;
	public MemberDescriptionList AllMembers => [.. Tree.Members, .. Members];
	public MemberDescriptionList SyntaxMembers => AllMembers.Where(m => m.Type.IsSyntaxType).ToArray();
	#endregion
}
