using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

public interface IControlFlowOutgoingBranch : IControlFlowBranch
{
	#region Properties
	IControlFlowBlock To { get; }
	#endregion
}

public interface IMutableControlFlowOutgoingBranch : IControlFlowOutgoingBranch
{
	#region Properties
	new IMutableControlFlowBlock To { get; set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IControlFlowBlock IControlFlowOutgoingBranch.To => To;
	#endregion
}

public static class IControlFlowOutgoingBranchExtensions
{
	extension(IControlFlowOutgoingBranch branch)
	{
		#region Properties
		public IControlFlowBlock ActualTo
		{
			get
			{
				IControlFlowBlock to = branch.To;
				while (true)
				{
					if (to is IControlFlowMarkerBlock marker)
					{
						//Debug.Assert(marker.Outgoing.Count is 1);
						if (marker.Outgoing.Count is 1)
						{
							to = marker.Outgoing[0].To;
							continue;
						}
					}
					else if (to is IControlFlowConstructBlock construct)
					{
						if (construct.Outgoing.Count is 1)
						{
							to = construct.Outgoing[0].To;
							continue;
						}
					}

					break;
				}

				return to;
			}
		}
		#endregion
	}
}
