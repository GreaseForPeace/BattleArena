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
public class InputDefinition
{
	public bool isUsed = true;		// this is used to check if this definition is actually used and should be added to the inputs list (needed as designer can remove certain inputs from layout at design time)
	public bool showInGUI = true;	// can player change this in the game settings/options gui? (helper that tells gui theme if this should be shown or not if theme support this)

	public int order = 0;			// order of input in callback list, lower numbers appears 1st in list
	public bool isActive = true;	// if false then will not be checked if callback is set

	public string inputName;		// name of the input, "Move Forward", "Turn Right", "SkillSlot1", etc
	public string groupName;		// group that this input belongs to, "Movement", "Actions", etc

	public bool triggerOnSingle = false;
	public bool triggerOnDouble = false;
	public bool triggerOnHeld = false;

	public KeyCode primaryButton = KeyCode.None;	// the primary key to bind to
	public KeyCode secondaryButton = KeyCode.None;	// an optional secondary key to bind to

	// callback (function to call when one of the above set key triggers occur)
	public delegate void Callback(InputManager.InputEvent e, System.Object[] args);
	public Callback callback { get; set; }
	public System.Object[] callbackParams { get; set; }

	// designer defined input definition
	public bool isCustom = false;
	public string eventGUID = "";
	public string cachedEventName { get; set; }
	public RPGEvent cachedEvent { get; set; }

	public void CopyTo(InputDefinition def)
	{
		def.isUsed = this.isUsed;
		def.showInGUI = this.showInGUI;
		//def.order = this.order; - do not copy order, not something that is changes outside of code
		def.inputName = this.inputName;
		def.groupName = this.groupName;
		def.triggerOnSingle = this.triggerOnSingle;
		def.triggerOnDouble = this.triggerOnDouble;
		def.triggerOnHeld = this.triggerOnHeld;
		def.primaryButton = this.primaryButton;
		def.secondaryButton = this.secondaryButton;
		def.isCustom = this.isCustom;
		def.eventGUID = this.eventGUID;
	}

	public InputDefinition GetCopy()
	{
		InputDefinition def = new InputDefinition();
		this.CopyTo(def);
		return def;
	}
}

[AddComponentMenu("")]
public class InputManager : MonoBehaviour 
{
	public class InputEvent
	{
		public bool isSingle=false;
		public bool isDouble=false;
		public bool isHeld=false;
	}

	public class InputIdxCache
	{
		public int groupIdx;
		public int inputIdx;
	}

	private class InputGroup
	{
		public string name;
		public List<InputDefinition> inputs = new List<InputDefinition>();
	}

	private List<InputGroup> inputGroups = new List<InputGroup>();
	private List<InputDefinition> callbackList = new List<InputDefinition>(); // list with only the inputs that make use of callbacks

	private enum State { Init, Running }
	private State state = State.Init;

	public class KeyStateInfo
	{
		public float timer = 0f;
		
		public int wentUpCount = 0;
		public bool allreadyWentUpThisFrame = false;
		
		public bool hasSendSingleTriggers = false;
		public bool shouldSetAs_hasSendSingleTriggers = false;

		public bool shouldRemove = false;
	}

	private Dictionary<KeyCode, KeyStateInfo> keyState = new Dictionary<KeyCode, KeyStateInfo>();

	// ================================================================================================================
	#region pub

	/// <summary>Load input definitions via a binder. This should be called during Start() of a component (or as soon as possible, but not in Awake())</summary>
	public void LoadInputFromBinder(InputBinderBase binder)
	{
		List<InputDefinition> defs = binder.GetInputBinds();
		if (defs == null) return;
		if (defs.Count == 0) return;

		state = State.Init;
		foreach (InputDefinition d in defs)
		{
			InputIdxCache idx = AddInput(d);
			binder._SaveInputIdxCache(idx); // tell the binder about the idx in case it wants to save it
		}
	}

