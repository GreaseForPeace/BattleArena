// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

public class RPGAttribute : ScriptableObject
{
	public GUID id;								// way to uniquely identify this attribute (can't use its name since that might change after the attrib was 1st defined)

	//use screenName to get/set the visible name for this definition. do not depend on .name to be valid
	public string screenName { get { return nm; } set { nm = value; name = value; } }
	[SerializeField] private string nm = string.Empty;

	public string shortName = string.Empty;
	public string description = string.Empty;
	public string notes = string.Empty;			// additional notes (used by designer, should not be something used in the game itself)
	public Texture2D[] icon = new Texture2D[3]; // up to 3 icons. what is used depends on game gui. editor uses the 1st if avail

	public string guiHelper;					// this can be used to help the gui determine what to do with this attrib (what you use here depends on the gui theme)

	public GameObject lootDropPrefab;			// the prefab to use when dropping this attribute as a loot reward

	// the Attribute has no data until it becomes part of an Actor Class
	public RPGAttributeData data = null;

	// ----------------------------------------------------------------------------------------------------------------

	public delegate void OnValueChange(RPGAttribute att);
	public event OnValueChange onValueChange = null; // add callback/ listeners here that will be informed when the value changes

	// ----------------------------------------------------------------------------------------------------------------

	private GameObject owningActor;			// the actor who owns this attrib at runtime

	private RPGEvent onValChangeEvent;		// instantiated from prefabs defined in data
	private RPGEvent onMinValueEvent;		// instantiated from prefabs defined in data
	private RPGEvent onMaxValueEvent;		// instantiated from prefabs defined in data

	private float regenAfterTimer = 0f;
	private float regenTimer = 0f;
	
	// this is the actual value of the attribute
	private float _value = 0f;
	public float Value {
		get
		{
			return _value + Bonus;
		}

		set
		{
			regenTimer = Time.time + 1f;
			
			if (value < _value)
			{	// if the attribute value was decreased then start regen after this timeout
				regenAfterTimer = data.regenAfterTimeout; 
			}

			_value = value;

			if (onValueChange != null) onValueChange(this);
	
			if (onValChangeEvent) onValChangeEvent.Execute(owningActor);

			if (_value <= MinValue)
			{
				_value = MinValue;
				if (onMinValueEvent) onMinValueEvent.Execute(owningActor);
			}
			else if (_value >= MaxValue)
			{
				_value = MaxValue;
				if (onMaxValueEvent) onMaxValueEvent.Execute(owningActor);
			}
		}
	}

	public float NoBonusValue { get { return _value; } }

	// this is the current/runtime min value that this attribute can reach, if used
	public float MinValue {get;set;}

	// this is the current/runtime max value that this attribute can reach, if used
	public float MaxValue {get;set;}

	// this will be added to the returned Value
	public float Bonus {get;set;}

	// ================================================================================================================

	public void CopyTo(RPGAttribute att, float maxLevel)
	{
		att.id = this.id.Copy();

		att.screenName = this.screenName;
		att.shortName = this.shortName;
		att.description = this.description;
		att.notes = this.notes;
		att.icon = new Texture2D[3] { this.icon[0], this.icon[1], this.icon[2] };
		att.guiHelper = this.guiHelper;

		if (this.data != null) att.data = this.data.Copy(maxLevel);
		else att.data = null;
	}

	void OnEnable()
	{	// This function is called when the object is loaded (used for similar reasons to MonoBehaviour.Reset)
		id = GUID.Create(id);
	}

	public void Init(GameObject owningActor)
	{
		this.owningActor = owningActor;
		Bonus = 0f;
		MinValue = data.baseMin;
		MaxValue = data.baseMax;
		_value = data.baseVal;
		if (_value > MaxValue) _value = MaxValue;
		if (_value < MinValue) _value = MinValue;

		if (data.onValChangeEventPrefab)
		{
			GameObject go = (GameObject)Instantiate(data.onValChangeEventPrefab);
			go.transform.parent = owningActor.transform;
			onValChangeEvent = go.GetComponent<RPGEvent>();
		}
		if (data.onMinValueEventPrefab)
		{
			GameObject go = (GameObject)Instantiate(data.onMinValueEventPrefab);
			go.transform.parent = owningActor.transform;
			onMinValueEvent = go.GetComponent<RPGEvent>();
		}
		if (data.onMaxValueEventPrefab)
		{
			GameObject go = (GameObject)Instantiate(data.onMaxValueEventPrefab);
			go.transform.parent = owningActor.transform;
			onMaxValueEvent = go.GetComponent<RPGEvent>();
		}
	}

	public void Init(GameObject owningActor, float initBaseValue)
	{
		Init(owningActor);
		_value = initBaseValue;
		if (_value > MaxValue) _value = MaxValue;
		if (_value < MinValue) _value = MinValue;
	}

	public void SetWithoutTriggeringEvents(float val, float min, float max, float bonus)
	{
		Bonus = bonus;
		_value = val;
		MinValue = min;
		MaxValue = max;
		if (_value > MaxValue) _value = MaxValue;
		if (_value < MinValue) _value = MinValue;
	}

	// called by ActorClass to update when needed (only called if canRegen = true)
	public void Update()
	{
		if (Value < MaxValue)
		{	// only run regen timers if value is actually less than the base value it is trying to reach
			if (regenAfterTimer > 0.0f)
			{
				regenAfterTimer -= Time.deltaTime;
			}
			else
			{
				if ((regenTimer - Time.time) < 0.0f)
				{
					Value += data.regenRate;
				}
			}
		}
	}

	/// <summary>Helper for getting an attribute by guid from a list of attributes</summary>
	public static RPGAttribute GetAttribByGuid(List<RPGAttribute> attribs, GUID guid)
	{
		if (guid == null) return null;
		for (int i = 0; i < attribs.Count; i++)
		{
			if (attribs[i].id.Value == guid.Value) return attribs[i];
		}
		return null;
	}

	/// <summary>A helper to find the index of the attribute with guid in the list of attribs that was passed</summary>
	public static int GetAttribIdx(List<RPGAttribute> attribs, GUID guid)
	{
		if (guid == null) return -1;
		if (attribs.Count > 0)
		{
			for (int i = 0; i < attribs.Count; i++)
			{
				if (attribs[i].id.Value == guid.Value) return i;
			}
		}
		return -1;
	}

	// ================================================================================================================
} }