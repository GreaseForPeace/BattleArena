// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class CurrencyAction : Action
{
	public int doWhat = 0; // 0:set, 1:add, 2:subtract
	public NumericValue amount = new NumericValue();

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		CurrencyAction a = act as CurrencyAction;
		a.doWhat = this.doWhat;
		a.amount = this.amount.Copy();
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
		if (obj != null)
		{
			Actor actor = obj.GetComponent<Actor>();
			if (actor)
			{
				if (doWhat == 0) actor.currency = (int)amount.Value;
				else if (doWhat == 1) actor.currency += (int)amount.Value;
				else if (doWhat == 2) actor.currency -= (int)amount.Value;
			}
			else Debug.LogError("Currency Action Error: The subject must be an Actor.");
		}
		else Debug.LogError("Currency Action Error: The subject did not exist.");
			
		return ReturnType.Done;
	}

	// ================================================================================================================
} }