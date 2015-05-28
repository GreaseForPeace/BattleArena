// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG
{
	[AddComponentMenu("")]
	public class CollidableAction : Action
	{
		public ObjectValue prefab = new ObjectValue();
		public bool triggerWhenReachTarget = true;
		public Vector3 offsetTargetPosition = Vector3.zero;
		public string setGlobalObjectVar = null;
		public ActionTarget spawnFrom = new ActionTarget();
		public Vector3 offsetFrom = Vector3.zero;
		public float speed = 10f;
		public bool selfDestructWhenNoTarget = true;

		public override void CopyTo(Action act)
		{
			base.CopyTo(act);
			CollidableAction a = act as CollidableAction;
			a.prefab = this.prefab.Copy();
			a.triggerWhenReachTarget = this.triggerWhenReachTarget;
			a.offsetTargetPosition = this.offsetTargetPosition;
			a.setGlobalObjectVar = this.setGlobalObjectVar;
			a.spawnFrom = this.spawnFrom.Copy();
			a.offsetFrom = this.offsetFrom;
			a.speed = this.speed;
			a.selfDestructWhenNoTarget = this.selfDestructWhenNoTarget;
		}

		public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
		{
			if (prefab.Value == null)
			{
				Debug.LogError("Collidable Create Error: Invalid prefab specified.");
				return ReturnType.Done;
			}

			if (((GameObject)prefab.Value).GetComponent<CollidableObject>() == null)
			{
				Debug.LogError("Collidable Create Error: Invalid prefab specified. It must be a Collidable Object.");
				return ReturnType.Done;
			}

			GameObject targetObject = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
			if (targetObject == null)
			{
				Debug.LogError("Collidable Create Error: The target does not exist.");
				return ReturnType.Done;
			}

			GameObject spawnTargetTR = DetermineTarget(spawnFrom, self, targeted, selfTargetedBy, equipTarget, helper);

			GameObject go = (GameObject)GameObject.Instantiate(prefab.Value);

			if (!string.IsNullOrEmpty(setGlobalObjectVar))
			{
				UniRPGGlobal.DB.SetGlobalObject(setGlobalObjectVar, go);
			}

			if (spawnTargetTR != null)
			{
				go.transform.position = spawnTargetTR.transform.position;
				go.transform.Translate(spawnTargetTR.transform.right * offsetFrom.x, Space.World);
				go.transform.Translate(spawnTargetTR.transform.up * offsetFrom.y, Space.World);
				go.transform.Translate(spawnTargetTR.transform.forward * offsetFrom.z, Space.World);
			}
			else
			{
				go.transform.position = offsetFrom;
			}

			CollidableObject c = go.GetComponent<CollidableObject>();
			c.triggerWhenReachTarget = triggerWhenReachTarget;
			c.targetObject = targetObject.transform;
			c.offsetTargetPosition = offsetTargetPosition;
			c.selfDestructWhenNoTarget = selfDestructWhenNoTarget;

			SimpleMove m = go.AddComponent<SimpleMove>();
			m.speed = speed;
			m.follow = targetObject.transform;
			m.finalPos = offsetTargetPosition;

			return ReturnType.Done; // this action is done
		}

		// ================================================================================================================
	}
}