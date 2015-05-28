// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class CustomVarAction : Action
{
	public string varName = "";

	public StringValue str = new StringValue();

	// the following are only used when varType != 2
	public int setFrom = 0; // 0: string var, 1: num var, 2: attribute, 3:level, 4:currency, 5:customvar
	public NumericValue num = new NumericValue();
	public GUID attId = new GUID();
	public int attWhat = 0; // 0:val, 1:min, 2:max

	// the following are only used when varType == 2
	public int objSet = 0; // 0:to subject, 1:subject child, 2:obj tagged, 3:obj named

	// I use "subject" in the values, so add a new "subject" here
	public ActionTarget customVariableOwner = new ActionTarget();

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		CustomVarAction a = act as CustomVarAction;
		a.customVariableOwner = this.customVariableOwner.Copy();
		a.varName = this.varName;
		a.setFrom = this.setFrom;
		a.num = this.num.Copy();
		a.str = this.str.Copy();
		a.attId = this.attId.Copy();
		a.attWhat = this.attWhat;
		a.objSet = this.objSet;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		if (string.IsNullOrEmpty(varName))
		{
			Debug.LogError("CustomVar Action Error: The variable name is not set.");
			return ReturnType.Done;
		}

		if (varName.Contains("|"))
		{
			Debug.LogError("CustomVar Action Error: The variable name is invalid. '|' is not allowed.");
			return ReturnType.Done;
		}

		GameObject obj = DetermineTarget(customVariableOwner, self, targeted, selfTargetedBy, equipTarget, helper);
		if (obj == null)
		{
			Debug.LogError("CustomVar Action Error: The subject did not exist.");
			return ReturnType.Done;
		}

		UniqueMonoBehaviour b = obj.GetComponent<UniqueMonoBehaviour>();
		if (b == null)
		{
			Debug.LogError("CustomVar Action Error: The subject does not support custom variables.");
			return ReturnType.Done;
		}

		if (setFrom == 0) 
		{
			b.SetCustomVariable(varName, str.Value);
		}
		else if (setFrom == 1)
		{
			b.SetCustomVariable(varName, num.Value.ToString());
		}

		// setting from attribute/level/currency (require actor)
		else if (setFrom == 2 || setFrom == 3)
		{
			obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
			if (obj != null)
			{
				Actor a = obj.GetComponent<Actor>();
				if (a != null)
				{
					if (setFrom == 2)
					{
						RPGAttribute att = a.ActorClass.GetAttribute(attId);
						if (att != null)
						{
							float val = att.Value;
							if (attWhat == 1) val = att.MinValue;
							else if (attWhat == 2) val = att.MaxValue;
							b.SetCustomVariable(varName, val.ToString());
						}
						else Debug.LogError("CustomVar Action Error: The object does not have the specified Attribute.");
					}
					else if (setFrom == 3)
					{
						b.SetCustomVariable(varName, a.ActorClass.Level.ToString());
					}
					else if (setFrom == 4)
					{
						b.SetCustomVariable(varName, a.currency.ToString());
					}

				}
				else Debug.LogError("CustomVar Action Error: The object must be an Actor.");
			}
			else Debug.LogError("CustomVar Action Error: No object specified.");
		}
		
		else if (setFrom == 5)
		{
			obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
			if (obj != null)
			{
				UniqueMonoBehaviour bh = obj.GetComponent<UniqueMonoBehaviour>();
				if (bh != null) b.SetCustomVariable(varName, bh.GetCustomVariable(str.Value));
				else Debug.LogError("CustomVar Action Error: the specified object does not support custom variables.");
			}
			else Debug.LogError("CustomVar Action Error: No object specified.");
		}

		return ReturnType.Done;
	}

	/*
	public string varName = "";
	public int toVal = 0; // 0:str, 1:num
	public StringValue str = new StringValue();
	public NumericValue num = new NumericValue();

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		CustomVarAction a = act as CustomVarAction;
		a.varName = this.varName;
		a.toVal = this.toVal;
		a.str = this.str.Copy();
		a.num = this.num.Copy();
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget)
	{
		if (string.IsNullOrEmpty(varName))
		{
			Debug.LogError("CustomVar Action Error: The variable name is not set.");
			return ReturnType.Done;
		}

		if (varName.Contains("|"))
		{
			Debug.LogError("CustomVar Action Error: The variable name is invalid. '|' is not allowed.");
			return ReturnType.Done;
		}

		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget);
		if (obj == null)
		{
			Debug.LogError("CustomVar Action Error: The subject did not exist.");
			return ReturnType.Done;
		}

		UniqueMonoBehaviour b = obj.GetComponent<UniqueMonoBehaviour>();
		if (b == null)
		{
			Debug.LogError("CustomVar Action Error: The subject does not support custom variables.");
			return ReturnType.Done;
		}

		if (toVal == 0) b.SetCustomVariable(varName, str.Value);
		else b.SetCustomVariable(varName, num.Value.ToString());

		return ReturnType.Done;
	}
	*/
	// ================================================================================================================
} }