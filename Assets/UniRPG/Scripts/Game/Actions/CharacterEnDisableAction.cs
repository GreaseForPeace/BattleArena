// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
	public class CharacterEnDisableAction : Action
{
	public int doWhat = 0; // 0:disable, 1:enable

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		CharacterEnDisableAction a = act as CharacterEnDisableAction;
		a.doWhat = this.doWhat;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
		if (obj != null)
		{
			CharacterBase chr = obj.GetComponent<CharacterBase>();
			if (chr)
			{
				chr.Actor.enabled = (doWhat == 1);
				chr.enabled = (doWhat == 1);
			}
			else Debug.LogError("Character EnDisable Action Error: The subject is not a Character type.");
		}
		else Debug.LogError("Character EnDisable Action Error: The subject did not exist.");
		return ReturnType.Done;
	}

	// ================================================================================================================
} }