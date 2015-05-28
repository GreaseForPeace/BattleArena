// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class CreateLootAction : Action
{
	public GUID lootId;

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		CreateLootAction a = act as CreateLootAction;
		a.lootId = this.lootId.Copy();
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		RPGLoot loot = UniRPGGlobal.DB.GetLoot(lootId);
		if (loot == null)
		{
			Debug.LogError("Create Loot Action Error: The specified Loot Table could not be found.");
			return ReturnType.Done;
		}

		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
		if (obj != null)
		{
			Actor actor = obj.GetComponent<Actor>();
			if (actor)
			{	// can send a level
				RPGLoot.CreateLootDrops(loot, obj.transform.position, actor.ActorClass.Level);
			}
			else
			{	// cant send level
				RPGLoot.CreateLootDrops(loot, obj.transform.position, 0);
			}
		}
		else Debug.LogError("Create Loot Action Error: The subject did not exist.");
		return ReturnType.Done;
	}

	// ================================================================================================================
} }
