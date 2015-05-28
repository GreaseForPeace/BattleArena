// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
[RequireComponent(typeof(Actor))]
public class CharacterBase : Interactable
{
	/// <summary>The Actor associated with this Character. Actors carry a lot of important data that describes the Character</summary>
	public Actor Actor { get { if (_actor == null) _actor = gameObject.GetComponent<Actor>(); return _actor; } }
	protected Actor _actor = null;
	
	/// <summary>The Actor Class associated with this character. The class contains the Attributes like Health, Experience, etc</summary>
	public RPGActorClass ActorClass { get { return Actor.ActorClass; } }

	private Interactable _targetInteract = null;
	public Interactable TargetInteract								// the object that is targeted
	{
		get { return _targetInteract; }
		set
		{
			if (TargetedCharacter != null) OnCharacterTargeted(false, TargetedCharacter);
			if (TargetedItem != null) OnItemTargeted(false, TargetedItem);
			if (TargetedObject != null) OnObjectTargeted(false, TargetedObject);
			if (TargetedLootDrop != null) OnLootDropTargeted(false, TargetedLootDrop);

			TargetedCharacter = null; TargetedItem = null; TargetedObject = null; TargetedLootDrop = null;
			_targetInteract = value;

			if (_targetInteract is CharacterBase)
			{
				TargetedCharacter = (CharacterBase)_targetInteract;
				OnCharacterTargeted(true, TargetedCharacter);
			}
			else if (_targetInteract is RPGItem)
			{
				TargetedItem = (RPGItem)_targetInteract;
				OnItemTargeted(true, TargetedItem);
			}
			else if (_targetInteract is RPGObject)
			{
				TargetedObject = (RPGObject)_targetInteract;
				OnObjectTargeted(true, TargetedObject);
			}
			else if (_targetInteract is RPGLootDrop)
			{
				TargetedLootDrop = (RPGLootDrop)_targetInteract;
				OnLootDropTargeted(true, TargetedLootDrop);
			}
		}
	}
	public CharacterBase TargetedCharacter { get; protected set; }	// will be set if TargetInteract is a Character
	public RPGItem TargetedItem { get; protected set; }				// will be set if TargetInteract is an RPG Item
	public RPGObject TargetedObject { get; protected set; }			// will be set if TargetInteract is an RPG Object
	public RPGLootDrop TargetedLootDrop { get; protected set; }		// will be set if TargetInteract is an RPG Loot Drop

	public SpawnPoint SpawnPoint { get; private set; }				// the spawn point that created this Character (if any)

	protected Transform _tr;
	private bool initing = true;
	private float hitAttPrevVal = 0f;	// a helper for when Getting Hit Events are used	

	// ================================================================================================================
	// plugins should override these

	/// <summary>return the Actor Type (player, hostile, friendly, etc). Should be override by Character classes</summary>
	public virtual UniRPGGlobal.ActorType ActorType() { return 0; }

	/// <summary>called by actor class when a skill is about to be executed</summary>
	public virtual void AboutToPerformSkill(RPGSkill skill) { }

	/// <summary>called by actor class when a skill's execution has stopped</summary>
	public virtual void DonePerformingSkill(RPGSkill skill) { }

	/// <summary>
	/// called by actor to find out if it is ok to execute the skill?
	/// a plugin might want to return false here if it is still 
	/// turning the character to look at the skill's target
	/// </summary>
	public virtual bool CanPerformSkillNow(RPGSkill skill, Vector3 skillTargetLocation) { return true; }

	/// <summary>The character is requested to move to the target position. SpawnPoints might ask for this</summary>
	public virtual void RequestMoveTo(Vector3 position) { }

	/// <summary>Called to find out if the character is idle (not moving)</summary>
	public virtual bool IsIdle() { return true; }

	// ================================================================================================================
	
	// these are called when something becomes targeted or untargeted and can be overwritten to listen for these events
	// a reference to the targeted object will be send. becomeTargeted=false means it was untargeted.

	protected virtual void OnCharacterTargeted(bool becomeTargeted, CharacterBase character) { }
	protected virtual void OnItemTargeted(bool becomeTargeted, RPGItem item) { }
	protected virtual void OnObjectTargeted(bool becomeTargeted, RPGObject obj) { }
	protected virtual void OnLootDropTargeted(bool becomeTargeted, RPGLootDrop lootDrop) { }

	// ================================================================================================================

