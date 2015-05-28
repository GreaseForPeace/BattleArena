// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRPG {

[System.Serializable]
public class ValueModifier
{
	public int resultToValue = 0;							// how is calculated applied to the value? 0:add, 1:sub, 2:div, 3:multiply
	public int[] param = { 0, 0 };							// 0: Not Used, 1:Numeric, 2:Value, 3:level(1), 4:attribute(1), 5:CustomVar(1), 6:level(2), 7:attribute(2), 8:CustomVar(2)
	public GUID[] attribId = { new GUID(), new GUID() };	// attrib to use if apply = 3
	public NumericValue[] numVal = { new NumericValue(), new NumericValue() };	// var to use if apply = 1
	public int doWhatToParams = 0;							// should the two params (if there are two) be 0:add, 1:sub, 2:div, 3:multiply, 4:mod (before being applied to the value)
	public string[] customVar = { "", "" };					// when param0/1 = 5 or 8

	public ValueModifier Copy()
	{
		ValueModifier v = new ValueModifier();
		v.resultToValue = this.resultToValue;
		v.param = new int[] { this.param[0], this.param[1] };
		v.attribId = new GUID[] { this.attribId[0].Copy(), this.attribId[1].Copy() };
		v.numVal = new NumericValue[] { this.numVal[0].Copy(), this.numVal[1].Copy() };
		v.doWhatToParams = this.doWhatToParams;
		v.customVar = this.customVar;
		return v;
	}
}

[AddComponentMenu("")]
public class AttributeAction : Action
{
	public ActionTarget aggressor = new ActionTarget();

	public GUID attribIdent = new GUID();
	public int affectedPart = 0; // 0: the Value, 1:Bonus, 2: Min, 3: Max

	public int doWhat = 0; // 0:subtract, 1:add, 2:set
	public NumericValue val = new NumericValue();

	public bool useRandomRange = false;
	public NumericValue val2 = new NumericValue(); // useRandomRange is true if this is >= val

	public bool usePercentage = false;
	public int percentBase = 0; // 0:curr value, 1:max value

	public List<ValueModifier> valueMod = new List<ValueModifier>(0);

	// ================================================================================================================

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		AttributeAction a = act as AttributeAction;
		a.aggressor = this.aggressor.Copy();
		a.attribIdent = this.attribIdent.Copy();
		a.affectedPart = this.affectedPart;
		a.doWhat = this.doWhat;
		a.val = this.val.Copy();
		a.useRandomRange = this.useRandomRange;
		a.val2 = this.val2.Copy();
		a.usePercentage = this.usePercentage;
		a.percentBase = this.percentBase;
		a.valueMod = new List<ValueModifier>(0);
		foreach (ValueModifier v in this.valueMod) a.valueMod.Add(v.Copy());
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		// Get the Subject (1)
		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
		if (obj == null)
		{
			Debug.LogError("Attribute Action Error: The subject did not exist.");
			return ReturnType.Done;
		}

		// Get the Subject's Actor (1)
		Actor actor = obj.GetComponent<Actor>();
		if (!actor)
		{
			Debug.LogError("Attribute Action Error: The subject is not an Actor.");
			return ReturnType.Done;
		}

		UniqueMonoBehaviour subB = null;
		if (actor == null) subB = obj.GetComponent<UniqueMonoBehaviour>();
		else subB = actor.Character;

		// Get the Subject's Attribute to modify
		RPGAttribute attrib = RPGAttribute.GetAttribByGuid(actor.ActorClass.Attributes, attribIdent);
		if (attrib == null)
		{
			Debug.LogError("Attribute Action Error: The Attribute could not be found.");
			return ReturnType.Done;
		}

		// Get the Aggressor Actor (2), if any
		Actor aggActor = null;
		UniqueMonoBehaviour aggB = null;
		obj = DetermineTarget(aggressor, self, targeted, selfTargetedBy, equipTarget, helper);
		if (obj != null)
		{
			aggActor = obj.GetComponent<Actor>();
			if (aggActor == null) aggB = obj.GetComponent<UniqueMonoBehaviour>();
			else aggB = aggActor.Character;
		}

