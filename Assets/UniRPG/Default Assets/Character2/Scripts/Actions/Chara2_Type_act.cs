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
	public class Chara2_Type_act : Action
	{
		public UniRPGGlobal.ActorType toType = UniRPGGlobal.ActorType.Hostile;

		public override void CopyTo(Action act)
		{
			base.CopyTo(act);
			Chara2_Type_act a = act as Chara2_Type_act;
			a.toType = this.toType;
		}

		public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
		{
			if (toType == UniRPGGlobal.ActorType.Player)
			{
				Debug.LogError("Character 2: Type Action Error: Player is not a valid type for an NPC.");
			}
			else
			{
				GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
				if (obj != null)
				{
					Chara2_NPC npc = obj.GetComponent<Chara2_NPC>();
					if (npc) npc.actorType = toType;
					else Debug.LogError("Character 2: Type Action Error: The subject is not of type DefaultNPC.");
				}
				else Debug.LogError("Character 2: Type Action Error: The subject did not exist.");
			}
			return ReturnType.Done;
		}

		// ============================================================================================================
	}
}
