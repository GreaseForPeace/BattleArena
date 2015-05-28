// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[System.Serializable]
public class RPGAttributeData
{
	public GUID attribId = new GUID();			// use to link the atribute data with attribute definition since ActorClass only save the data and not whole definition

	public float baseVal = 0f;					// starting/saved value
	public float baseMin = 0f;					// min that value may reach
	public float baseMax = 999f;				// max for values that can regen (like Health)

	public bool canRegen = false;				// true for values like HP which goes up and down within a limit
	public float regenRate = 0f;				// regen so much per second
	public float regenAfterTimeout = 0f;		// regen can only start if attrib val did not decr in past x seconds. usefull for implementing "out-of-combat" regen

	public bool levelAffectVal = false;			// does level influence what the value is?
	public bool levelAffectMax = false;			// does level influence what the max value is?

	public float valAffectMin = 1f;
	public float valAffectMax = 10f;
	public float maxAffectMin = 1f;
	public float maxAffectMax = 10f;
	public AnimationCurve valAffectCurve=null;
	public AnimationCurve maxAffectCurve=null;

	// Events to call when event happens (these references the Event prefabs, as defined in the database, which will be instantiated at runtime by the Attribute)
	public GameObject onValChangeEventPrefab;	// called when the value changed
	public GameObject onMinValueEventPrefab;	// called when the value reached the min value
	public GameObject onMaxValueEventPrefab;	// called when the value reached the max value

	// ----------------------------------------------------------------------------------------------------------------

	public RPGAttributeData Copy(float maxLevel)
	{
		RPGAttributeData a = new RPGAttributeData();
		a.attribId.Value = this.attribId.Value;
		a.baseVal = this.baseVal;
		a.baseMin = this.baseMin;
		a.baseMax = this.baseMax;
		a.canRegen = this.canRegen;
		a.regenRate = this.regenRate;
		a.levelAffectVal = this.levelAffectVal;
		a.levelAffectMax = this.levelAffectMax;
		a.regenAfterTimeout = this.regenAfterTimeout;
		a.valAffectMax = this.valAffectMax;
		a.maxAffectMax = this.maxAffectMax;
		if (this.valAffectCurve != null) a.valAffectCurve = AnimationCurve.Linear(1, 1, maxLevel, a.valAffectMax);
		if (this.maxAffectCurve != null) a.maxAffectCurve = AnimationCurve.Linear(1, 1, maxLevel, a.maxAffectMax);
		a.onValChangeEventPrefab = this.onValChangeEventPrefab;
		a.onMinValueEventPrefab = this.onMinValueEventPrefab;
		a.onMaxValueEventPrefab = this.onMaxValueEventPrefab;
		return a;
	}

	// ================================================================================================================
} }
