// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG
{

//[AddComponentMenu("UniRPG/RPG LootDrop")]
[AddComponentMenu("")]
public class RPGLootDrop : Interactable
{
	public RPGLoot loot { get; set; } // runtime only

	private Spline.Path path;
	private float pathPos = 0f; // 0 .. 1
	private bool playDropAni = false;

	//public override void OnDestroy()
	//{
	//	if (!Application.isLoadingLevel)
	//	{	// the object should be removed from list of recreate-items
	//		UniRPGGlobal.RemoveAutoRecreate(saveKey);
	//	}
	//}

	//protected override void SaveState()
	//{
	//}

	//protected override void LoadState()
	//{
	//	//if (UniRPGGlobal.Instance.DoNotLoad) return;
	//}

	//public void SaveAutoRecreate(string key)
	//{
	//}

	//public void LoadAutoRecreate(string key)
	//{
	//	Debug.Log("LoadAutoRecreate: " + key);
	//}

	public void DoDrop(RPGLoot containedLoot)
	{
		IsPersistent = false; // for now drops can't be saved
		//UniRPGGlobal.RegisterAutoRecreate(typeof(RPGLootDrop), saveKey);

		loot = containedLoot;

		canBeTargeted = false;	// player can't yet target it
		playDropAni = true;

		// calculate drop animation
		float x = Random.Range(0.5f, 1.5f) * (Random.Range(0, 10) > 5 ? -1 : 1);
		float z = Random.Range(0.5f, 1.5f) * (Random.Range(0, 10) > 5 ? -1 : 1);
		x += transform.position.x;
		z += transform.position.z;

		Vector3 mid = new Vector3(x, transform.position.y + 1.5f, z);
		Vector3 end = new Vector3(x, transform.position.y, z);

		// check that end is on the floor
		int mask = (1 << UniRPGGlobal.DB.floorLayerMask);
		RaycastHit hit;
		if (Physics.Raycast(mid, Vector3.down, out hit, 100f, mask))
		{
			end = hit.point;
		}

		// create the path
		path = new Vector3[] { transform.position, mid, end };		
	}

	public override void Update()
	{
		base.Update();

		if (playDropAni)
		{
			transform.position = Spline.MoveOnPath(path, transform.position, ref pathPos, 10f);

			if (pathPos == 1f)
			{
				if (loot == null)
				{	// this component was only used for the drop effect, now destroy it
					IsPersistent = false;
					Destroy(this);
				}
				else
				{
					playDropAni = false;
					canBeTargeted = true;
				}
			} 
		}
	}

	// ================================================================================================================
} }