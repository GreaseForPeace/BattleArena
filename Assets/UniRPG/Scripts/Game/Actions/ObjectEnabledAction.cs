// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class ObjectEnabledAction : Action
{
	public int enable = 1; // 0:disable, 1:enable
	public bool setComponent = true;
	public string targetComponent = "";

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		ObjectEnabledAction a = act as ObjectEnabledAction;
		a.enable = this.enable;
		a.setComponent = this.setComponent;
		a.targetComponent = this.targetComponent;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
		if (obj != null)
		{
			if (setComponent)
			{
				if (!string.IsNullOrEmpty(targetComponent))
				{
					MonoBehaviour c = (MonoBehaviour)obj.GetComponent(targetComponent);
					if (c) c.enabled = (enable == 1);
					else Debug.LogError("Object Enable Action Error: The component was not found on the target object.");
				}
				else Debug.LogError("Object Enable Action Error: No component was specified.");
			}
			else
			{
				obj.SetActive(enable==1);
			}
		}
		else Debug.LogError("Object Enable Action Error: The subject did not exist.");
		return ReturnType.Done;
	}

	// ================================================================================================================
} }