	/// <summary>The SpawnPoint will call this function to notify the character that this point spawned it</summary>
	public virtual void SetSpawnPoint(SpawnPoint sp)
	{
		this.IsPersistent = false; // do not save character created by a spawn point as there is no real way to restore them when loading
		this.SpawnPoint = sp;
	}

	/// <summary>checks if the character can use the skill. does it need a target and is target in range?</summary>
	public virtual bool IsSkillTargetInRange(RPGSkill s)
	{
		if (s == null) return false;
		if (s.targetMech == RPGSkill.TargetingMechanic.AroundOwner || s.targetMech == RPGSkill.TargetingMechanic.AroundLocation) return true;
		if (s.validTargetsMask == 0) return true; // "Nothing"
		if (s.validTargetsMask == UniRPGGlobal.Target.Self) return true;
		if (((int)UniRPGGlobal.Target.Self & (int)s.validTargetsMask) != 0) return true;
		if (s.validTargetsMask == 0 && TargetInteract == null) return true;
		if (TargetInteract == null) return false;
		if (s.IsValidTarget(TargetInteract.gameObject))
		{
			return (Mathf.Abs((TargetInteract.transform.position - _tr.position).sqrMagnitude) <= (s.onUseMaxTargetDistance * s.onUseMaxTargetDistance));
		}
		return false;
	}

	/// <summary>un-target/ unselect if something is targeted. clears queued skill too</summary>
	public virtual void ClearTarget()
	{
		if (TargetInteract != null)
		{
			_actor.ClearQueuedSkill(TargetInteract.gameObject);
			UniRPGGameController.ExecuteActions(TargetInteract.onClearTargetActions, TargetInteract.gameObject, null, gameObject, null, null, false);
			TargetInteract = null;			
		}
	}

	/// <summary>return true if there is no skill being performed or the active skill can be interrupted by the player</summary>
	public virtual bool NotBussy()
	{
		if (_actor.currSkill != null)
		{
			return _actor.currSkill.ownerCanInterrupt;
		}
		return true;
	}

	// ================================================================================================================

	public override void Awake()
	{
		base.Awake();

		_tr = gameObject.transform;
		_actor = gameObject.GetComponent<Actor>();
		initing = true;
	}

	public virtual void Start()
	{
		// should be called in Start cause UniRPGGlobal.Instance might not be ready in Awake() when scene started via play button
		if (UniRPGGlobal.Instance.state == UniRPGGlobal.State.InMainMenu)
		{	// character should be disabled when in viewed in menu
			IsPersistent = false; // will also set _loading false
			enabled = false;
			initing = false;
			return;
		}
	}

	protected override void SaveState()
	{
		base.SaveState();
		Actor.SaveState(saveKey);
	}

	protected override void LoadState()
	{
		base.LoadState();
		if (UniRPGGlobal.Instance.DoNotLoad) return;

		Actor.LoadState(saveKey);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		// tell the SpawnPoint that this Character (NPC) is being destroyed
		if (SpawnPoint != null) SpawnPoint.RemoveCharacter(this);
	}

	public override void Update()
	{
		if (IsLoading)
		{
			base.Update();
			return;
		}

		if (initing)
		{
			initing = false;
			HookupAttribEvents();
		}
	}

	private void HookupAttribEvents()
	{
		// check if should hook up to Attribute events to determine when to fire Getting Hit and Death
		if (hitAttrib != null)
		{
			RPGAttribute att = _actor.ActorClass.GetAttribute(hitAttrib);
			if (att!=null)
			{
				hitAttPrevVal = att.Value;
				att.onValueChange += OnHitAttribValueChange;
			}
		}

		if (deathAttrib != null)
		{
			RPGAttribute att = _actor.ActorClass.GetAttribute(deathAttrib);
			if (att != null)
			{
				hitAttPrevVal = att.Value;
				att.onValueChange += OnDeathAttribValueChange;
			}
		}
	}

	private void OnHitAttribValueChange(RPGAttribute att)
	{
		if (att.Value < hitAttPrevVal)
		{	// was a hit since value is now less
			hitAttPrevVal = att.Value;
			UniRPGGameController.ExecuteActions(onGetHit, gameObject, null, null, null, null, false);
		}
	}

	private void OnDeathAttribValueChange(RPGAttribute att)
	{
		if (att.Value <= 0)
		{	// was a death since value is now <= (0)
			
			// Stop any skill that might be running
			_actor.StopAndClearAllSkill();

			// Run death event
			UniRPGGameController.ExecuteActions(onDeath, gameObject, null, null, null, null, false);
		}
	}

	// ================================================================================================================
} }