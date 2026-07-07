namespace OwlDomain.ParsingTools.Syntax.Debugging;

public interface IDebugNodeFactory
{
	#region Methods
	IDebugTreeNode GetDebugNode();
	#endregion
}

public interface IDebugNodeFactory<out T> : IDebugNodeFactory
where T : notnull, IDebugTreeNode
{
	#region Methods
	new T GetDebugNode();
	IDebugTreeNode IDebugNodeFactory.GetDebugNode() => GetDebugNode();
	#endregion
}

public interface IDebugTreeFactory : IDebugNodeFactory<IDebugTree>
{
	#region Methods
	IDebugTree GetDebugTree();
	IDebugTreeNode IDebugNodeFactory.GetDebugNode() => GetDebugTree();
	IDebugTree IDebugNodeFactory<IDebugTree>.GetDebugNode() => GetDebugTree();
	#endregion
}

public interface IDebugTextFactory : IDebugNodeFactory<IDebugTreeText>
{
	#region Methods
	public TextFragmentCollection GetDebugText();
	IDebugTreeNode IDebugNodeFactory.GetDebugNode()
	{
		TextFragmentCollection fragments = GetDebugText();
		return new DebugTreeText(fragments);
	}
	IDebugTreeText IDebugNodeFactory<IDebugTreeText>.GetDebugNode()
	{
		TextFragmentCollection fragments = GetDebugText();
		return new DebugTreeText(fragments);
	}
	#endregion
}

public interface IDebugListFactory : IDebugNodeFactory<IDebugTreeList>
{
	#region Methods
	IDebugTreeList GetDebugList();
	IDebugTreeNode IDebugNodeFactory.GetDebugNode() => GetDebugList();
	IDebugTreeList IDebugNodeFactory<IDebugTreeList>.GetDebugNode() => GetDebugList();
	#endregion
}

public interface IDebugObjectFactory : IDebugNodeFactory<IDebugTreeObject>
{
	#region Methods
	IDebugTreeObject GetDebugObject();
	IDebugTreeNode IDebugNodeFactory.GetDebugNode() => GetDebugObject();
	IDebugTreeObject IDebugNodeFactory<IDebugTreeObject>.GetDebugNode() => GetDebugObject();
	#endregion
}
