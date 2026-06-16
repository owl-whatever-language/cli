namespace OwlDomain.ParsingTools.Generator.Syntax;

internal sealed record class MemberDescription(ITypeDescription Type, Name Name);

internal interface ITypeDescription
{
	#region Properties
	string TargetType { get; }
	bool IsSyntaxType { get; }
	#endregion

	#region Methods
	string GetTargetType(SyntaxNodeInfo node);
	#endregion
}
internal sealed record class TypeDescription(Name Type) : ITypeDescription
{
	public string TargetType => Type.Original;
	public bool IsSyntaxType
	{
		get
		{
			if (Type.Original is "string" or "string?")
				return false;

			return Type.Original == Type.Original.ToLower();
		}
	}
	public string GetTargetType(SyntaxNodeInfo node)
	{
		if (IsSyntaxType is false)
			return TargetType;

		string name = node.Tree.GetTargetName(Type.Original);
		return name;
	}
}

internal sealed record class ListTypeDescription(Name ValueType) : ITypeDescription
{
	public string TargetType => $"SyntaxList<{ValueType.PascalCase}>";
	public bool IsSyntaxType => true;

	public string GetTargetType(SyntaxNodeInfo node)
	{
		string valueType = node.Tree.GetTargetName(ValueType.Original);
		return $"ISyntaxList<{valueType}>";
	}
}
internal sealed record class SeparatedListTypeDescription(Name ValueType, Name SeparatorType) : ITypeDescription
{
	public string TargetType => $"SyntaxList<{ValueType.PascalCase}, {SeparatorType.PascalCase}>";
	public bool IsSyntaxType => true;

	public string GetTargetType(SyntaxNodeInfo node)
	{
		string valueType = node.Tree.GetTargetName(ValueType.Original);
		string sepType = node.Tree.GetTargetName(SeparatorType.Original);

		return $"ISyntaxList<{valueType}, {sepType}>";
	}
}


internal interface ISyntaxDescription { }
internal sealed record class NamespaceDescription(string RootNamespace) : ISyntaxDescription;
internal sealed record class ShadowingDescription(IReadOnlyList<Name> Order) : ISyntaxDescription;
internal sealed record class TokenDescription(Name Kind, MemberDescriptionList Members) : ISyntaxDescription;
internal sealed record class TreeDescription(Name Kind, MemberDescriptionList Members) : ISyntaxDescription;
internal sealed record class GroupDescription(Name Name, MemberDescriptionList Members) : ISyntaxDescription;
internal sealed record class OnKindDescription(Name Name, Name Kind, MemberDescriptionList Members) : ISyntaxDescription;
internal sealed record class NodeDescription(Name Name, Name Kind, MemberDescriptionList Members) : ISyntaxDescription;

internal sealed record class SyntaxDescriptionFile(IReadOnlyList<ISyntaxDescription> Descriptions) : IReadOnlyList<ISyntaxDescription>
{
	#region Properties
	public int Count => Descriptions.Count;
	#endregion

	#region Indexers
	public ISyntaxDescription this[int index] => Descriptions[index];
	#endregion

	#region Methods
	public IEnumerator<ISyntaxDescription> GetEnumerator() => Descriptions.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	#endregion

	#region Functions
	internal static SyntaxDescriptionFile? Parse(SourceText text)
	{
		List<ISyntaxDescription> descriptions = [];

		foreach (TextLine line in text.Lines)
		{
			string lineText = line.ToString().Trim();

			if (lineText.IsWhiteSpace())
				continue;

			if (lineText.StartsWith("#"))
				continue;

			string[] parts = lineText.Split([':'], StringSplitOptions.None);
			if (parts.Length is 0)
				continue;

			string declaration = parts[0].Trim();
			string members = parts.Length >= 2 ? parts[1].Trim() : "";
			string shadow = parts.Length >= 3 ? parts[2].Trim() : "";

			ISyntaxDescription? description = Parse(declaration, members, shadow);
			if (description is not null)
				descriptions.Add(description);
		}

		return new(descriptions);
	}
	private static ISyntaxDescription? Parse(string declaration, string members, string shadow)
	{
		string[] declarationParts = Split(declaration, ' ');

		if (declaration is "namespace")
			return new NamespaceDescription(members);

		string declaration1 = declarationParts[0];
		string declaration2 = declarationParts.Length >= 2 ? declarationParts[1] : "";
		string declaration3 = declarationParts.Length >= 3 ? declarationParts[2] : "";

		if (declaration1 is "shadow")
		{
			Name[] order = Split(members, ' ').Select(m => new Name(m)).ToArray();
			return new ShadowingDescription(order);
		}

		MemberDescriptionList memberList = ParseMembers(members);

		if (declaration1 is "token")
			return new TokenDescription(declaration2, memberList);

		if (declaration1 is "tree")
			return new TreeDescription(declaration2, memberList);

		if (declaration1 is "group")
			return new GroupDescription(declaration2, memberList);

		if (declaration1 is "document")
			return new NodeDescription("document", "node", memberList);

		if (declaration2 is "@")
			return new OnKindDescription(declaration1, declaration3, memberList);

		return new NodeDescription(declaration1, declaration2, memberList);
	}
	private static MemberDescriptionList ParseMembers(string text)
	{
		List<MemberDescription> members = [];

		while (text.IsWhiteSpace() is false)
			text = ParseNextMember(members, text);

		return members;
	}
	private static string ParseNextMember(List<MemberDescription> container, string text)
	{
		int open = text.IndexOf('<');
		int close = text.IndexOf('>');
		int comma = text.IndexOf(',');

		if (comma < 0)
			comma = text.Length;

		if (comma > open && comma < close)
			comma = text.IndexOf(',', close);

		string memberPart = text.Substring(0, comma);
		int space = memberPart.LastIndexOf(' ');

		string typeText = memberPart.Substring(0, space).Trim();
		string name = memberPart.Substring(space + 1).Trim();

		ITypeDescription type = ParseType(typeText);
		container.Add(new(type, name));

		if (comma == text.Length || comma == text.Length - 1)
			return "";

		return text.Substring(comma + 1).Trim();
	}
	private static ITypeDescription ParseType(string text)
	{
		if (text.StartsWith("list") is false)
			return new TypeDescription(text);

		int open = text.IndexOf('<');
		int close = text.IndexOf('>');

		string typesText = text.Substring(open + 1, close - (open + 1));
		string[] types = Split(typesText, ',');

		return types.Length switch
		{
			1 => new ListTypeDescription(types[0].Trim()),
			2 => new SeparatedListTypeDescription(types[0].Trim(), types[1].Trim()),

			_ => new TypeDescription(text.Trim())
		};
	}
	#endregion

	#region Helpers
	private static string[] Split(string text, char ch)
	{
		List<string> fragments = [];
		string[] parts = text.Split(ch);

		foreach (string part in parts)
		{
			string trimmed = part.Trim();
			if (trimmed.IsWhiteSpace() is false)
				fragments.Add(part);
		}

		return fragments.ToArray();
	}
	#endregion
}
