// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class MoveObjectAction : Action
{
	public int moveTo = 0; // 0:position, 1:location of target, 2:a spawnpoint id
	public Vector3 position = Vector3.zero;
	public ActionTarget positionTarget = new ActionTarget();
	public NumericValue num = new NumericValue();
	public bool offset = false;
	public bool faceMoveDirection = false;
	public bool instant = true;
	public float speed = 0f;
	public bool autoDestroy = false;
	public bool doOrient = false;
	public SimpleMove.OrientTo orientTo = SimpleMove.OrientTo.Forward;
	public float orientOffset = 1f;

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		MoveObjectAction a = act as MoveObjectAction;
		a.moveTo = this.moveTo;
		a.position = this.position;
		a.positionTarget = this.positionTarget.Copy();
		a.num = this.num.Copy();
		a.offset = this.offset;
		a.instant = this.instant;
		a.speed = this.speed;
		a.autoDestroy = this.autoDestroy;
		a.faceMoveDirection = this.faceMoveDirection;
		a.doOrient = this.doOrient;
		a.orientTo = this.orientTo;
		a.orientOffset = this.orientOffset;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
		if (obj != null)
		{
			if (moveTo == 0)
			{
				MoveIt(obj, null);
			}
			else if (moveTo == 1)
			{
				GameObject Targetobj = DetermineTarget(positionTarget, self, targeted, selfTargetedBy, equipTarget, helper);
				if (Targetobj != null)
				{
					MoveIt(obj, Targetobj.transform);
				}
				else Debug.LogError("Move Object Action Error: The target location object did not exist.");
			}
			else
			{
				for (int i = 0; i < UniRPGGameController.Instance.SpawnPoints.Length; i++)
				{
					if (UniRPGGameController.Instance.SpawnPoints[i].ident == num.Value)
					{
						MoveIt(obj, UniRPGGameController.Instance.SpawnPoints[i].transform);
						return ReturnType.Done;
					}
				}
				Debug.LogError("Move Object Action Error: The SpawnPoint ("+num.Value+") was not found.");
			}
		}
		else Debug.LogError("Move Object Action Error: The subject did not exist.");
		return ReturnType.Done;
	}

	private void MoveIt(GameObject obj, Transform t)
	{
		if (instant)
		{
			if (t != null)
			{
				obj.transform.position = t.position;
				if (offset)
				{
					obj.transform.Translate(t.right * position.x, Space.World);
					obj.transform.Translate(t.up * position.y, Space.World);
					obj.transform.Translate(t.forward * position.z, Space.World);
				}
			}
			else
			{
				obj.transform.position = position;
			}

			if (doOrient)
			{
				if (orientTo == SimpleMove.OrientTo.Forward) obj.transform.forward = t.forward * orientOffset;
				else if (orientTo == SimpleMove.OrientTo.Right) obj.transform.right = t.right * orientOffset;
				else if (orientTo == SimpleMove.OrientTo.Up) obj.transform.up = t.up * orientOffset;
			}
		}
		else
		{
			SimpleMove m = obj.AddComponent<SimpleMove>();
			m.speed = speed;
			m.destroyAtEnd = autoDestroy;
			m.faceMoveDirection = faceMoveDirection;
			m.doOrient = doOrient;
			m.orientOffset = orientOffset;
			m.orientTo = orientTo;
			m.targetTransform = t;

			if (offset && t != null)
			{
				m.finalPos = t.position + (t.rotation * position);
			}
			else m.finalPos = position; // (position) is used as offset position in this case
		}
	}

	// ================================================================================================================
} }