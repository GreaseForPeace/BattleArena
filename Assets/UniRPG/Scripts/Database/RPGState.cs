// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

public class RPGState : ScriptableObject
{
	public GUID id;								// unique identifier for this State definition

	//use screenName to get/set the visible name for this definition. do not depend on .name to be valid
	public string screenName { get { return nm; } set { nm = value; name = value; } }
	[SerializeField]private string nm = string.Empty;

	public string description = string.Empty;
	public string notes = string.Empty;			// additional notes (used by designer, should not be something used in the game itself)
	public Texture2D[] icon = new Texture2D[3]; // up to 3 icons. what is used depends on game gui. editor uses the 1st if avail

	public string guiHelper;					// this can be used to help the gui determine what to do with this attrib (what you use here depends on the gui theme)

	public int maxInstances = 0;				// how many instances of this State can exist on an object (0=unlimited)

	public enum Effect
	{
		RunEventInIntervals,	// this is for when the State works like a buff/debuff
		PreventEquipSlot,		// this is for when the state is used to prevent the playuer from equipping to a specified equip slot
	}

	public Effect effect = Effect.RunEventInIntervals;

	// used by Effect.PreventEquipSlot
	public int slot = 0;					

	// used as timeout by Effect.RunEventInIntervals
	public float timeoutSettings = 1f;
	public float autoRemoveTimeoutSettings = 0f;
	public GameObject eventPrefab = null;

	// ================================================================================================================
	// Runtime

	private RPGEvent eventInstance = null;
	private float timeout = 0f;
	private float autoRemoveTimeout = 0f;

	// ================================================================================================================

	public void CopyTo(RPGState st)
	{
		st.id = this.id.Copy();

		st.screenName = this.screenName;
		st.description = this.description;
		st.notes = this.notes;
		st.icon = new Texture2D[3] { this.icon[0], this.icon[1], this.icon[2] };
		st.guiHelper = this.guiHelper;

		st.maxInstances = this.maxInstances;
		st.effect = this.effect;
		st.slot = this.slot;
		st.timeoutSettings = this.timeoutSettings;
		st.autoRemoveTimeoutSettings = this.autoRemoveTimeoutSettings;
		st.eventPrefab = this.eventPrefab;
	}

	public static RPGState CreateInstance(RPGState state, Transform eventParent)
	{
		RPGState s = ScriptableObject.CreateInstance<RPGState>();
		s.id = new GUID();
		s.id.Value = state.id.Value;
		s.slot = state.slot;
		s.effect = state.effect;
		s.timeoutSettings = state.timeoutSettings;
		s.autoRemoveTimeoutSettings = state.autoRemoveTimeoutSettings;
		s.eventPrefab = state.eventPrefab;

		if (s.effect == Effect.RunEventInIntervals && s.eventPrefab)
		{
			GameObject go = (GameObject)GameObject.Instantiate(s.eventPrefab);
			if (go)
			{
				go.transform.parent = eventParent;
				s.eventInstance = go.GetComponent<RPGEvent>();
				s.timeout = s.timeoutSettings;
				s.autoRemoveTimeout = s.autoRemoveTimeoutSettings;
			}
		}

		return s;
	}

	void OnEnable()
	{	// This function is called when the object is loaded (used for similar reasons to MonoBehaviour.Reset)
		id = GUID.Create(id);
	}

	void OnDestroy()
	{
		if (eventInstance) Destroy(eventInstance.gameObject);
	}

	/// <summary>Called each frame by the owner of the State. Returns true to inform owner to remove the State from itself.</summary>
	public bool UpdateState(GameObject owner)
	{
		if (effect == Effect.RunEventInIntervals && eventInstance)
		{	
			// run the event every so many seconds
			timeout -= Time.deltaTime;
			if (timeout <= 0.0f)
			{
				timeout = timeoutSettings;
				eventInstance.Execute(owner);
			}

			// check if State should auto-remove after time
			if (autoRemoveTimeoutSettings > 0.0f)
			{
				autoRemoveTimeout -= Time.deltaTime;
				if (autoRemoveTimeout <= 0.0f) return true;
			}
		}
		return false;
	}

	// ================================================================================================================
} }