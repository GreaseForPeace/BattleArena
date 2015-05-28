// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class CameraSelectAction : Action
{
	public string camName = "";
	public GUID camId;

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		CameraSelectAction a = act as CameraSelectAction;
		a.camName = this.camName;
		a.camId = this.camId.Copy();
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		if (!UniRPGGlobal.Instance.SetActiveCamera(camId))
		{
			Debug.LogError("Camera Activate Action Error: The Camera was not found.");
		}
		return ReturnType.Done;
	}

	// ================================================================================================================
} }