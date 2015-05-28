// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRPG
{
	[AddComponentMenu("UniRPG/Collidable Object")]
	public class CollidableObject : UniqueMonoBehaviour
	{
		public float stayCallRate = 1f;	// in seconds
		public UniRPGGlobal.Target validTargetsMask = (UniRPGGlobal.Target.Neutral | UniRPGGlobal.Target.Hostile);

		public List<Action> onTriggerEnterActions = new List<Action>(0);
		public List<Action> onTriggerExitAction = new List<Action>(0);
		public List<Action> onTriggerStayAction = new List<Action>(0);

		// these can be modified by creation the Collidable with the CollidableAction
		public Transform targetObject = null;				// the object to follow or move towards
		public bool triggerWhenReachTarget = false;			// the object will always trigger onTriggerEnterActions when it reached the target
		public Vector3 offsetTargetPosition = Vector3.zero;	// don't want it to go for the base/pivot/position of the target?
		public bool selfDestructWhenNoTarget = false;

		// ============================================================================================================

		private float stayTimer = 0f;
		private List<GameObject> cachedValidObject = new List<GameObject>();
		private List<GameObject> cachedInvalidObject = new List<GameObject>();
		private Interactable interactObj = null;

		// ============================================================================================================

		public override void Awake()
		{
			IsPersistent = false;

			for (int i = 0; i < onTriggerEnterActions.Count; i++) onTriggerEnterActions[i].enabled = false;
			for (int i = 0; i < onTriggerExitAction.Count; i++) onTriggerExitAction[i].enabled = false;
			for (int i = 0; i < onTriggerStayAction.Count; i++) onTriggerStayAction[i].enabled = false;
		}

		public void Start()
		{
			Collider c = GetComponent<Collider>();
			if (c == null)
			{	// must have collider to detect collisions
				if (!triggerWhenReachTarget)
				{
					Debug.LogError("CollidableObject must have a Collider.");
					enabled = false;
					return;
				}
			}
			else
			{
				if (rigidbody == null) gameObject.AddComponent<Rigidbody>();
				rigidbody.useGravity = false;	// not using rigidbody for its physics in this way
				rigidbody.isKinematic = true;	// not using rigidbody for its physics in this way
				c.isTrigger = true;				// make sure it is trigger type collider since OnTriggerXxxx is used
			}

			if (targetObject != null)
			{
				interactObj = (Interactable)targetObject.GetComponent<Interactable>();
			}
		}

		void LateUpdate()
		{
			if (triggerWhenReachTarget && targetObject != null)
			{	// manually check if reached target
				//if (transform.position == targetObject.position + offsetTargetPosition)
				if (Vector3.Distance(transform.position, targetObject.position + offsetTargetPosition) <= 0.01f)
				{
					if (IsValidTarget(targetObject.gameObject))
					{
						UniRPGGameController.ExecuteActions(onTriggerEnterActions, gameObject, targetObject.gameObject, null, null, null, false);
					}
				}
			}

			if (selfDestructWhenNoTarget)
			{
				if (targetObject == null) GameObject.Destroy(gameObject);
				else if (interactObj != null)
				{
					if (interactObj.canBeTargeted == false) GameObject.Destroy(gameObject);
				}
			}
		}

		void OnTriggerEnter(Collider c)
		{
			if (onTriggerEnterActions.Count > 0)
			{
				if (IsValidTarget(c.gameObject))
				{
					UniRPGGameController.ExecuteActions(onTriggerEnterActions, gameObject, c.gameObject, null, null, null, false);
				}
			}
		}

		void OnTriggerExit(Collider c)
		{
			if (onTriggerExitAction.Count > 0)
			{
				if (IsValidTarget(c.gameObject))
				{
					UniRPGGameController.ExecuteActions(onTriggerExitAction, gameObject, c.gameObject, null, null, null, false);
				}
			}
		}

		void OnTriggerStay(Collider c)
		{
			if (onTriggerStayAction.Count > 0)
			{
				if (IsValidTarget(c.gameObject))
				{
					stayTimer -= Time.deltaTime;
					if (stayTimer < 0.0f)
					{
						stayTimer = stayCallRate;
						UniRPGGameController.ExecuteActions(onTriggerStayAction, gameObject, c.gameObject, null, null, null, false);
					}
				}
			}
		}

		private bool IsValidTarget(GameObject obj)
		{
			if (triggerWhenReachTarget)
			{	// only the specified target is valid
				if (targetObject == null) return false;
				if (obj != targetObject.gameObject) return false;
			}

			if (validTargetsMask == 0) return true;					// nothing to test, everything is considered fine
			if (cachedValidObject.Contains(obj)) return true;		// previously found to be valid, no need to test again
			if (cachedInvalidObject.Contains(obj)) return false;	// previously found to be invalid, no need to test again

			if (((int)UniRPGGlobal.Target.RPGItem & (int)validTargetsMask) != 0)
			{
				if (obj.GetComponent<RPGItem>() != null)
				{
					cachedValidObject.Add(obj);
					return true;
				}
			}

			if (((int)UniRPGGlobal.Target.RPGObject & (int)validTargetsMask) != 0)
			{
				if (obj.GetComponent<RPGObject>() != null)
				{
					cachedValidObject.Add(obj);
					return true;
				}
			}

			Actor actor = obj.GetComponent<Actor>();
			if (actor != null)
			{
				if (((int)actor.ActorType & (int)validTargetsMask) != 0)
				{
					cachedValidObject.Add(obj);
					return true;
				}
			}

			// reached this point? invalid object.
			cachedInvalidObject.Add(obj);
			return false;
		}

		// ============================================================================================================
	}
}