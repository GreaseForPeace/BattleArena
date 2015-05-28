// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class SpawnAction : Action
{
	public ObjectValue prefab = new ObjectValue(); // prefab to instantiate
	public Vector3 rotation = Vector3.zero;
	public Vector3 position = Vector3.zero;
	
	public int doParent = 0;						// 0:no, 1:parent, 2:only offset

	public bool localPos = true;					// if set then the rot/pos is local
	public bool localRot = true;					// if set then the rot/pos is local
	public string setGlobalObjectVar = null;		// if set then this var will contain a reference to the new object that was created
	public float autoDestroyTimeout = 0f;			// will auto destroy object with > 0
	public bool persistItem = false;

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		SpawnAction a = act as SpawnAction;
		a.prefab = this.prefab;
		a.rotation = this.rotation;
		a.position = this.position;
		a.doParent = this.doParent;
		a.localPos = this.localPos;
		a.localRot = this.localRot;
		a.autoDestroyTimeout = this.autoDestroyTimeout;
		a.persistItem = this.persistItem;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		GameObject toCreate = prefab.Value as GameObject;
		if (toCreate)
		{
			GameObject go = (GameObject)GameObject.Instantiate(toCreate, position, Quaternion.Euler(rotation));

			if (go != null)
			{
				UniqueMonoBehaviour ub = go.GetComponent<UniqueMonoBehaviour>();
				if (ub != null)
				{
					ub.IsPersistent = false;

					if (persistItem)
					{
						RPGItem item = go.GetComponent<RPGItem>();
						if (item != null) UniRPGGlobal.RegisterItemForAutoSaving(item);
					}
				}

				if (autoDestroyTimeout > 0.0f)
				{
					GameObject.Destroy(go, autoDestroyTimeout);
				}

				if (!string.IsNullOrEmpty(setGlobalObjectVar))
				{
					UniRPGGlobal.DB.SetGlobalObject(setGlobalObjectVar, go);
				}

				if (doParent != 0 && go)
				{
					GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
					if (obj)
					{
						if (doParent == 1)
						{
							go.transform.parent = obj.transform;
							if (localPos) go.transform.localPosition = position;
							if (localRot) go.transform.localRotation = Quaternion.Euler(rotation);
						}
						else
						{
							go.transform.position = obj.transform.position;
							go.transform.Translate(obj.transform.right * position.x, Space.World);
							go.transform.Translate(obj.transform.up * position.y, Space.World);
							go.transform.Translate(obj.transform.forward * position.z, Space.World);
							//go.transform.position = obj.transform.position;
							//if (localPos) go.transform.localPosition += position;
						}
					}
					else Debug.LogError("Spawn Action Error: The Parent object could not be found.");
				}
			}
		}
		else Debug.LogError("Spawn Action Error: The Prefab must be set.");
		return ReturnType.Done;
	}

	// ================================================================================================================
} }