	public void UnloadInputBinder(InputBinderBase binder)
	{
		List<InputDefinition> defs = binder.GetInputBinds();
		if (defs == null) return;
		if (defs.Count == 0) return;

		state = State.Init;

		List<InputGroup> checkEmptyGroups = new List<InputGroup>();
		InputGroup group = null;
		foreach (InputDefinition d in defs)
		{
			if (group != null)
			{
				if (!group.name.Equals(d.groupName)) group = null;
			}

			if (group == null)
			{
				int idx = GetInputGroupIdx(d.groupName);
				if (idx < 0) continue;
				group = inputGroups[idx];
				if (!checkEmptyGroups.Contains(group)) checkEmptyGroups.Add(group);
			}

			int rem = -1;
			for (int i = 0; i < group.inputs.Count; i++)
			{
				if (group.inputs[i].inputName.Equals(d.inputName)) { rem = i; break; }
			}
			
			if (rem >= 0) 
			{
				group.inputs.RemoveAt(rem);

				rem = -1;
				for (int i = 0; i < callbackList.Count; i++)
				{
					if (callbackList[i].groupName.Equals(d.groupName) && callbackList[i].inputName.Equals(d.inputName))
					{
						rem = i; break;
					}
				}

				if (rem >= 0) callbackList.RemoveAt(rem);
			}
		}

		for (int i = 0; i < checkEmptyGroups.Count; i++)
		{
			if (checkEmptyGroups[i].inputs.Count == 0) inputGroups.Remove(checkEmptyGroups[i]);
		}
	}

	/// <summary>add a new input and return its idx and the idx for the group it is in. will return groupIdx=-1 and inputIdx=-1 if the inputdef was not added</summary>
	public InputIdxCache AddInput(InputDefinition definition)
	{
		if (!definition.isUsed) return new InputIdxCache() { groupIdx = -1, inputIdx = -1 }; // not actually used, dont add

		InputIdxCache idx = new InputIdxCache();

		// get the group idx, else add group
		idx.groupIdx = GetInputGroupIdx(definition.groupName);
		if (idx.groupIdx < 0)
		{
			InputGroup group = new InputGroup() { name = definition.groupName };
			inputGroups.Add(group);
			idx.groupIdx = inputGroups.IndexOf(group);
		}

		// check if the input was already raged before and show warning if so (someone forgot to unload an input binder)
		int inputIdx = GetInputIdx(idx.groupIdx, definition.inputName);
		if (inputIdx < 0)
		{
			inputGroups[idx.groupIdx].inputs.Add(definition);
			idx.inputIdx = inputGroups[idx.groupIdx].inputs.IndexOf(definition);

			if (definition.callback != null) callbackList.Add(definition); // add to special list too
			else if (definition.isCustom && !string.IsNullOrEmpty(definition.eventGUID))
			{
				RPGEvent e = UniRPGGlobal.DB.GetEvent(new GUID(definition.eventGUID));
				if (e != null)
				{					
					definition.cachedEvent = e;
					callbackList.Add(definition);
				}
			}
		}
		else
		{
			if (!definition.isCustom) Debug.LogError(string.Format("InputBinder Error. It seems like you are trying to load an InputBinder that is already loaded. Forgot to unload it when you should have? {0} :: {1}", definition.groupName, definition.inputName));
			return new InputIdxCache() { groupIdx = -1, inputIdx = -1 };
		}

		return idx;
	}

	public InputDefinition GetInputDefinition(string groupName, string inputName)
	{
		int i = GetInputGroupIdx(groupName);
		if (i >= 0)
		{
			for (int j = 0; j < inputGroups[i].inputs.Count; j++)
			{
				if (inputGroups[i].inputs[j].inputName.Equals(inputName))
				{
					return inputGroups[i].inputs[j];
				}
			}
		}
		return null;
	}

	public InputDefinition GetInputDefinition(InputIdxCache idx)
	{
		return inputGroups[idx.groupIdx].inputs[idx.inputIdx];
	}

	/// <summary>Find the idx of named group, returns -1 if not found</summary>
	public int GetInputGroupIdx(string groupName)
	{
		for (int i = 0; i < inputGroups.Count; i++)
		{
			if (inputGroups[i].name.Equals(groupName)) return i;
		}
		return -1;
	}

	/// <summary>Find the idx of an input i the group. returns -1 if not found</summary>
	public int GetInputIdx(int groupIdx, string inputName)
	{
		if (groupIdx >= 0 && groupIdx < inputGroups.Count)
		{
			for (int i = 0; i < inputGroups[groupIdx].inputs.Count; i++)
			{
				if (inputGroups[groupIdx].inputs[i].inputName.Equals(inputName)) return i;
			}
		}
		return -1;
	}

	/// <summary>Find input and return the idx of the group it is in and input's idx. return idx with -1,-1 if input not found</summary>
	public InputIdxCache GetInputIdx(string inputName)
	{
		for (int i = 0; i < inputGroups.Count; i++)
		{
			for (int j = 0; j < inputGroups[i].inputs.Count; j++)
			{
				if (inputGroups[i].inputs[j].inputName.Equals(inputName))
				{
					InputIdxCache idx = new InputIdxCache(){ groupIdx = i, inputIdx = j };
					return idx;
				}
			}
		}
		return new InputIdxCache() { groupIdx = -1, inputIdx = -1 };
	}

