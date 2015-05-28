// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class TargetingFlagAction : Action
{
	public bool allowTargeting = true;

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		TargetingFlagAction a = act as TargetingFlagAction;
		a.allowTargeting = this.allowTargeting;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper); 
		if (obj != null)
		{
			Interactable c = obj.GetComponent<Interactable>();
			if (c) c.canBeTargeted = allowTargeting;
			else Debug.LogError("Toggle canBeTargeted Action Error: The subject must be Interactable (for example an RPGObject).");
		}
		else Debug.LogError("Toggle canBeTargeted Action Error: The subject did not exist.");
		return ReturnType.Done;
	}

	// ================================================================================================================
} }