// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

#if UNIRPG_CORE

using UnityEngine;
using UniRPG;

namespace DiaQ
{
	[AddComponentMenu("")]
	public class DiaQDecisionAction : Action 
	{
		public DiaQDecision decision = new DiaQDecision();
		public int doOption = 0;	// what to do when test is TRUE; 0: skip next, 1: goto, 2: exit
		public int gotoAction = 1;	// the action to goto if doOption = 1

		public override void CopyTo(Action act)
		{
			base.CopyTo(act);
			DiaQDecisionAction a = act as DiaQDecisionAction;
			a.decision = this.decision.Copy();
			a.doOption = this.doOption;
			a.gotoAction = this.gotoAction;
		}

		public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
		{
			bool res = decision.Evaluate(null);

			if (res == true)
			{
				if (doOption == 0)
				{
					return ReturnType.SkipNext;
				}
				else if (doOption == 1)
				{
					if (gotoAction <= 0)
					{
						Debug.LogError("DiaQ Decision Action Error: You should enter an Action number of (1) or higher. Exiting Action Queue.");
						return ReturnType.Exit;
					}
					return (ReturnType.ExecuteSpecificNext + gotoAction);
				}
				else if (doOption == 2)
				{
					return ReturnType.Exit;
				}
			}

			return ReturnType.Done;
		}

		// ================================================================================================================
	}
}

#endif