// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class DelayAction : Action
{
	public NumericValue time = new NumericValue();
	private float timer = 0f;

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		DelayAction a = act as DelayAction;
		a.time = this.time.Copy();
		a.timer = this.timer;
	}

	public override void Init(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		timer = time.Value;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		timer -= Time.deltaTime;					// since Execute() is called by GameController.Update()
		if (timer <= 0f) return ReturnType.Done;	// this action is done
		return ReturnType.CallAgain;				// this action is still running
	}

	// ================================================================================================================
} }