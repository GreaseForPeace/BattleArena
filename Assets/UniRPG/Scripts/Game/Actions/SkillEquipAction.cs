// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class SkillEquipAction : Action
{
	public string skillName;
	public bool equipSkill = true; // else remove
	public GameObject skillPrefab = null;
	public int equipToSlot = 0; // 0..n:slot (set if you also want to equip at same time being added to skill list)

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		SkillEquipAction a = act as SkillEquipAction;
		a.skillName = this.skillName;
		a.equipSkill = this.equipSkill;
		a.skillPrefab = this.skillPrefab;
		a.equipToSlot = this.equipToSlot;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		if (skillPrefab == null && equipSkill == true)
		{
			Debug.LogError("Skill Equip Action Error: The Skill is (NULL).");
			return ReturnType.Done;
		}

		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
		if (obj != null)
		{
			Actor actor = obj.GetComponent<Actor>();
			if (actor)
			{
				if (equipSkill)
				{
					if (false == actor.SetActionSlot(equipToSlot, skillPrefab))
					{
						Debug.LogError("Skill Equip Action Error: Failed to Equip to Slot: " + equipToSlot);
					}
				}
				else
				{
					actor.ClearActionSlot(equipToSlot);
				}
			}
			else Debug.LogError("Skill Equip Action Error: The subject must be an Actor.");
		}
		else Debug.LogError("Skill Equip Action Error: The subject did not exist.");
		return ReturnType.Done;
	}

	// ================================================================================================================
} }