		// ...
		float value = val.Value;
		if (useRandomRange)
		{
			float value2 = val2.Value;
			if (value < value2) value = Random.Range(value, value2);
			else value = Random.Range(value2, value);
		}

		// apply modifiers to value
		if (valueMod.Count > 0)
		{
			for (int i = 0; i < valueMod.Count; i++)
			{
				value = ApplyValueMod(valueMod[i], value, actor, aggActor, subB, aggB);
			}
		}

		// check if it is a percentage value
		if (usePercentage)
		{
			value /= 100f;
			if (percentBase == 0) value *= attrib.Value;// a percentage of current value
			else value *= attrib.data.baseMax;			// a percentage of the max value
		}

		// apply to the attribute
		if (affectedPart == 0)
		{	// to the Value
			if (doWhat == 0) attrib.Value -= value;			// subtract
			else if (doWhat == 1) attrib.Value += value;	// add
			else if (doWhat == 2) attrib.Value = value;		// set
		}
		else if (affectedPart == 1)
		{	// to the Bonus
			if (doWhat == 0) attrib.Bonus -= value;			// subtract
			else if (doWhat == 1) attrib.Bonus += value;	// add
			else if (doWhat == 2) attrib.Bonus = value;		// set
		}
		else if (affectedPart == 2)
		{	// to Min
			if (doWhat == 0) attrib.MinValue -= value;		// subtract
			else if (doWhat == 1) attrib.MinValue += value;	// add
			else if (doWhat == 2) attrib.MinValue = value;	// set
		}
		else if (affectedPart == 3)
		{	// to Max
			if (doWhat == 0) attrib.MaxValue -= value;		// subtract
			else if (doWhat == 1) attrib.MaxValue += value;	// add
			else if (doWhat == 2) attrib.MaxValue = value;	// set
		}

