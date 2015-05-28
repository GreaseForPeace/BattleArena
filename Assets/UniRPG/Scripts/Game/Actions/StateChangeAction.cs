// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class StateChangeAction : Action
{
	public bool setState = true; // else clear
	public RPGState state = null;

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		StateChangeAction a = act as StateChangeAction;
		a.setState = this.setState;
		a.state = this.state;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		if (state == null)
		{
			Debug.LogError("State Change Action Error: The State is (NULL).");
			return ReturnType.Done;
		}

		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
		if (obj != null)
		{
			Actor actor = obj.GetComponent<Actor>();
			if (actor)
			{
				if (setState) actor.AddState(state);
				else actor.RemoveState(state);
			}
			else Debug.LogError("State Change Action Error: The subject must be an Actor.");
		}
		else Debug.LogError("State Change Action Error: The subject did not exist.");
		return ReturnType.Done;
	}

	// ================================================================================================================
} }