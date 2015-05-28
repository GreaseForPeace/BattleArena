// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("UniRPG/Trigger")]
public class Trigger : UniqueMonoBehaviour 
{
	public enum AreaKind
	{
		None,
		Rectangular,
		Circular
	}

	public AreaKind areaKind = AreaKind.None;

	// Events to call when event happens

	public List<Action> onLoaded = new List<Action>(0);
	public List<Action> onPlayerEnter = new List<Action>(0);	// only Player
	public List<Action> onPlayerLeave = new List<Action>(0);	// only Player
	public List<Action> onPlayerStay = new List<Action>(0);		// only Player
	public List<Action> onNPCEnter = new List<Action>(0);		// only NPCs
	public List<Action> onNPCLeave = new List<Action>(0);		// only NPCs
	public List<Action> onNPCStay = new List<Action>(0);		// only NPCs

	private bool performedFirstRun = false;

	// ================================================================================================================
	// Runtime

	public override void Awake()
	{
		autoSaveLoadEnabled = false;
		autoSaveDestroy = false;
		performedFirstRun = false;

		base.Awake();
		// disable the action components since they will be manually called as needed
		for (int i = 0; i < onLoaded.Count; i++) onLoaded[i].enabled = false;
		for (int i = 0; i < onPlayerEnter.Count; i++) onPlayerEnter[i].enabled = false;
		for (int i = 0; i < onPlayerLeave.Count; i++) onPlayerLeave[i].enabled = false;
		for (int i = 0; i < onPlayerStay.Count; i++) onPlayerStay[i].enabled = false;
		for (int i = 0; i < onNPCEnter.Count; i++) onNPCEnter[i].enabled = false;
		for (int i = 0; i < onNPCLeave.Count; i++) onNPCLeave[i].enabled = false;
		for (int i = 0; i < onNPCStay.Count; i++) onNPCStay[i].enabled = false;
	}

	void Start()
	{
		// create the trigger area if this is such a type of trigger

		if (areaKind == AreaKind.Circular)
		{
			SphereCollider col = gameObject.AddComponent<SphereCollider>();
			col.isTrigger = true;
			col.radius = 1f; // it scales with the transform's localScale
		}
		else if (areaKind == AreaKind.Rectangular)
		{
			BoxCollider col = gameObject.AddComponent<BoxCollider>();
			col.isTrigger = true;
			col.size = Vector3.one; // one cause the localScale of the object is allready used
		}
	}

	private void PerformFirstRun()
	{
		performedFirstRun = true;
		bool mustBeActive = false;

		if (areaKind != AreaKind.None)
		{
			// check if there are any other events and enable the trigger if so
			if (onPlayerEnter.Count > 0) mustBeActive = true;
			else if (onPlayerLeave.Count > 0) mustBeActive = true;
			else if (onPlayerStay.Count > 0) mustBeActive = true;
			else if (onNPCEnter.Count > 0) mustBeActive = true;
			else if (onNPCLeave.Count > 0) mustBeActive = true;
			else if (onNPCStay.Count > 0) mustBeActive = true;
		}

		if (mustBeActive == false)
		{
			if (onLoaded.Count > 0)
			{	// onLoad needs to run so tell executer to disable this object when it is done with onLoad
				UniRPGGameController.ExecuteActions(onLoaded, gameObject, null, null, null, gameObject, false);
			}
			else
			{	// this is a trigger with no evnets, disable the whole thing now
				gameObject.SetActive(false);
			}
		}
		else
		{	// run the onLoaded events. note that the executer will nto disable this gameObject 
			// since there are other events that could potentially eb triggered
			UniRPGGameController.ExecuteActions(onLoaded, gameObject, null, null, null, null, false);
		}
	}

	public override void Update()
	{
		if (performedFirstRun) return;

		if (IsLoading)
		{
			base.Update();
			return;
		}

		PerformFirstRun();
	}

	void OnTriggerEnter(Collider c)
	{
		if (onPlayerEnter.Count == 0 && onNPCEnter.Count == 0) return;
		Actor a = c.GetComponent<Actor>();
		if (a == null) return; // was not an actor

		if (onPlayerEnter.Count > 0)
		{	// player entered
			if (a.ActorType == UniRPGGlobal.ActorType.Player) UniRPGGameController.ExecuteActions(onPlayerEnter, gameObject, a.gameObject, null, null, null, false);
		}
		if (onNPCEnter.Count > 0)
		{	// npc entered
			if (a.ActorType != UniRPGGlobal.ActorType.Player) UniRPGGameController.ExecuteActions(onNPCEnter, gameObject, a.gameObject, null, null, null, false);
		}
	}

	void OnTriggerExit(Collider c)
	{
		if (onPlayerLeave.Count == 0 && onNPCLeave.Count == 0) return;
		Actor a = c.GetComponent<Actor>();
		if (a == null) return; // was not an actor

		if (onPlayerLeave.Count > 0)
		{	// player
			if (a.ActorType == UniRPGGlobal.ActorType.Player) UniRPGGameController.ExecuteActions(onPlayerLeave, gameObject, a.gameObject, null, null, null, false);
		}
		if (onNPCLeave.Count > 0)
		{	// npc
			if (a.ActorType != UniRPGGlobal.ActorType.Player) UniRPGGameController.ExecuteActions(onNPCLeave, gameObject, a.gameObject, null, null, null, false);
		}
	}

	void OnTriggerStay(Collider c)
	{
		if (onPlayerStay.Count == 0 && onNPCStay.Count == 0) return;
		Actor a = c.GetComponent<Actor>();
		if (a == null) return; // was not an actor

		if (onPlayerStay.Count > 0)
		{	// player
			if (a.ActorType == UniRPGGlobal.ActorType.Player) UniRPGGameController.ExecuteActions(onPlayerStay, gameObject, a.gameObject, null, null, null, false);
		}
		if (onNPCStay.Count > 0)
		{	// npc
			if (a.ActorType != UniRPGGlobal.ActorType.Player) UniRPGGameController.ExecuteActions(onNPCStay, gameObject, a.gameObject, null, null, null, false);
		}
	}

	// ================================================================================================================

#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;

		// the main gizmo
		Gizmos.DrawLine(transform.position - Vector3.up * 0.1f, transform.position + Vector3.up * 2f);
		Gizmos.DrawCube(transform.position - Vector3.up * 0.1f, new Vector3(0.1f, 0.1f, 0.1f));
		Gizmos.DrawSphere(transform.position + Vector3.up * 2f, 0.25f);
		Gizmos.DrawIcon(transform.position + Vector3.up * 2f, "UniRPG_trigger.png");
	}

	void OnDrawGizmosSelected()
	{
		if (areaKind == AreaKind.Circular)
		{
			float r = Mathf.Max(transform.localScale.x, Mathf.Max(transform.localScale.y, transform.localScale.z));
			Gizmos.color = new Color(1f, 0f, 1f, 0.15f);
			Gizmos.DrawSphere(transform.position, r);
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(transform.position, r);
		}

		else if (areaKind == AreaKind.Rectangular)
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = new Color(1f, 0f, 1f, 0.15f);
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	}
#endif

	// ================================================================================================================
} }