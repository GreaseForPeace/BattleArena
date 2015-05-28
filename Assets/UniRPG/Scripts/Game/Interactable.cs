// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class Interactable : UniqueMonoBehaviour 
{
	public float interactDistance = 2f;			// (onInteractActions) how far can object to be that is trying to interact with this?
	public GUID hitAttrib = null;				// (onGetHit) the attribute to watch for onGetHit events
	public GUID deathAttrib = null;				// (onDeath) the attribute to watch for onDeath events

	public bool canBeTargeted = true;			// flag which can be set if this can be targeted

	// Notes on Actions.
	// If an Item can be equipped then it will not have onUseActions since it should create a Skill on the player 
	// via onEquipActions which is removed via onUnEquipActions. For example, a sword do not have onUseActions
	// but should give the player a Skill which can be used to attack with the sword. The skill will carry
	// all the needed onUseActions which will be executed when the player uses the Skill.

	public List<Action> onTargetedActions = new List<Action>(0);	// it was targeted/selected
	public List<Action> onClearTargetActions = new List<Action>(0);	// it was cleared as target
	public List<Action> onInteractActions = new List<Action>(0);	// interaction started with it (pickup for Items)
	public List<Action> onUseActions = new List<Action>(0);			// (only for items) when used - from bag
	public List<Action> onEquipActions = new List<Action>(0);		// (only for items) when equipped
	public List<Action> onUnEquipActions = new List<Action>(0);		// (only for items) when unequipped
	public List<Action> onGetHit = new List<Action>(0);				// (only for characters) when being hit/ attacked (it is triggered when specified attribute decreases)
	public List<Action> onDeath = new List<Action>(0);				// (only for characters) when dead (it is triggered when specified Attribute reaches (0))
	public List<Action> onAddedToBag = new List<Action>(0);			// (only for items)
	public List<Action> onRemovedFromBag = new List<Action>(0);		// (only for items)

	// ================================================================================================================

	public override void Awake()
	{
		base.Awake();
		// disable the action components since they will be manually called as needed
		for (int i = 0; i < onTargetedActions.Count; i++) onTargetedActions[i].enabled = false;
		for (int i = 0; i < onClearTargetActions.Count; i++) onClearTargetActions[i].enabled = false;
		for (int i = 0; i < onInteractActions.Count; i++) onInteractActions[i].enabled = false;
		for (int i = 0; i < onUseActions.Count; i++) onUseActions[i].enabled = false;
		for (int i = 0; i < onEquipActions.Count; i++) onEquipActions[i].enabled = false;
		for (int i = 0; i < onUnEquipActions.Count; i++) onUnEquipActions[i].enabled = false;
		for (int i = 0; i < onGetHit.Count; i++) onGetHit[i].enabled = false;
		for (int i = 0; i < onDeath.Count; i++) onDeath[i].enabled = false;
		for (int i = 0; i < onAddedToBag.Count; i++) onAddedToBag[i].enabled = false;
		for (int i = 0; i < onRemovedFromBag.Count; i++) onRemovedFromBag[i].enabled = false;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		for (int i = 0; i < onTargetedActions.Count; i++) Destroy(onTargetedActions[i]);
		for (int i = 0; i < onClearTargetActions.Count; i++) Destroy(onClearTargetActions[i]);
		for (int i = 0; i < onInteractActions.Count; i++) Destroy(onInteractActions[i]);
		for (int i = 0; i < onUseActions.Count; i++) Destroy(onUseActions[i]);
		for (int i = 0; i < onEquipActions.Count; i++) Destroy(onEquipActions[i]);
		for (int i = 0; i < onUnEquipActions.Count; i++) Destroy(onUnEquipActions[i]);
		for (int i = 0; i < onGetHit.Count; i++) Destroy(onGetHit[i]);
		for (int i = 0; i < onDeath.Count; i++) Destroy(onDeath[i]);
		for (int i = 0; i < onAddedToBag.Count; i++) Destroy(onAddedToBag[i]);
		for (int i = 0; i < onRemovedFromBag.Count; i++) Destroy(onRemovedFromBag[i]);
	}

	// ================================================================================================================
} }