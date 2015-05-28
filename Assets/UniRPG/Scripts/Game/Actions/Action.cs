// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[System.Serializable]
public class ActionTarget
{
	public Action.TargetType type = Action.TargetType.Self;	// note that not all actions uses this and should choose to display this option in their editor via ActionsEdBase.ShowTargetTypeOption() if they do
	public ObjectValue targetObject = new ObjectValue();	// used when type = TargetType.Specified
	//public int linkedIdx = 1;							// used with TargetType.Linked

	public ActionTarget Copy()
	{
		ActionTarget t = new ActionTarget();
		t.type = this.type;
		t.targetObject = this.targetObject.Copy();
		//t.linkedIdx = this.linkedIdx;
		return t;
	}

	public new string ToString()
	{
		return type.ToString();
	}
}

[AddComponentMenu("")]
public class Action: MonoBehaviour
{
	// NOTE: I tried System.Object and even ScriptableObject but could not get Unity to serialize the
	// Action objects that derive from this base class. The ScriptableObject needs to be assets and
	// I can't detect when duplicates of objects are made in the scene (or prefabs) so I can't
	// create unique copies of the Action for these duped objects. There is kind of a way to detect
	// duplication but is way too much effort. So MonoBehaviour it is.
	// http://forum.unity3d.com/threads/12849-Unhappy-Derived-Types!?p=89770&viewfull=1#post89770

	// NOTE: Actions will not be executed in EDIT MODE

	/// <summary>
	/// See the documentation for the queue list that the action is placed into to
	/// see what the following types mean and when they will be valid
	/// </summary>
	public enum TargetType
	{
		Self = 0,			// normally the character that "owns" the action but there might be cases where it is another object type
		Targeted = 1,		// the targeted object/character
		TargetedBy = 2,		// the character targeting the "self" - not many cases where this is set and valid
		Specified = 3,		// always the specified object/prefab, except if that object is in another scene, in which case it will be invalid value
		EquipTarget = 4,	// only valid during equip/unequip actions (the character being equiped/unequiped)
		Player = 5,			// always UniRPGGlobal.Player
		Helper = 6,			// Helper object that might be set or not depending on situation. For Skills this would point to Skill object itself
	}

	/// <summary>
	/// The Action's return type will determine what will be done next in the Action Queue of the Event
	/// </summary>
	public enum ReturnType
	{
		Done = 0,		// The action is done
		CallAgain=1,	// the acton wants to be called again next frame
		Exit = 2,		// Stop executing the action list		
		SkipNext=3,		// this will cause the next action to be skipped and the one after it will be executed next

		// An action can return a number higher than 100 to indicate which action should be executed next in the list.
		// It counts from the first action being (1). Example, if you wanted to have action 5 executed next then your
		// action would simply return ExecuteSpecificNext+5; The IfThenAction uses this.
		ExecuteSpecificNext=100,
	}

	public ActionTarget subject = new ActionTarget();

	/// <summary>Should be overwitten to copy the properties of the Action to a target Action.</summary>
	/// <param name="act">The action that should be copied to.</param>
	public virtual void CopyTo(Action act)
	{
		act.subject = this.subject.Copy();
	}

	/// <summary>
	/// Actions that wants to be executed when the LoasdSave System is restoring the state of something should return true here.
	/// Normally an Action will not want to run but there might be cases where it is useful. Currently only Actions in the 
	/// On Equip Event will be called when the system restores the items that where equipped on a Character
	/// </summary>
	public virtual bool ExecuteWhenRestoringState()
	{
		return false;
	}

	/// <summary>Called before the action's Execute will be called. Actions can use it to reset vars as needed.</summary>
	public virtual void Init(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
	}

	/// <summary>
	/// called when it is time to run the action. actions are called in order and the action system
	/// will move on to the next/ previous or exit, depending on what this action returns
	/// </summary>
	public virtual ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		return ReturnType.Done;
	}

	/// <summary>will determine the target GameObject based on what "subject" is set to</summary>
	protected GameObject DetermineTarget(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		return DetermineTarget(subject, self, targeted, selfTargetedBy, equipTarget, helper);
	}

	/// <summary>will determine the target GameObject based on what targetInfo is set to</summary>
	protected GameObject DetermineTarget(ActionTarget targetInfo, GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		GameObject obj = null;

		if (targetInfo.type == TargetType.Helper) obj = helper;
		else if (targetInfo.type == TargetType.Player) obj = UniRPGGlobal.Player.gameObject;
		else if (targetInfo.type == TargetType.Specified)
		{
			if (targetInfo.targetObject.Value == null) return null;
			obj = (GameObject)targetInfo.targetObject.Value;
		}
		else if (targetInfo.type == TargetType.Self) obj = self;
		else if (targetInfo.type == TargetType.Targeted) obj = targeted;
		else if (targetInfo.type == TargetType.TargetedBy) obj = selfTargetedBy;
		else if (targetInfo.type == TargetType.EquipTarget) obj = equipTarget;

		return obj;
	}

	// ================================================================================================================
} }