	/// <summary>Find input and return the idx of the group it is in and input's idx. null if not found</summary>
	public InputIdxCache GetInputIdx(string groupName, string inputName)
	{
		int i = GetInputGroupIdx(groupName);
		if (i >= 0)
		{
			for (int j = 0; j < inputGroups[i].inputs.Count; j++)
			{
				if (inputGroups[i].inputs[j].inputName.Equals(inputName))
				{
					InputIdxCache idx = new InputIdxCache() { groupIdx = i, inputIdx = j };
					return idx;
				}
			}
		}
		return new InputIdxCache() { groupIdx = -1, inputIdx = -1 };
	}

	/// <summary>returns true of the two definitions uses the same buttons</summary>
	public bool UsesSameButtons(InputIdxCache idx1, InputIdxCache idx2)
	{
		return (inputGroups[idx1.groupIdx].inputs[idx1.inputIdx].primaryButton == inputGroups[idx2.groupIdx].inputs[idx2.inputIdx].primaryButton ||
				inputGroups[idx1.groupIdx].inputs[idx1.inputIdx].secondaryButton == inputGroups[idx2.groupIdx].inputs[idx2.inputIdx].secondaryButton);
	}

	/// <summary>will check if the prim/secondary button of the input is active as defined</summary>
	public bool IsHeld(InputIdxCache idx)
	{
		InputDefinition def = inputGroups[idx.groupIdx].inputs[idx.inputIdx];
		return ( (def.primaryButton != KeyCode.None && Input.GetKey(def.primaryButton))
				||
				(def.secondaryButton != KeyCode.None && Input.GetKey(def.secondaryButton))
				);
	}

	/// <summary>used to enable/ disable an input definition</summary>
	public void SetActive(InputIdxCache idx, bool active)
	{
		inputGroups[idx.groupIdx].inputs[idx.inputIdx].isActive = active;
	}

	#endregion
	// ================================================================================================================
	#region start and update

	void Start()
	{
		// do not do the init inside start as input binders are still being loaded
		// at the first update the init of the inputmanager will continue
		state = State.Init;
	}

	void Update()
	{
		if (state == State.Running)
		{
			if (!UniRPGGlobal.InstanceExist) return;
			if (UniRPGGlobal.Instance.state != UniRPGGlobal.State.InGame) return;

			// do not do anything if GUI consumed the input
			// don't do it here as it also stop the gui from receiving its toggles
			// each system will have to check itself 
			//if (UniRPGGlobal.GUIConsumedInput) return;
			if (GUIUtility.hotControl != 0) return;

			List<KeyCode> remove = new List<KeyCode>();
			foreach (KeyValuePair<KeyCode, KeyStateInfo> kv in keyState)
			{
				kv.Value.timer -= Time.deltaTime;
				kv.Value.allreadyWentUpThisFrame = false;
				if (kv.Value.shouldSetAs_hasSendSingleTriggers) kv.Value.hasSendSingleTriggers = true;

				if (kv.Value.timer <= 0.0f)
				{
					// check if key not still being held and set to remove
					if (!Input.GetKey(kv.Key) && !Input.GetKeyUp(kv.Key))
					{
						kv.Value.shouldRemove = true;

						// lost window to check for double click
						if (kv.Value.wentUpCount > 0 && !kv.Value.hasSendSingleTriggers)
						{
							for (int i = 0; i < callbackList.Count; i++)
							{
								if (callbackList[i].primaryButton == kv.Key)
								{
									if (callbackList[i].triggerOnSingle)
									{	// at least send single click event then cause this input is gonna be destroyed now
										MakeCallback(new InputEvent() { isSingle = true }, callbackList[i]);
									}
								}
							}
						}
					}
				}

				if (kv.Value.shouldRemove) remove.Add(kv.Key);
			}

			foreach (KeyCode k in remove)
			{
				keyState.Remove(k);
			}

			for (int i = 0; i < callbackList.Count; i++)
			{
				if (callbackList[i].isActive == false || (callbackList[i].callback == null && !callbackList[i].isCustom)) continue;
				CheckKey(callbackList[i].primaryButton, callbackList[i]);
				CheckKey(callbackList[i].secondaryButton, callbackList[i]);
			}
		}

		else if (state == State.Init)
		{
			state = State.Running;
			InitInput();
		}
	}

