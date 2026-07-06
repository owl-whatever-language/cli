using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

public interface IControlFlowIncomingBranch : IControlFlowBranch
{
	#region Properties
	IControlFlowBlock From { get; }
	#endregion
}

public interface IMutableControlFlowIncomingBranch : IControlFlowIncomingBranch
{
	#region Properties
	new IMutableControlFlowBlock From { get; set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IControlFlowBlock IControlFlowIncomingBranch.From => From;
	#endregion
}

public static class IControlFlowIncomingBranchExtensions
{
	extension(IControlFlowIncomingBranch branch)
	{
		#region Properties
		public IControlFlowBlock ActualFrom
		{
			get
			{
				IControlFlowBlock from = branch.From;
				while (true)
				{
					if (from is IControlFlowMarkerBlock marker)
					{
						if (marker.Incoming.Count is 1)
						{
							from = marker.Incoming[0].From;
							continue;
						}
					}
					else if (from is IControlFlowConstructBlock construct)
					{
						if (construct.Incoming.Count is 1)
						{
							from = construct.Incoming[0].From;
							continue;
						}
					}

					break;
				}

				return from;
			}
		}
		#endregion
	}
}
