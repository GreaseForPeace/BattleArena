// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG
{
	[AddComponentMenu("")]
	public class Chara2_Move_act : Action
	{
		public float moveSpeed = 0f;
		public float chaseSpeed = 0f;
		public float turnSpeed = 0f;

		public override void CopyTo(Action act)
		{
			base.CopyTo(act);
			Chara2_Move_act a = act as Chara2_Move_act;
			a.moveSpeed = this.moveSpeed;
			a.chaseSpeed = this.chaseSpeed;
			a.turnSpeed = this.turnSpeed;
		}

		public override bool ExecuteWhenRestoringState()
		{
			return false; // not needed since these values are saved and restored by character 2
		}

		public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
		{
			GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
			if (obj == null)
			{
				Debug.LogError("Character2 Movement Action Error: The subject did not exist.");
				return ReturnType.Done;
			}

			Chara2_Player plr = obj.GetComponent<Chara2_Player>();
			if (plr != null)
			{
				plr.moveSpeed = moveSpeed;
				plr.turnSpeed = turnSpeed;
			}
			else
			{
				Chara2_NPC npc = obj.GetComponent<Chara2_NPC>();
				if (npc != null)
				{
					npc.moveSpeed = moveSpeed;
					npc.chaseSpeed = chaseSpeed;
					npc.turnSpeed = turnSpeed;
				}
				else Debug.LogError("Character2 Movement Action Error: The subject must be a Character 2 Player or NPC.");
			}

			return ReturnType.Done;
		}

		// ============================================================================================================
	}
}
