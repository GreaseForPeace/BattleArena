// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class StopExeAction : Action
{
	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		return ReturnType.Exit; // exit the action queue now
	}

	// ================================================================================================================
} }