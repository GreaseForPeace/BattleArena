// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG
{
	[AddComponentMenu("")]
	public class ExecuteRandom : Action
	{
		public int startAction = 1;
		public int endAction = 1;

		public override void CopyTo(Action act)
		{
			base.CopyTo(act);
			ExecuteRandom a = act as ExecuteRandom;
			a.startAction = this.startAction;
			a.endAction = this.endAction;
		}

		public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
		{
			if (startAction < 1)
			{
				Debug.LogError("ExecuteRandom Action Error: The Range must start at (1) or higher.");
				return ReturnType.Done;
			}

			if (startAction > endAction)
			{
				Debug.LogError("ExecuteRandom Action Error: Start Range can't be bigger than End Range.");
				return ReturnType.Done;
			}

			int r = (startAction == endAction ? startAction : Random.Range(startAction, endAction+1));
			return (ReturnType.ExecuteSpecificNext + r);
		}

	}
}