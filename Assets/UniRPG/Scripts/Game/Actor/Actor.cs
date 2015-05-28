// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class Actor : MonoBehaviour 
{
	// ================================================================================================================
	#region Inspector Properties

	//use screenName to get/set the visible name for this definition. do not depend on .name to be valid
	public string screenName { get { return nm; } set { nm = value; name = value; } }
	[SerializeField] private string nm = string.Empty;

	public string description = string.Empty;		// a description of the character
	public string notes = string.Empty;				// additional notes (used by designer, should not be something used in the game itself)
	public Texture2D[] portrait = new Texture2D[3]; // up to 3 different portraits can be set and depends on how Game GUI wants to use them. 1st one is used by editor in various areas.

	public bool availAtStart = true;				// indicate if player can choose from these when gui supports it on new game screen

	public RPGActorClass actorClassPrefab = null;	// class of actor, like warrior, wizard, etc (don't use directly at runtime, rather use the Instantiated ActorClass property))
	public int startingXP = 0;						// the XP that this actor starts with will determine its starting Level.

	// equipped items, the index into this list is the same as Database.equipSlots
	// this will be equipped.Count=0 if not used, else it should be equipped.Count = Database.equipSlots.Count
	public List<RPGItem> equipped = new List<RPGItem>(0);

	public int currency = 0;						// how much currency this actor carry (gold, coins, etc)
	public int bagSize = 10;						// how many slots available to store items in, Make a call to CheckBagSize() if you change this so that bag (below) is updated to correct size
	public List<BagSlot> bag = new List<BagSlot>(0);// this will be bag.Count=0 if not used else it should be bag.Count = bagSize

	// the skill slots can be used to bind skills. using the slot will cause the onUse action chain for the skill to be called
	// these are prefabs for skills that the actor starts with and not the actual instances. see further below for runtime skills
	public int actionSlotCount = 10;										// how many slots there are
	public List<GameObject> startingSkills = new List<GameObject>(0);		// the available skills
	public List<GameObject> startingSkillSlots = new List<GameObject>(0);	// the actual equipped skills to start with
	public List<RPGState> startingStates = new List<RPGState>(0);			// states that the actor starts with. dont access this at runtime!
	
	#endregion
	// ================================================================================================================
	#region runtime vars

	public List<RPGSkill> skills { get; private set; }		// all skills that this actor has (instantiated)
	private List<RPGState> states = new List<RPGState>(0);	// these are the runtime/ instantiated states
	private List<ActionSlot> actionSlots = new List<ActionSlot>(0);

	public CharacterBase Character							// The Character that has this Actor instance at runtime
	{
		get
		{
			if (_character == null) _character = gameObject.GetComponent<CharacterBase>();
			return _character;
		}
	}
	private CharacterBase _character = null;

	public UniRPGGlobal.ActorType ActorType
	{
		get
		{
			if (Character != null) return Character.ActorType();
			//Debug.LogError("The Actor is broken. It needs one of the Character components.");
			return 0;
		}
	}

	public RPGActorClass ActorClass { get; private set; }	// instance of actorClassPrefab

	public RPGSkill currSkill { get; private set; }			// skill being used
	public RPGSkill nextSkill { get; private set; }			// queued skill
	public GameObject nextSkillTarget { get; private set; } // target of queued skill
	public float nextSkillMaxDistance { get; private set; }
	public Vector3 nextSkillLocation { get; set; }

	private RPGSkill autoSkill = null;						// set when auto-queuing of skill is wanted
	private GameObject autoSkillTarget = null;

	public bool InAOESelectMode { get; private set; }		// true while the actor is expecting an AOE target location to be submitted (use SubmitAOETarget() to submit a location to Actor so that skill can be performed) 
	public RPGSkill AOESkill { get; private set; }

	private Transform _tr;

	private int lastSlotCall = -1;				// used to detect double tap (only for player type)
	private float lastSlotCallTime = 0f;
	private Transform aoeDummyTr = null;

	private Transform[] mountPoints = null;

	#endregion
	// ================================================================================================================
	#region Init/Start

	public void Awake()
	{
		_tr = gameObject.transform;
		_character = gameObject.GetComponent<CharacterBase>();

		InAOESelectMode = false;
		currSkill = null;
		nextSkill = null;
		autoSkill = null;
		nextSkillTarget = null;
		autoSkillTarget = null;
		nextSkillMaxDistance = 0f;
	}

	public void Start() 
	{
		if (UniRPGGlobal.Instance.state == UniRPGGlobal.State.InMainMenu)
		{
			// Actor should be disabled when viewing character in the menu
			enabled = false;
			return;
		}

		// cache the equip slot transforms
		mountPoints = new Transform[UniRPGGlobal.DB.equipSlots.Count];
		for (int i = 0; i < UniRPGGlobal.DB.equipSlots.Count; i++)
		{
			mountPoints[i] = FindMarkedTransform(UniRPGGlobal.DB.equipSlots[i], _tr);
			//if (mountPoints[i] != null) Debug.Log("Found mount: " + mountPoints[i].name);
		}

		// These must be in Start because it depend on the DB being inited. If a designer run a scene
		// directly then the DB will be init in UniRPGGlobal.Awake, so can't do these in Awake

		// init the skills. all skills that can be mounted must be present in the 
		// SKILLS list and will be added there if not inited from startingSkills

		skills = new List<RPGSkill>(0);
		for (int i = 0; i < startingSkills.Count; i++)
		{
			AddSkill(startingSkills[i]);
		}

		if (actionSlotCount > 0)
		{
			actionSlots = new List<ActionSlot>(actionSlotCount);
			for (int i = 0; i < actionSlotCount; i++)
			{	// create the new action slot
				actionSlots.Add(new ActionSlot());

				// set a skill in it if needed
				if (i < startingSkillSlots.Count)
				{
					SetActionSlot(i, startingSkillSlots[i]);
				}
			}
		}

		// instantiate ActorClass (grab the 1st avail class if none set)
		if (actorClassPrefab == null) actorClassPrefab = UniRPGGlobal.DB.classes[0];
		this.ActorClass = (RPGActorClass)ScriptableObject.Instantiate(actorClassPrefab);
		this.ActorClass.Init(gameObject, startingXP);

		// init the starting states
		for (int i = 0; i < startingStates.Count; i++)
		{
			AddState(startingStates[i]);
		}
	}

	public virtual void SaveState(string key)
	{
		ActorClass.SaveState(key);
		int count = 0;

		// save some misc things
		UniRPGGlobal.SaveInt(key + "act_c", this.currency);

		// save state of bag/inventory
		UniRPGGlobal.SaveInt(key + "act_bags", bagSize);
		if (bagSize > 0)
		{
			count = 0; // keep tract of what was actually saved
			for (int i = 0; i < bag.Count; i++)
			{
				if (i > bagSize) break;
				if (bag[i] != null)
				{
					if (bag[i].item != null)
					{
						//Debug.Log(key + "act_b" + count + " => " + bag[i].item.prefabId.ToString() + "|" + bag[i].stack);
						UniRPGGlobal.SaveString(key + "act_b" + count, bag[i].item.prefabId.ToString() + "|" + bag[i].stack);
						count++;
					}
					else UniRPGGlobal.DeleteSaveKey(key + "act_b" + i); // make sure the key is not set
				}
				else UniRPGGlobal.DeleteSaveKey(key + "act_b" + i); // make sure the key is not set
			}
			UniRPGGlobal.SaveInt(key + "act_bag", count);
		}
		else
		{
			UniRPGGlobal.SaveInt(key + "act_bag", 0);
		}

		// save what skills the actor has
		if (skills.Count > 0)
		{
			UniRPGGlobal.SaveInt(key + "act_skill", skills.Count);
			for (int i = 0; i < skills.Count; i++)
			{
				string save = skills[i].id.ToString() + "|";
				bool found = false;

				// check if the skill is in a slot and save that info too
				for (int j = 0; j < actionSlots.Count; j++)
				{
					if (actionSlots[j].Skill == skills[i])
					{							
						save += j.ToString();
						found = true;  break;
					}
				}
				if (!found) save += "-5";
				UniRPGGlobal.SaveString(key + "act_s" + i, save);
			}
		}
		else UniRPGGlobal.SaveInt(key + "act_skill", 0);

		// save what items are placed into action slots
		count = 0;
		for (int i = 0; i < actionSlots.Count; i++)
		{
			if (actionSlots[i].IsItem)
			{
				count++;
				UniRPGGlobal.SaveString(key + "act_i" + i, actionSlots[i].Item.prefabId.ToString());
			}
			else UniRPGGlobal.DeleteSaveKey(key + "act_i" + i);
		}
		UniRPGGlobal.SaveInt(key + "act_slot_i", count);

		// states
		if (states.Count > 0)
		{
			UniRPGGlobal.SaveInt(key + "act_state", states.Count);
			for (int i = 0; i < states.Count; i++)
			{
				UniRPGGlobal.SaveString(key + "act_st" + i, states[i].id.ToString());
			}
		}
		else UniRPGGlobal.SaveInt(key + "act_state", 0);

		// equipped items		
		if (equipped.Count > 0)
		{
			count = 0;
			for (int i = 0; i < equipped.Count; i++)
			{
				if (equipped[i] != null) // could be null if nothing equipped here
				{
					UniRPGGlobal.SaveString(key + "act_eq" + i, equipped[i].prefabId.ToString());
					count++;
				}
				else UniRPGGlobal.DeleteSaveKey(key + "act_eq" + i); // just in case there was a previous key saved
			}
			UniRPGGlobal.SaveInt(key + "act_eq", count);
		}
		else UniRPGGlobal.SaveInt(key + "act_eq", 0);
		
	}

	public virtual void LoadState(string key)
	{
		bool failed = true; // helper

		// tell actor class to load
		ActorClass.LoadState(key);

		// load some misc things
		this.currency = UniRPGGlobal.LoadInt(key + "act_c", this.currency);

		// load bag/inventory
		bagSize = UniRPGGlobal.LoadInt(key + "act_bags", 0);
		int count = UniRPGGlobal.LoadInt(key + "act_bag", 0);
		if (count > 0 && bagSize > 0)
		{
			bag = new List<BagSlot>(bagSize);
			for (int i = 0; i < bagSize; i++)
			{
				if (i < count)
				{
					failed = true;
					string v = UniRPGGlobal.LoadString(key + "act_b" + i, null);
					if (!string.IsNullOrEmpty(v))
					{
						string[] vs = v.Split('|');
						if (vs.Length == 2)
						{
							GUID id = new GUID(vs[0]);
							if (id != null)
							{								
								if (UniRPGGlobal.DB.RPGItems.ContainsKey(id.Value))
								{
									int cnt = 0;
									if (int.TryParse(vs[1], out cnt))
									{
										BagSlot slot = new BagSlot() { item = UniRPGGlobal.DB.RPGItems[id.Value], stack = cnt };
										bag.Add(slot);
										failed = false;
									}
								}
							}
						}
					}
					if (failed)
					{
						Debug.LogWarning(count+"Actor LoadState BagSlot failed: Key (" + key + "act_b" + i + "), Value (" + v + ")");
						bag.Add(null);
					}
				}
				else
				{	// is simply empty slot
					bag.Add(null);
				}
			}
		} else bag.Clear();

		// load skills
		count = UniRPGGlobal.LoadInt(key + "act_skill", -1);
		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				failed = true;
				string v = UniRPGGlobal.LoadString(key + "act_s" + i, null);
				if (!string.IsNullOrEmpty(v))
				{
					string[] vs = v.Split('|');
					if (vs.Length == 2)
					{
						GameObject skillFab = UniRPGGlobal.DB.GetSkillPrefab(new GUID(vs[0]));
						if (skillFab != null)
						{
							failed = false;
							int slot = -5;
							if (!int.TryParse(vs[1], out slot)) slot = -5;

							if (slot >= 0) SetActionSlot(slot, skillFab);
							else AddSkill(skillFab);
						}
					}
				}
				if (failed)
				{
					Debug.LogWarning("Actor LoadState Skill failed: Key (" + key + "act_s" + i + "), Value (" + v + ")");
				}
			}
		}
		else if (count == 0)
		{
			skills.Clear();
			for (int i = 0; i < actionSlots.Count; i++) actionSlots[i].Clear();
		}

		// load what items are placed into action slots
		count = UniRPGGlobal.LoadInt(key + "act_slot_i", 0);
		for (int i = 0; i < count; i++)
		{
			if (i < actionSlots.Count)
			{
				string s = UniRPGGlobal.LoadString(key + "act_i" + i, null);
				GUID id = new GUID(s);
				if (!id.IsEmpty)
				{
					RPGItem item = UniRPGGlobal.DB.GetItem(id);
					SetActionSlot(i, item);
				}
			}
		}

		// load states
		count = UniRPGGlobal.LoadInt(key + "act_state", 0);
		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				failed = true;
				string v = UniRPGGlobal.LoadString(key + "act_st" + i, null);
				if (!string.IsNullOrEmpty(v))
				{
					RPGState st = UniRPGGlobal.DB.GetState(new GUID(v));
					if (st != null)
					{
						failed = false;
						AddState(st);
					}
				}

				if (failed)
				{
					Debug.LogWarning("Actor LoadState State failed: Key (" + key + "act_st" + i + "), Value (" + v + ")");
				}
			}
		}
		else states.Clear();

		// load equipped
		count = UniRPGGlobal.LoadInt(key + "act_eq", 0);
		if (count > 0)
		{
			equipped.Clear();
			CheckEquipSlotsSize(UniRPGGlobal.DB);
			for (int i = 0; i < equipped.Count; i++)
			{
				string s = UniRPGGlobal.LoadString(key + "act_eq" + i, null);
				if (!string.IsNullOrEmpty(s))
				{
					GUID id = new GUID(s);
					if (UniRPGGlobal.DB.RPGItems.ContainsKey(id.Value))
					{
						equipped[i] = UniRPGGlobal.DB.RPGItems[id.Value];
						// InitSelf() will handle the following
						//UniRPGGameController.ExecuteActions(equipped[i].onEquipActions, equipped[i].gameObject, null, null, gameObject, null, true);
					}
					else
					{
						Debug.LogWarning("Actor LoadState Equipped Item failed: Key (" + key + "act_eq" + i + "), Value (" + s + ")");
					}
				}
			}
		}
		else equipped.Clear();
	}

	private bool initSelf = false;
	private void InitSelf()
	{
		// Check if equip events should have fired for anything that is "equipped" when this character comes into world
		initSelf = true;
		for (int i = 0; i < equipped.Count; i++)
		{
			if (equipped[i] != null)
			{
				// Check if there should be visible item
				EquipVisibleItem(i, equipped[i]);

				// Call equip actions
				UniRPGGameController.ExecuteActions(equipped[i].onEquipActions, equipped[i].gameObject, null, null, gameObject, null, false);
			}
		}
	}

	#endregion
	// ================================================================================================================
	#region Update

	public void Update()
	{
		if (UniRPGGlobal.Instance.state == UniRPGGlobal.State.InMainMenu) return; // needed cause the enabled=false in start is too late and Update will run once at least
		if (!initSelf) return;

		this.ActorClass.Update();
		this.UpdateStates();

		// *** check skills casting
		if (currSkill != null)
		{	// a skill is being performed			
			if (!currSkill.IsCasting)
			{
				_character.DonePerformingSkill(currSkill);
				currSkill = null;	// done				
			}
			return;
		}
		else if (nextSkill != null)
		{	// there is a skill in the queue, execute it if possible						
			if (nextSkill.IsReady)
			{
				// make sure actor is looking at the target and is in range to perform the skill
				if (nextSkillTarget != null)
				{
					nextSkillLocation = nextSkillTarget.transform.position;

					// check if target still valid just in case this skill was queued by auto
					Interactable it = nextSkillTarget.GetComponent<Interactable>();
					if (it != null)
					{
						if (it.canBeTargeted == false)
						{
							ClearQueuedSkill();
							return;
						}
					}
				}
				else nextSkillLocation = transform.position;

				// Make sure queued skill is still valid (target might have become invalid while waiting to perform skill)
				if (nextSkill.targetMech == RPGSkill.TargetingMechanic.SingleTarget || nextSkill.targetMech == RPGSkill.TargetingMechanic.AroundTargeted)
				{
					if (false == nextSkill.IsValidTarget(nextSkillTarget))
					{
						ClearQueuedSkill();
						return;
					}
				}

				// Check if can perform skill
				if (_character.CanPerformSkillNow(nextSkill, nextSkillLocation))
				{
					if (Mathf.Abs((nextSkillLocation - _tr.position).sqrMagnitude) <= nextSkillMaxDistance)
					{	// stop the actor and execute the skill
						_character.AboutToPerformSkill(nextSkill);
						currSkill = nextSkill;
						nextSkill.Use(gameObject, nextSkill.FindTargetsAround(nextSkillLocation, nextSkillTarget), nextSkillLocation);
						nextSkill = null;
						nextSkillTarget = null;
						if (autoSkill != null) UseSkill(autoSkill, autoSkillTarget, true); // queue auto skill
						return;
					}
				}
			}
		}
	}

	public void LateUpdate()
	{
		if (!initSelf)
		{
			InitSelf();
		}
	}

	#endregion
	// ================================================================================================================
	#region Equip slots and Equipped items

	/// <summary>check that enough slots in (equip) is allocated, if not it will be destroyed and inited with correct size</summary>
	public void CheckEquipSlotsSize(Database db)
	{
		if (equipped.Count != db.equipSlots.Count)
		{
			if (equipped.Count > 0)
			{	// there might be equipped items, do not wanna lose 'em
				List<RPGItem> newEquip = new List<RPGItem>(db.equipSlots.Count);
				for (int i = 0; i < db.equipSlots.Count; i++)
				{
					if (i < equipped.Count) newEquip.Add(equipped[i]);
					else equipped.Add(null);
				}
				equipped = newEquip;
			}
			else
			{
				equipped = new List<RPGItem>(db.equipSlots.Count);
				for (int i = 0; i < db.equipSlots.Count; i++) equipped.Add(null);
			}
		}
	}

	/// <summary>return true if item can be equipped to the slot</summary>
	public bool CanEquip(int slot, RPGItem item)
	{
		if (!item.validEquipSlots.Contains(slot)) return false;

		// check states (check if equip on this slot allowed)
		foreach (RPGState state in states)
		{
			if (state == null) continue;
			if (state.effect == RPGState.Effect.PreventEquipSlot)
			{
				if (state.slot == slot)
				{
#if UNITY_EDITOR
					Debug.LogWarning("State prevents equipping an item to this slot.");
#endif
					return false;
				}
			}
		}
		return true;
	}

	/// <summary> 
	/// equip the item to the slot. will do CheckEquipSlotsSize() 1st. return false if equip failed, for example from state preventing equip.
	/// NOTE that this function will unequip an item in the target slot but will NOT put that item in the bag. you must do that manually.
	/// NOTE Do not call this from editor scripts. The editor has its own Equip/UnEquip functions in ActorInspector
	/// </summary>
	public bool Equip(int slot, RPGItem item)
	{
		if (!CanEquip(slot, item)) return false;

		// attempt equip
		CheckEquipSlotsSize(UniRPGGlobal.DB);
		if (slot >= 0 && slot < equipped.Count)
		{
			// 1st "unequip" whatever might be in the slot
			if (equipped[slot] != null) UnEquip(slot);

			// now equip
			equipped[slot] = item;

			// show the visible model if there is a transform marked with slot name in the character
			EquipVisibleItem(slot, item);

			// run the equip action of the item
			UniRPGGameController.ExecuteActions(item.onEquipActions, item.gameObject, null, null, gameObject, null, false);

			return true;
		}
		return false;
	}

	/// <summary>
	/// remove item from slot. this will make equipped.Count = 0 if no more equipped items (if all slots = null) if called from Editor
	/// NOTE Do not call this from editor scripts. The editor has its own Equip/UnEquip fucntions on ActorInspector
	/// </summary>
	public void UnEquip(int slot)
	{
		if (slot >= 0 && slot < equipped.Count)
		{
			if (equipped[slot] == null) return;

			// remove the visible item if any (it will be child of mount point)
			if (mountPoints[slot] != null)
			{
				if (mountPoints[slot].childCount > 0)
				{
					Destroy(mountPoints[slot].GetChild(0).gameObject);
				}
			}

			// run the unequip rules of the item
			UniRPGGameController.ExecuteActions(equipped[slot].onUnEquipActions, equipped[slot].gameObject, null, null, gameObject, null, false);

			// unequip
			equipped[slot] = null;
		}
	}

	public void EquipVisibleItem(int slot, RPGItem item)
	{
		Transform t = mountPoints[slot];
		if (t != null)
		{
			GameObject go = (GameObject)Object.Instantiate(item.gameObject);
			if (go.collider != null) Destroy(go.collider);
			if (go.rigidbody != null) Destroy(go.rigidbody);
			Destroy(go.GetComponent<RPGItem>());

			go.transform.parent = t;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
		}
	}

	public Transform FindMarkedTransform(string tag, Transform t)
	{
		if (t == null) return null;
		if (t.tag.Equals(tag)) return t;

		for (int i = 0; i < t.childCount; i++)
		{
			Transform ret = FindMarkedTransform(tag, t.GetChild(i));
			if (ret != null) return ret;
		}

		return null;
	}

	/// <summary> get name of item on slot, else empty string if none or invalid slot (or word -empty- if param is true)</summary>
	public string GetEquippedName(int slot, bool returnWordEmpty)
	{
		string ret = (returnWordEmpty ? "-" : "");
		if (slot >= 0 && slot < equipped.Count)
		{
			if (equipped[slot] != null)
			{
				if (!string.IsNullOrEmpty(equipped[slot].name)) ret = equipped[slot].name;
			}
		}
		return ret;
	}

	/// <summary> return the item that is equipped on slot, else null if none or invalid slot</summary>
	public RPGItem GetEquippedItem(int slot)
	{
		if (slot >= 0 && slot < equipped.Count)
		{
			return equipped[slot];
		}
		return null;
	}

	#endregion
	// ================================================================================================================
	#region The Bag and Items in the bag

	/// <summary>Used to check if "bag" is inited to correct bagSize</summary>
	public void CheckBagSize()
	{
		if (bag.Count != bagSize)
		{
			if (bag.Count > 0)
			{	// there might be items in the bag, do not want to lose it
				List<BagSlot> newBag = new List<BagSlot>(bagSize);
				for (int i = 0; i < bagSize; i++)
				{
					if (i < bag.Count) newBag.Add(bag[i]);
					else newBag.Add(null);
				}
				bag = newBag;
			}
			else
			{
				bag = new List<BagSlot>(bagSize);
				for (int i = 0; i < bagSize; i++) bag.Add(null);
			}
		}
	}

	/// <summary>Return true if the bag has enough space for the indicated item</summary>
	public bool BagHasSpaceForItem(RPGItem item)
	{
		if (item == null) return false;
		CheckBagSize();

		// 1st check if item can stack and if same type in bag
		if (item.maxStack > 1)
		{
			foreach (BagSlot slot in bag)
			{
				if (slot == null) continue;
				if (slot.stack == 0) continue;

				if (slot.item == item && slot.stack < item.maxStack)
				{	// found a spot where item can be placed
					int canTake = item.maxStack - slot.stack;
					if (canTake > 0) return true;
				}
			}
		}

		// find open slot(s) to place the item into
		for (int slot = 0; slot < bag.Count; slot++)
		{
			if (bag[slot] == null) return true;
			if (bag[slot].stack <= 0) return true;
		}
		return false;
	}

	/// <summary>Find the first slot that contains a copy of the item. return -1 if none found.</summary>
	public int FindInBag(RPGItem item)
	{
		if (item == null) return -1;
		if (bag.Count == 0) return -1;
		for (int i = 0; i < bag.Count; i++)
		{
			if (bag[i].item == null) continue;
			if (bag[i].item.prefabId == item.prefabId) return i;
		}
		return -1;
	}

	/// <summary>
	/// add the item to the first open bag slot. will auto stack if item can stack
	/// will add as many copies of item as possible as indicated by count
	/// </summary>
	public bool AddToBag(RPGItem item, int count)
	{
		return AddToBag(item, count, false);
	}

	/// <summary>
	/// add the item to the first open bag slot. will auto stack if item can stack</summary>
	/// will add as many copies of item as possible as indicated by count
	/// If autoIncSize=true then will increase the bagSize if too small to hold new item (usefull for shopkeeper's bags which dont need set size)
	/// </summary>	
	public bool AddToBag(RPGItem item, int count, bool autoIncSize)
	{
		if (count <= 0 || item == null) return false;

		//bool allGood = false;
		CheckBagSize();

		// 1st check if item can stack and if same type in bag
		if (item.maxStack > 1)
		{
			foreach (BagSlot slot in bag)
			{
				if (count <= 0) break;
				if (slot == null) continue;
				if (slot.stack == 0) continue;

				if (slot.item == item && slot.stack < item.maxStack)
				{	// found a spot where item can be placed
					int canTake = item.maxStack - slot.stack;
					if (count >= canTake)
					{
						count -= canTake;
						slot.stack += canTake;
						UniRPGGameController.ExecuteActions(item.onAddedToBag, item.gameObject, null, gameObject, null, null, false);
						//allGood = true;
					}
					else
					{
						slot.stack += count;
						count = 0;
						UniRPGGameController.ExecuteActions(item.onAddedToBag, item.gameObject, null, gameObject, null, null, false);
						//allGood = true;
						break;
					}
				}
			}
		}

		// find open slot(s) to place the item into
		if (count > 0)
		{
			for (int slot = 0; slot < bag.Count; slot++)
			{
				if (count <= 0) break;

				// cant use null to check if slot is empty cause unity's serialize seems to be creating empty objects for me
				// so i cant be sure that empty slots will be null, but I can do next best thing and check stack
				if (bag[slot] != null)
				{
					if (bag[slot].stack != 0) continue;
				}

				bag[slot] = new BagSlot();
				bag[slot].item = item;
				if (item.maxStack > 1)
				{
					if (count >= item.maxStack)
					{
						count -= item.maxStack;
						bag[slot].stack = item.maxStack;
						UniRPGGameController.ExecuteActions(item.onAddedToBag, item.gameObject, null, gameObject, null, null, false);
						//allGood = true;
					}
					else
					{
						bag[slot].stack = count;
						count = 0;
						UniRPGGameController.ExecuteActions(item.onAddedToBag, item.gameObject, null, gameObject, null, null, false);
						//allGood = true;
						break;
					}
				}
				else
				{
					bag[slot].stack = 1;
					count--;
					UniRPGGameController.ExecuteActions(item.onAddedToBag, item.gameObject, null, gameObject, null, null, false);
					//allGood = true;
				}
			}

			if (autoIncSize && count > 0)
			{
				bagSize++; // simply add a slot and call Add again since it will make call to CheckBagSize();
				AddToBag(item, count, autoIncSize);
			}
		}

		//if (allGood)
		//{
		//	UniRPGGameController.ExecuteActions(item.onAddedToBag, item.gameObject, null, gameObject, null, null, false);
		//}

		return (count <= 0);
	}

	/// <summary>
	/// place item in specified slot. return false if the slot is occupied.
	/// will only allow count>1 if the item can stack
	/// </summary>
	public bool SetBagSlot(RPGItem item, int slot, int count)
	{
		CheckBagSize();

		// again, cant depend on slot being null (see reason inside AddToBag() comments)
		if (bag[slot] != null)
		{
			if (bag[slot].stack != 0) return false;
		}

		int added = count;

		bag[slot] = new BagSlot();
		bag[slot].item = item;
		bag[slot].stack = count;

		// check if stack size allowed
		if (bag[slot].stack > item.maxStack)
		{
			added -= (bag[slot].stack - item.maxStack);
			bag[slot].stack = item.maxStack;
		}

		for (int i = 0; i < added; i++)
		{
			UniRPGGameController.ExecuteActions(bag[slot].item.onAddedToBag, bag[slot].item.gameObject, null, gameObject, null, null, false);
		}

		return true;
	}

	/// <summary>remove the item at slot</summary>
	public void ClearBagSlot(int slot)
	{
		if (slot >= 0 && slot < bag.Count)
		{
			if (bag[slot].item != null)
			{
				for (int i = 0; i < bag[slot].stack; i++)
				{
					UniRPGGameController.ExecuteActions(bag[slot].item.onRemovedFromBag, bag[slot].item.gameObject, null, gameObject, null, null, false);
				}
			}

			bag[slot].item = null;
			bag[slot] = null;

			// now check if there is at least one slot that is not null, else set whole bag as empty
			bool foundOne = false;
			foreach (BagSlot bs in bag)
			{
				if (bs != null)
				{
					if (bs.stack != 0) { foundOne = true; break; }
				}
			}
			if (!foundOne) bag = new List<BagSlot>(0);
		}
	}

	/// <summary>remove a number of items from the slot</summary>
	public void RemoveFromBag(int slot, int count)
	{
		if (count < 0) return;
		if (slot >= 0 && slot < bag.Count)
		{
			if (bag[slot] != null)
			{
				int remed = bag[slot].stack >= count ? count : count - bag[slot].stack;
				for (int i = 0; i < remed; i++)
				{
					if (bag[slot].item != null)
					{
						UniRPGGameController.ExecuteActions(bag[slot].item.onRemovedFromBag, bag[slot].item.gameObject, null, gameObject, null, null, false);
					}
				}

				bag[slot].stack -= count;
				if (bag[slot].stack <= 0)
				{
					bag[slot].item = null;
					bag[slot] = null;
				}

				// now check if there is at least one slot that is not null, else set whole bag as empty
				bool foundOne = false;
				foreach (BagSlot bs in bag)
				{
					if (bs != null)
					{
						if (bs.stack != 0) { foundOne = true; break; }
					}
				}
				if (!foundOne) bag = new List<BagSlot>(0);
			}
		}
	}

	/// <summary>
	/// get name of item on slot, else empty string if none or invalid slot
	/// if allowPrefabName set then the prefab name will be used if item dont have screen name
	/// </summary>
	public string GetBagItemName(int slot, bool allowPrefabName)
	{
		if (slot >= 0 && slot < bag.Count)
		{
			RPGItem item = (bag[slot] == null ? null : bag[slot].item);
			return (item == null ? "" : (string.IsNullOrEmpty(item.screenName) ? (allowPrefabName? item.name : "") : item.screenName));
		}
		return "";
	}

	/// <summary>get stack size in specified slot</summary>
	public int GetBagStackSize(int slot)
	{
		if (slot >= 0 && slot < bag.Count)
		{
			return (bag[slot] == null ? 0 : (bag[slot].item == null ? 0 : bag[slot].stack));
		}
		return 0;
	}

	/// <summary>return the whole slot object at slot</summary>
	public BagSlot GetBagSlot(int slot)
	{
		if (slot >= 0 && slot < bag.Count)
		{
			return bag[slot];
		}
		return null;
	}

	/// <summary>return the item that is equipped on slot, else null if none or invalid slot</summary>
	public RPGItem GetBagItem(int slot)
	{
		if (slot >= 0 && slot < bag.Count)
		{
			return (bag[slot] == null ? null : bag[slot].item);
		}
		return null;
	}

	/// <summary>
	/// tells actor to use the item in the bag slot. this will fail if called 
	/// on items that are equiped from bag and not set to use-from-bag
	/// </summary>
	public void UseBagItem(int slot)
	{
		RPGItem item = GetBagItem(slot);
		if (item == null) return;
		if (item.equipWhenUseFromBag) return;

		// is usable item, run actions
		UniRPGGameController.ExecuteActions(item.onUseActions, item.gameObject, (_character.TargetInteract == null ? null : _character.TargetInteract.gameObject), null, gameObject, null, false);

		// deduct one if consumable
		if (item.consumable) RemoveFromBag(slot, 1);
	}

	#endregion
	// ================================================================================================================
	#region States related

	/// <summary>Add a State to the Actor. Returns false if the State could not be added, for example when the State's maxInstances prevents it.</summary>
	public bool AddState(RPGState state)
	{
		if (state.maxInstances > 0)
		{	// check if not already too many instances of this state active
			int counted = 0;
			for (int i = 0; i < this.states.Count; i++)
			{
				if (this.states[i].id == state.id) counted++;
			}
			if (counted >= state.maxInstances) return false;
		}
		this.states.Add(RPGState.CreateInstance(state, transform));
		return true;
	}

	/// <summary>Removes a State from the Actor. Returns false if there was no such State on the Actor. This removes the 1st State that has the same GUID.</summary>
	public bool RemoveState(RPGState state)
	{
		int idx = -1;
		for (int i = 0; i < this.states.Count; i++)
		{
			if (this.states[i].id == state.id)
			{
				idx = i;
				break;
			}
		}

		if (idx >= 0)
		{
			Destroy(this.states[idx]);
			this.states.RemoveAt(idx);
			return true;
		}

		return false;
	}

	/// <summary>Will call Update() on each State</summary>
	private void UpdateStates()
	{
		for (int i = states.Count-1; i >= 0; i--)
		{
			if (states[i].UpdateState(gameObject))
			{
				Destroy(this.states[i]);
				this.states.RemoveAt(i);
			}
		}
	}

	#endregion
	// ================================================================================================================
	#region Skills related

	public void SubmitAOETarget(Vector3 pos)
	{
		if (AOESkill == null || InAOESelectMode == false)
		{
			Debug.LogError("Call to SubmitAOETarget() when Actor was not expecting it");
			return;
		}

		// Create a dummy target
		if (aoeDummyTr == null)
		{
			GameObject dummy = new GameObject("HELPER_AOE_DUMMY_" + AOESkill.screenName);
			aoeDummyTr = dummy.transform;
		}

		aoeDummyTr.position = pos;
		AOESkill.HideAOEMarker();
		InAOESelectMode = false;
		nextSkill = AOESkill;
		nextSkillTarget = aoeDummyTr.gameObject;
		nextSkillLocation = pos;
		nextSkillMaxDistance = nextSkill.onUseMaxTargetDistance;
		nextSkillMaxDistance = nextSkillMaxDistance * nextSkillMaxDistance; // cause I'm using sqrMagnitude for distance check

	}

	public void CancelAOETargeting()
	{
		if (AOESkill != null)
		{
			AOESkill.HideAOEMarker();
		}

		AOESkill = null;
		InAOESelectMode = false;
	}

	/// <summary>
	/// will queue the skill to be used as soon as current skill is done and/or queued skill is avail for use (cool down might be running)
	/// setAsAutoQueueSkill=true will cause the skill to be queued and performed as long as target is valid or stopped by an action by the player
	/// </summary>
	public void UseSkill(RPGSkill skill, GameObject target, bool setAsAutoQueueSkill)
	{
		if (skill.targetMech == RPGSkill.TargetingMechanic.AroundLocation)
		{
			// the player must first select a spot with the mouse
			InAOESelectMode = true;
			AOESkill = skill;
			nextSkill = null;
			autoSkill = null;
			nextSkillTarget = null;
			autoSkillTarget = null;
		}

		else if ((skill.targetMech == RPGSkill.TargetingMechanic.SingleTarget || skill.targetMech == RPGSkill.TargetingMechanic.AroundTargeted) &&
				skill.validTargetsMask != 0 && ((int)UniRPGGlobal.Target.Self & (int)skill.validTargetsMask) == 0)
		{
			if (skill.IsValidTarget(target))
			{
				if (setAsAutoQueueSkill) { autoSkill = skill; autoSkillTarget = target; }
				nextSkill = skill;
				nextSkillTarget = target;
				if (target == null) nextSkillLocation = transform.position;
				else nextSkillLocation = target.transform.position;
				nextSkillMaxDistance = nextSkill.onUseMaxTargetDistance;
				nextSkillMaxDistance = nextSkillMaxDistance * nextSkillMaxDistance; // cause I'm using sqrMagnitude for distance check				
			}
			else
			{
				nextSkill = null;
				autoSkill = null;
				nextSkillTarget = null;
				autoSkillTarget = null;
				InAOESelectMode = false;
				AOESkill = null;
			}
		}
		else
		{
			if (setAsAutoQueueSkill) { autoSkill = skill; autoSkillTarget = target; }
			nextSkill = skill;
			nextSkillTarget = target;

			if (skill.targetMech == RPGSkill.TargetingMechanic.AroundOwner)
			{
				nextSkillLocation = transform.position; //UniRPGGlobal.Player.transform.position;
			}
			else nextSkillLocation = Vector3.zero;

			if (skill.validTargetsMask == 0 || ((int)UniRPGGlobal.Target.Self & (int)skill.validTargetsMask) != 0)
			{
				nextSkillTarget = ((int)UniRPGGlobal.Target.Self & (int)skill.validTargetsMask) != 0 ? gameObject : null;
				nextSkillLocation = transform.position;
				nextSkillMaxDistance = 0;
			}
			else
			{
				nextSkillMaxDistance = nextSkill.onUseMaxTargetDistance;
				nextSkillMaxDistance = nextSkillMaxDistance * nextSkillMaxDistance; // cause I'm using sqrMagnitude for distance check
			}
		}
	}

	/// <summary>remove queued skill, only if it has the mentioned target</summary>
	public void ClearQueuedSkill(GameObject ifTargeted)
	{
		// only untarget if this target
		if (nextSkillTarget != ifTargeted) return;

		CancelAOETargeting();
		nextSkill = null;
		autoSkill = null;
		nextSkillTarget = null;
		autoSkillTarget = null;
	}

	/// <summary>Cancel queued skill and target. useful for AI to cancel any targeting when the player is out of range</summary>
	public void ClearQueuedSkill()
	{
		CancelAOETargeting();
		nextSkill = null;
		autoSkill = null;
		nextSkillTarget = null;
		autoSkillTarget = null;
	}

	/// <summary>stop current skill and clear next one. does not check if it can be interrupted or not.</summary>
	public void StopAndClearAllSkill()
	{
		CancelAOETargeting();
		if (currSkill != null)
		{
			currSkill.Stop(); // interrupt the skill so that movement can take place
			_character.DonePerformingSkill(currSkill);
		}
		currSkill = null;
		nextSkill = null;
		nextSkillTarget = null;
	}

	/// <summary>returns true if there is an active skill or one in the queue (meaning the character is on its way to perform it)</summary>
	public bool IsPerformingSkill
	{
		get
		{
			return (nextSkill != null || currSkill != null);
		}
	}

	/// <summary>return true if this actor is still casting a the current skill</summary>
	public bool IsCasting
	{
		get
		{
			if (currSkill == null) return false;
			return currSkill.IsCasting;
		}
	}

	/// <summary>Instantiate and add skill to list of available skills. will not add if duplicate</summary>
	public RPGSkill AddSkill(GameObject skillPrefab)
	{
		if (skillPrefab)
		{	// 1st grab the component so that I can extract the ID and also see if this prefab is actually a skill prefab
			RPGSkill skillFabComponent = skillPrefab.GetComponent<RPGSkill>();
			if (skillFabComponent)
			{	// check if the skill is not allready present
				if (!HasSkill(skillFabComponent.id))
				{	// can now instantiate the skill
					GameObject go = (GameObject)Instantiate(skillPrefab);
					go.transform.parent = transform; // make the parent this actor
					RPGSkill skill = go.GetComponent<RPGSkill>();
					skills.Add(skill);
					return skill;
				}
			}
		}
		return null;
	}

	/// <summary>will remove the skill from the list of available skills and from any slot it is mounted and then destroy the instance of the skill</summary>
	public void RemoveSkill(GameObject skillPrefab)
	{
		if (skillPrefab)
		{	// need to extract the ID of the skil lfrom the prefab
			RPGSkill skillFabComponent = skillPrefab.GetComponent<RPGSkill>();
			if (skillFabComponent) RemoveSkill(skillFabComponent.id);
		}
	}

	/// <summary>will remove the skill from the list of available skills and from any slot it is mounted and then destroy the instance of the skill</summary>
	public void RemoveSkill(RPGSkill skill)
	{
		if (skill) RemoveSkill(skill.id);
	}

	/// <summary>will remove the skill from the list of available skills and from any slot it is mounted and then destroy the instance of the skill</summary>
	public void RemoveSkill(GUID skillId)
	{
		if (skillId!=null)
		{
			int found = -1;
			for (int i = 0; i < skills.Count; i++)
			{
				if (skills[i].id == skillId) { found = i; break; }
			}

			if (found >= 0)
			{
				RPGSkill s = skills[found];
				skills.RemoveAt(found);

				// check skill/action slots
				for (int i = 0; i < actionSlots.Count; i++)
				{
					if (actionSlots[i].Skill == s) actionSlots[i].Clear();
				}

				Destroy(s);
			}
		}
	}

	/// <summary>return true of the actor has the mentioned skill in skill list</summary>
	public bool HasSkill(GameObject skillPrefab)
	{
		if (skillPrefab)
		{
			RPGSkill skillFabComponent = skillPrefab.GetComponent<RPGSkill>();
			if (skillFabComponent) return HasSkill(skillFabComponent.id);
		}
		return false;
	}

	/// <summary>return true of the actor has the mentioned skill in skill list</summary>
	public bool HasSKill(RPGSkill skill)
	{
		if (skill) return HasSkill(skill.id);
		return false;
	}

	/// <summary>return true of the actor has the mentioned skill in skill list</summary>
	public bool HasSkill(GUID skillId)
	{
		if (skillId != null)
		{
			for (int i = 0; i < skills.Count; i++)
			{
				if (skills[i].id == skillId) return true;
			}
		}
		return false;
	}

	/// <summary>Get instance of the skill from skill list. return null if actor does not have the skill</summary>
	public RPGSkill GetSkill(GameObject skillPrefab, bool addIfNotFound)
	{
		if (skillPrefab)
		{
			RPGSkill skillFabComponent = skillPrefab.GetComponent<RPGSkill>();
			if (skillFabComponent)
			{
				RPGSkill skill = GetSkill(skillFabComponent.id);
				if (!skill && addIfNotFound) skill = AddSkill(skillPrefab);
				return skill;
			}
		}
		return null;
	}

	/// <summary>Get instance of the skill from skill list. return null if actor does not have the skill</summary>
	public RPGSkill GetSkill(RPGSkill skill)
	{
		if (skill) return GetSkill(skill.id);
		return null;
	}

	/// <summary>Get instance of the skill from skill list. return null if actor does not have the skill</summary>
	public RPGSkill GetSkill(GUID skillId)
	{
		if (skillId != null)
		{
			for (int i = 0; i < skills.Count; i++)
			{
				if (skills[i].id == skillId) return skills[i];
			}
		}
		return null;
	}

	#endregion
	// ================================================================================================================
	#region Action/Skill Slot

	/// <summary>Put the Skill in the slot</summary>
	public bool SetActionSlot(int slot, GameObject skillPrefab)
	{
		if (skillPrefab == null) return false;
		if (slot < 0 || slot >= actionSlots.Count) return false;
		RPGSkill skill = GetSkill(skillPrefab, true);
		return SetActionSlot(slot, skill);
	}

	/// <summary>Put the Skill in the slot. skill must already be instantiated</summary>
	public bool SetActionSlot(int slot, RPGSkill skill)
	{
		if (skill == null) return false;
		if (slot < 0 || slot >= actionSlots.Count) return false;
		actionSlots[slot].SetAsSkill(skill);

		// clear any other slot that has same skill in it
		for (int i = 0; i < actionSlots.Count; i++)
		{
			if (actionSlots[i].Skill == null) continue;
			if (i != slot && actionSlots[i].Skill.id == skill.id) ClearActionSlot(i);
		}

		return true;
	}		

	/// <summary>Put the Item in the slot</summary>
	public bool SetActionSlot(int slot, RPGItem item)
	{
		if (item == null) return false;
		if (slot < 0 || slot >= actionSlots.Count) return false;
		actionSlots[slot].SetAsItem(item);
		return true;
	}

	public void ClearActionSlot(int slot)
	{
		if (slot < 0 || slot >= actionSlots.Count) return;
		actionSlots[slot].Clear();
	}

	public ActionSlot GetActionSlot(int slot)
	{
		if (slot < 0 || slot >= actionSlots.Count) return null;
		return actionSlots[slot];
	}

	/// <summary>Use the Item or Skill in the slot</summary>
	public void UseActionSlot(int slot, GameObject target)
	{
		if (slot < 0 || slot >= actionSlots.Count) return;
		if (actionSlots[slot].IsSkill)
		{
			if (UniRPGGlobal.DB.skillDoubleTapSystemOn && ActorType == UniRPGGlobal.ActorType.Player)
			{
				bool isDoubleTap = false;
				if (lastSlotCall == slot) { isDoubleTap = (Time.time - lastSlotCallTime < UniRPGGlobal.DB.doubleTapTimeout + 1f); }
				lastSlotCall = slot;
				lastSlotCallTime = Time.time;
				UseSkill(actionSlots[slot].Skill, target, isDoubleTap);
			}
			else
			{
				UseSkill(actionSlots[slot].Skill, target, false);
			}
		}

		else if (actionSlots[slot].IsItem)
		{
			// don't do anything is this actor is casting
			if (this.IsCasting) return;
			// first check if it is equip or use item
			if (actionSlots[slot].Item.equipWhenUseFromBag) return;
			// find a copy of this item in the bag
			int bagSlot = FindInBag(actionSlots[slot].Item);
			if (bagSlot >= 0) UseBagItem(bagSlot);
		}
	}

	#endregion
	// ================================================================================================================
} }