		return ReturnType.Done;
	}

	private float ApplyValueMod(ValueModifier mod, float value, Actor a1, Actor a2, UniqueMonoBehaviour ub1, UniqueMonoBehaviour ub2)
	{
		if (mod.param[0] == 0 && mod.param[1] == 0) return value; // nothing to do

		float v = 0f;
		float v1 = 0f;
		float v2 = 0f;

		if (mod.param[0] > 0)
		{	// get value from param 1; 0: Not Used, 1:Numeric, 2:Value, 3:level(1), 4:attribute(1), 5:CustomVar(1), 6:level(2), 7:attribute(2), 8:CustomVar(2)

			if (mod.param[0] == 1) v1 = mod.numVal[0].Value;
			else if (mod.param[0] == 2) v1 = value;
			else if (mod.param[0] == 3) v1 = a1.ActorClass.Level;
			else if (mod.param[0] == 4)
			{
				RPGAttribute att = a1.ActorClass.GetAttribute(mod.attribId[0]);
				if (att != null) v1 = att.Value;
				else Debug.LogError("Attribute Action Error: Modifier Attribute not found on Subject.");
			}
			else if (mod.param[0] == 5)
			{
				if (ub1 != null)
				{
					if (false == float.TryParse(ub1.GetCustomVariable(mod.customVar[0]), out v1))
					{
						Debug.LogError("Attribute Action Error: Modifier Custom Variable not found on Subject or has invalid value.");
					}
				}
				else Debug.LogError("Attribute Action Error: Subject does not support custom variables.");
			}
			else
			{
				if (mod.param[0] == 6 || mod.param[0] == 7)
				{
					if (a2 != null)
					{
						if (mod.param[0] == 6) v1 = a2.ActorClass.Level;
						else if (mod.param[0] == 7)
						{
							RPGAttribute att = a2.ActorClass.GetAttribute(mod.attribId[0]);
							if (att != null) v1 = att.Value;
							else Debug.LogError("Attribute Action Error: Modifier Attribute not found on Aggressor.");
						}
					}
					else Debug.LogError("Attribute Action Error: Aggressor is not an Actor.");
				}
				else if (mod.param[0] == 8)
				{
					if (ub2 != null)
					{
						if (false == float.TryParse(ub2.GetCustomVariable(mod.customVar[0]), out v1))
						{
							Debug.LogError("Attribute Action Error: Modifier Custom Variable not found on Aggressor or has invalid value.");
						}
					}
					else Debug.LogError("Attribute Action Error: Aggressor does not support custom variables.");
				}				
			}			
		}

		if (mod.param[1] > 0)
		{	// get value from param 2; 0: Not Used, 1:Numeric, 2:Value, 3:level(1), 4:attribute(1), 5:CustomVar(1), 6:level(2), 7:attribute(2), 8:CustomVar(2)
			
			if (mod.param[1] == 1) v2 = mod.numVal[1].Value;
			else if (mod.param[1] == 2) v2 = value;
			else if (mod.param[1] == 3) v2 = a1.ActorClass.Level;
			else if (mod.param[1] == 4)
			{
				RPGAttribute att = a1.ActorClass.GetAttribute(mod.attribId[1]);
				if (att != null) v2 = att.Value;
				else Debug.LogError("Attribute Action Error: Modifier Attribute not found on Subject.");
			}
			else if (mod.param[1] == 5)
			{
				if (ub1 != null)
				{
					if (false == float.TryParse(ub1.GetCustomVariable(mod.customVar[0]), out v1))
					{
						Debug.LogError("Attribute Action Error: Modifier Custom Variable not found on Subject or has invalid value.");
					}
				}
				else Debug.LogError("Attribute Action Error: Subject does not support custom variables.");
			}
			else
			{
				if (mod.param[1] == 6 || mod.param[1] == 7)
				{
					if (a2 != null)
					{
						if (mod.param[1] == 6) v2 = a2.ActorClass.Level;
						else if (mod.param[1] == 7)
						{
							RPGAttribute att = a2.ActorClass.GetAttribute(mod.attribId[1]);
							if (att != null) v2 = att.Value;
							else Debug.LogError("Attribute Action Error: Modifier Attribute not found on Aggressor.");
						}
					} else Debug.LogError("Attribute Action Error: Aggressor is not an Actor.");
				}
				else if (mod.param[1] == 8)
				{
					if (ub2 != null)
					{
						if (false == float.TryParse(ub2.GetCustomVariable(mod.customVar[1]), out v2))
						{
							Debug.LogError("Attribute Action Error: Modifier Custom Variable not found on Aggressor or has invalid value.");
						}
					}
					else Debug.LogError("Attribute Action Error: Aggressor does not support custom variables.");
				}
			}
		}

		if (mod.param[0] == 0) v = v2;
		if (mod.param[1] == 0) v = v1;

		if (mod.param[0] > 0 && mod.param[1] > 0)
		{	// apply math to param1 and 2 if applicable; 0:add, 1:sub, 2:div, 3:multiply
			if (mod.doWhatToParams == 0) v = v1 + v2;
			else if (mod.doWhatToParams == 1) v = v1 - v2;
			else if (mod.doWhatToParams == 2) v = v1 / v2;
			else if (mod.doWhatToParams == 3) v = v1 * v2;
			else if (mod.doWhatToParams == 4) v = v1 % v2;
		}

		// apply to value, // 0:add, 1:sub, 2:div, 3:multiply
		if (mod.resultToValue == 0) value = value + v;
		else if (mod.resultToValue == 1) value = value - v;
		else if (mod.resultToValue == 2) value = value / v;
		else if (mod.resultToValue == 3) value = value * v;
		else if (mod.resultToValue == 4) value = value % v;

		return value;
	}

	// ================================================================================================================
} }