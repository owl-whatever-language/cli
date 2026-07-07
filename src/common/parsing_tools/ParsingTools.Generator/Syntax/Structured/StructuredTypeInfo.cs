namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

internal interface IStructuredTypeInfo
{
	#region Properties
	string TypeName { get; }
	bool IsNullable { get; }
	#endregion
}

internal interface IStructuredSyntaxTypeInfo : IStructuredTypeInfo
{
	#region Properties
	string InterfaceType { get; }
	string ImplementationType { get; }
	#endregion
}

internal sealed class StructuredTypeInfo : IStructuredTypeInfo
{
	#region Properties
	public string TypeName { get; }
	public bool IsNullable => TypeName.EndsWith("?");
	#endregion

	#region Constructors
	public StructuredTypeInfo(string typeName) => TypeName = typeName;
	#endregion
}

internal sealed class StructuredSyntaxTypeInfo : IStructuredSyntaxTypeInfo
{
	#region Properties
	public string InterfaceType { get; }
	public string ImplementationType { get; }
	string IStructuredTypeInfo.TypeName => InterfaceType;
	public bool IsNullable { get; }
	#endregion

	#region Constructors
	public StructuredSyntaxTypeInfo(string interfaceType, string implementationType, bool isNullable)
	{
		InterfaceType = interfaceType;
		ImplementationType = implementationType;
		IsNullable = isNullable;
	}
	#endregion
}

internal sealed class StructuredListSyntaxTypeInfo : IStructuredSyntaxTypeInfo
{
	#region Properties
	public string InterfaceType { get; }
	public string ImplementationType { get; }
	public string ValueType { get; }
	public bool IsNullable { get; }
	string IStructuredTypeInfo.TypeName => InterfaceType;
	#endregion

	#region Constructors
	public StructuredListSyntaxTypeInfo(string valueType, bool isNullable)
	{
		ValueType = valueType;
		ImplementationType = $"SyntaxList<{valueType}>";
		InterfaceType = "I" + ImplementationType;

		IsNullable = isNullable;
		if (isNullable)
		{
			ImplementationType += "?";
			InterfaceType += "?";
		}
	}
	#endregion
}

internal sealed class StructuredSeparatedListSyntaxTypeInfo : IStructuredSyntaxTypeInfo
{
	#region Properties
	public string InterfaceType { get; }
	public string ImplementationType { get; }
	public string ValueType { get; }
	public string SeparatorType { get; }
	public bool IsNullable { get; }
	string IStructuredTypeInfo.TypeName => InterfaceType;
	#endregion

	#region Constructors
	public StructuredSeparatedListSyntaxTypeInfo(string valueType, string separatorType, bool isNullable)
	{
		ValueType = valueType;
		SeparatorType = separatorType;

		ImplementationType = $"SyntaxList<{valueType}, {separatorType}>";
		InterfaceType = "I" + ImplementationType;

		IsNullable = isNullable;
		if (isNullable)
		{
			ImplementationType += "?";
			InterfaceType += "?";
		}
	}
	#endregion
}
