// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class GlobalVarAction : Action
{
	public int varType = 0; // 0:number, 1:text, 2:object
	public string varName = "";

	public StringValue str = new StringValue();

	// the following are only used when varType != 2
	public int setFrom = 0; // 0: string var, 1: num var, 2: attribute, 3:level, 4:currency, 5:customvar
	public NumericValue num = new NumericValue();
	public GUID attId = new GUID();
	public int attWhat = 0; // 0:val, 1:min, 2:max

	// the following are only used when varType == 2
	public int objSet = 0; // 0:to subject, 1:subject child, 2:obj tagged, 3:obj named

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		GlobalVarAction a = act as GlobalVarAction;
		a.varType = this.varType;
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
			Debug.LogError("GlobalVar Action Error: The variable name is not set.");
			return ReturnType.Done;
		}

		if (varName.Contains("|"))
		{
			Debug.LogError("GlobalVar Action Error: The variable name is invalid.");
			return ReturnType.Done;
		}

		if (varType == 2)
		{
			Object obj = null;
			if (objSet == 0)
			{
				obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
			}
			else if (objSet == 1)
			{
				GameObject go = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
				if (go != null)
				{
					Transform tr = go.transform.FindChild(str.Value);
					if (tr != null) obj = tr.gameObject;
				}
			}
			else if (objSet == 2)
			{
				obj = GameObject.FindWithTag(str.Value);
			}
			else if (objSet == 3)
			{
				obj = GameObject.Find(str.Value);
			}

			UniRPGGlobal.DB.SetGlobalObject(varName, obj);
		}
		else
		{
			// normal.. string or number
			if (setFrom == 0 || setFrom == 1)
			{
				if (varType == 0)
				{
					if (setFrom == 0)
					{
						float r = 0f; float.TryParse(str.Value, out r);
						UniRPGGlobal.DB.SetGlobalNumber(varName, r);
					}
					else if (setFrom == 1)
					{
						UniRPGGlobal.DB.SetGlobalNumber(varName, num.Value);
					}
				}
				else if (varType == 1)
				{
					if (setFrom == 0) UniRPGGlobal.DB.SetGlobalString(varName, str.Value);
					else if (setFrom == 1) UniRPGGlobal.DB.SetGlobalString(varName, num.Value.ToString());
				}
			}

			// setting from attribute/level/currency (require actor)
			else if (setFrom == 2 || setFrom == 3 || setFrom == 4)
			{
				GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
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
								if (varType == 0) UniRPGGlobal.DB.SetGlobalNumber(varName, val);
								else UniRPGGlobal.DB.SetGlobalString(varName, val.ToString());
							}
							else Debug.LogError("GlobalVar Action Error: The object does not have the specified Attribute.");
						}
						else if (setFrom == 3)
						{
							if (varType == 0) UniRPGGlobal.DB.SetGlobalNumber(varName, a.ActorClass.Level);
							else UniRPGGlobal.DB.SetGlobalString(varName, a.ActorClass.Level.ToString());
						}
						else if (setFrom == 4)
						{
							if (varType == 0) UniRPGGlobal.DB.SetGlobalNumber(varName, a.currency);
							else UniRPGGlobal.DB.SetGlobalString(varName, a.currency.ToString());
						}
						else if (setFrom == 5)
						{
							string v = a.Character.GetCustomVariable(str.Value);
							if (varType == 0)
							{
								float r = 0f; float.TryParse(str.Value, out r);
								UniRPGGlobal.DB.SetGlobalNumber(varName, r);
							}
							else UniRPGGlobal.DB.SetGlobalString(varName, v);
						}
					}
					else Debug.LogError("GlobalVar Action Error: The object must be an Actor.");
				}
				else Debug.LogError("GlobalVar Action Error: No object specified.");
			}

			else if (setFrom == 5)
			{
				GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
				if (obj != null)
				{
					UniqueMonoBehaviour bh = obj.GetComponent<UniqueMonoBehaviour>();
					if (bh != null)
					{
						string val = bh.GetCustomVariable(str.Value);
						if (varType == 0)
						{
							float r = 0f; float.TryParse(val, out r);
							UniRPGGlobal.DB.SetGlobalNumber(varName, r);
						}
						else UniRPGGlobal.DB.SetGlobalString(varName, val);
					}
					else Debug.LogError("CustomVar Action Error: the specified object does not support custom variables.");
				}
				else Debug.LogError("CustomVar Action Error: No object specified.");
			}
		}

		return ReturnType.Done;
	}

	// ================================================================================================================
} }