	private void CheckKey(KeyCode k, InputDefinition def)
	{		
		if (k != KeyCode.None)
		{
			// Get it started
			if (Input.GetKeyDown(k))
			{
				KeyStateInfo ks = null;
				keyState.TryGetValue(k, out ks);
				if (ks == null)
				{	// is a new key, add it now
					ks = new KeyStateInfo()
					{
						timer = 0.45f,
						wentUpCount = 0,
						allreadyWentUpThisFrame = false,
						hasSendSingleTriggers = false,
						shouldSetAs_hasSendSingleTriggers = false,
						shouldRemove = false,
					};
					keyState.Add(k, ks);
				}
			}

			// Detect Single/Double click event
			if (Input.GetKeyUp(k))
			{
				KeyStateInfo ks = null;
				keyState.TryGetValue(k, out ks);
				if (ks != null)
				{
					if (!ks.allreadyWentUpThisFrame)
					{
						ks.allreadyWentUpThisFrame = true;
						ks.wentUpCount++;
					}

					if (!ks.hasSendSingleTriggers)
					{
						ks.shouldSetAs_hasSendSingleTriggers = true;
						if (def.triggerOnSingle)
						{
							MakeCallback(new InputEvent() { isSingle = true }, def);
						}
					}

					if (ks.wentUpCount > 1)
					{	// a double click was detected?
						if (ks.timer > 0.0f)
						{	// yes
							if (def.triggerOnDouble)
							{	// send double click event
								ks.hasSendSingleTriggers = true; // don't allow further single triggers to occur
								MakeCallback(new InputEvent() { isDouble = true }, def);
							}
						}
						ks.shouldRemove = true;
					}

					else
					{
						if (ks.timer <= 0.0f)
						{
							ks.shouldRemove = true;
						}
					}
				}
			}

			// Detect 'Held' event
			if (Input.GetKey(k))
			{
				KeyStateInfo ks = null;
				keyState.TryGetValue(k, out ks);
				if (ks != null)
				{
					if (def.triggerOnHeld)
					{
						MakeCallback(new InputEvent() { isHeld = true }, def);
					}
				}
			}
		}
	}

	void LateUpdate()
	{
		UniRPGGlobal.GUIConsumedInput = false; // reset the flag
	}

	private void InitInput()
	{
		// 1st tun through db and look for custom input definitions to be added to manager
		for (int i = 0; i < UniRPGGlobal.DB.inputDefs.Count; i++)
		{
			if (UniRPGGlobal.DB.inputDefs[i].isCustom && UniRPGGlobal.DB.inputDefs[i].isUsed)
			{
				AddInput(UniRPGGlobal.DB.inputDefs[i]);
			}
		}

		// by now all the input binders where called. now load changes to input from DB
		InputDefinition def = null;
		for (int i = 0; i < UniRPGGlobal.DB.inputDefs.Count; i++)
		{
			def = GetInputDefinition(UniRPGGlobal.DB.inputDefs[i].groupName, UniRPGGlobal.DB.inputDefs[i].inputName);
			if (def != null)
			{
				if (!def.isCustom)
				{
					UniRPGGlobal.DB.inputDefs[i].CopyTo(def);

					// remove from the callbacks if not a used input
					if (!def.isUsed && def.callback != null) callbackList.Remove(def);
				}
			}
		}

		// sort the callback list according to InputDefinition.order field
		callbackList.Sort(delegate(InputDefinition a, InputDefinition b) { return a.order.CompareTo(b.order); });

		// fixme .. todo
		// load changes from playerprefs/saved settings
	}

	private void MakeCallback(InputEvent e, InputDefinition def)
	{
		if (def.isCustom)
		{
			if (UniRPGGlobal.Instance.state != UniRPGGlobal.State.InMainMenu)
			{
				def.cachedEvent.Execute(null);
			}
		}
		else
		{
			def.callback(e, def.callbackParams);
		}
	}

	#endregion
	// ================================================================================================================

	private static InputManager _instance;
	public static InputManager Instance
	{
		get
		{
			if (_instance == null)
			{
				Object[] objs = FindObjectsOfType(typeof(InputManager));
				if (objs.Length > 0) _instance = objs[0] as InputManager;
				else
				{
					GameObject g = new GameObject("InputManager");
					_instance = g.AddComponent<InputManager>();
					UniRPGGlobal.RegisterGlobalObject(g);
				}
			}
			return _instance;
		}
	}

	public static bool InstanceExist { get { return _instance != null; } }

	// ================================================================================================================
} }