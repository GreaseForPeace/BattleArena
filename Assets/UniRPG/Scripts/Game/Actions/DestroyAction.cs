// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class DestroyAction : Action
{
	public float afterTimeout = 0f; // seconds

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		DestroyAction a = act as DestroyAction;
		a.afterTimeout = this.afterTimeout;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper); 
		if (obj != null)
		{
			GameObject.Destroy(obj, afterTimeout);
		}
		else Debug.LogError("Destroy Action Error: The subject did not exist.");
		return ReturnType.Done;
	}

	// ================================================================================================================
} }