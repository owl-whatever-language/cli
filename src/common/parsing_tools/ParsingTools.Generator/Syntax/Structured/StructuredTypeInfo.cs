namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

internal interface IStructuredTypeInfo
{
	#region Properties
	string TypeName { get; }
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
	#endregion

	#region Constructors
	public StructuredSyntaxTypeInfo(string interfaceType, string implementationType)
	{
		InterfaceType = interfaceType;
		ImplementationType = implementationType;
	}
	#endregion
}

internal sealed class StructuredListSyntaxTypeInfo : IStructuredSyntaxTypeInfo
{
	#region Properties
	public string InterfaceType { get; }
	public string ImplementationType { get; }
	public string ValueType { get; }
	string IStructuredTypeInfo.TypeName => InterfaceType;
	#endregion

	#region Constructors
	public StructuredListSyntaxTypeInfo(string valueType)
	{
		ValueType = valueType;
		ImplementationType = $"SyntaxList<{valueType}>";
		InterfaceType = "I" + ImplementationType;
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
	string IStructuredTypeInfo.TypeName => InterfaceType;
	#endregion

	#region Constructors
	public StructuredSeparatedListSyntaxTypeInfo(string valueType, string separatorType)
	{
		ValueType = valueType;
		SeparatorType = separatorType;

		ImplementationType = $"SyntaxList<{valueType}, {separatorType}>";
		InterfaceType = "I" + ImplementationType;
	}
	#endregion
}
