// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class SkillChangeAction : Action
{
	public string skillName;
	public bool addSkill = true; // else remove
	public GameObject skillPrefab = null;

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		SkillChangeAction a = act as SkillChangeAction;
		a.skillName = this.skillName;
		a.addSkill = this.addSkill;
		a.skillPrefab = this.skillPrefab;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		if (skillPrefab == null)
		{
			Debug.LogError("Skill Change Action Error: The Skill is (NULL).");
			return ReturnType.Done;
		}

		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
		if (obj != null)
		{
			Actor actor = obj.GetComponent<Actor>();
			if (actor)
			{
				if (addSkill) actor.AddSkill(skillPrefab);
				else actor.RemoveSkill(skillPrefab);
			}
			else Debug.LogError("Skill Change Action Error: The subject must be an Actor.");
		}
		else Debug.LogError("Skill Change Action Error: The subject did not exist.");
		return ReturnType.Done;
	}

	// ================================================================================================================
} }