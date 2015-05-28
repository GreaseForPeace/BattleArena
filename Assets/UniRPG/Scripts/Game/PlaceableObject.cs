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
	[AddComponentMenu("UniRPG/Placable Object")]
	public class PlaceableObject : UniqueMonoBehaviour
	{
		public UniRPGGlobal.Target validTargetsMask = (UniRPGGlobal.Target.Neutral | UniRPGGlobal.Target.Hostile);
		public int maxTargets = 1;	// how many can be effected when triggered
		public float radius = 2f;
		public bool isTimed = true; // timed = true:  will run onTriggerEnterActions actions after timeout, else it will act more like a Trigger object
		public float timeout = 1f;	// execute after so many seconds
		public bool autoDestroy = true;
		public List<Action> actions = new List<Action>(0);

		private float timer = 0f;

		public override void Awake()
		{
			IsPersistent = false;
			for (int i = 0; i < actions.Count; i++) actions[i].enabled = false;
		}

		public void Start()
		{
			if (isTimed)
			{
				timer = timeout;
			}
			else
			{
				SphereCollider c = gameObject.AddComponent<SphereCollider>();
				c.isTrigger = true;
				c.radius = radius;
				if (rigidbody == null) gameObject.AddComponent<Rigidbody>();
				rigidbody.useGravity = false;
				rigidbody.isKinematic = true;
			}
		}

		public override void Update()
		{
			if (!isTimed || timer < 0.0f) return;

			timer -= Time.deltaTime;
			if (timer <= 0.0f)
			{
				// find what is in the trigger area
				List<GameObject> targets = FindTargets();

				for (int i = 0; i < targets.Count; i++)
				{
					UniRPGGameController.ExecuteActions(actions, gameObject, targets[i], null, null, null, false);
				}

				// done
				gameObject.SetActive(false);
				if (autoDestroy) Destroy(gameObject);
			}
		}

		void OnTriggerEnter(Collider c)
		{
			if (isTimed) return; // should not have reached here, but test anyway

			if (IsValidTarget(c.gameObject))
			{
				UniRPGGameController.ExecuteActions(actions, gameObject, c.gameObject, null, null, null, false);

				if (autoDestroy)
				{
					gameObject.SetActive(false);
					Destroy(gameObject);
				}
			}
		}

		private bool IsValidTarget(GameObject obj)
		{
			if (validTargetsMask == 0) return true;

			if (((int)UniRPGGlobal.Target.RPGItem & (int)validTargetsMask) != 0)
			{
				if (obj.GetComponent<RPGItem>() != null)
				{
					return true;
				}
			}

			if (((int)UniRPGGlobal.Target.RPGObject & (int)validTargetsMask) != 0)
			{
				if (obj.GetComponent<RPGObject>() != null)
				{
					return true;
				}
			}

			Actor actor = obj.GetComponent<Actor>();
			if (actor != null)
			{
				if (((int)actor.ActorType & (int)validTargetsMask) != 0)
				{
					return true;
				}
			}

			// reached this point? invalid object.
			return false;
		}

		private List<GameObject> FindTargets()
		{
			if (radius <= 0.0f)
			{
				Debug.LogError("radius <= 0.0f");
				return null;
			}

			Vector3 pos = transform.position;
			pos.y += 1000f;
			RaycastHit[] hits = Physics.SphereCastAll(pos, radius, Vector3.down, 2000f);
			if (hits.Length > 0)
			{
				List<GameObject> ret = new List<GameObject>();
				for (int i = 0; i < hits.Length; i++)
				{
					if (ret.Count >= maxTargets) break;
					if (IsValidTarget(hits[i].collider.gameObject))
					{
						if (!ret.Contains(hits[i].collider.gameObject)) ret.Add(hits[i].collider.gameObject);
					}
				}

				//Debug.Log("Found:");
				//foreach (GameObject go in ret) Debug.Log(go);

				return ret;
			}

			return null;
		}

		// ============================================================================================================
	}
}
