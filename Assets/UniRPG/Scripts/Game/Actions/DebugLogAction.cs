// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class DebugLogAction : Action
{
	public int logType = 0; // 0:log, 1:warn, 2:error
	public bool inclNum = false;
	public bool inclObj = false;
	public StringValue text = new StringValue();
	public NumericValue num = new NumericValue(0f);
	public ObjectValue obj = new ObjectValue();

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		DebugLogAction a = act as DebugLogAction;
		a.logType = this.logType;
		a.inclNum = this.inclNum;
		a.inclObj = this.inclObj;
		a.text = this.text.Copy();
		a.num = this.num.Copy();
		a.obj = this.obj.Copy();
	}

	public override bool ExecuteWhenRestoringState()
	{
		return true;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		string o = "";
		if (inclObj)
		{
			if (obj.Value != null) o = obj.Value.ToString();
		}

		switch (logType)
		{
			case 0: Debug.Log(text.Value + " " + (inclNum ? num.Value.ToString() : "") + o); break;
			case 1: Debug.LogWarning(text.Value + " " + (inclNum ? num.Value.ToString() : "") + o); break;
			case 2: Debug.LogError(text.Value + " " + (inclNum ? num.Value.ToString() : "") + o); break;
		}
		return ReturnType.Done;
	}

	// ================================================================================================================
} }