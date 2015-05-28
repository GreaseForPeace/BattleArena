// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

#if UNIRPG_CORE

using UnityEngine;
using UniRPG;

namespace DiaQ
{
	[AddComponentMenu("")]
	public class DiaQRewardAction : Action
	{
		public string questIdent;
		public string questName = "-";
		public bool checkIfAccepted = true;
		public bool checkIfCompleted = true;
		public bool checkIfHandedIn = true;

		public override void CopyTo(Action act)
		{
			base.CopyTo(act);
			DiaQRewardAction a = act as DiaQRewardAction;
			a.questIdent = this.questIdent;
			a.questName = this.questName;
			a.checkIfAccepted = this.checkIfAccepted;
			a.checkIfCompleted = this.checkIfCompleted;
			a.checkIfHandedIn = this.checkIfHandedIn;
		}

		public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
		{
			if (!string.IsNullOrEmpty(questIdent))
			{
				DiaQuest q = DiaQEngine.Instance.FindDefinedQuest(questIdent);
				if (q != null)
				{
					if (checkIfAccepted && q.IsAccepted == false) return ReturnType.Done;
					if (checkIfCompleted && q.IsCompleted == false) return ReturnType.Done;
					if (checkIfHandedIn && q.HandedIn == true) return ReturnType.Done;

					// can now hand out rewards
					GiveRewards(q);
				}
				else Debug.LogError("DiaQ Reward Action Error: Quest not defined");
			}
			else Debug.LogError("DiaQ Reward Action Error: No Quest specified");

			return ReturnType.Done;
		}

		private void GiveRewards(DiaQuest q)
		{
			// first set that quest was handed-in
			q.HandIn(true);

			// now hand out reward(s)
			foreach (DiaQuest.Reward r in q.rewards)
			{
				if (r.type == DiaQuest.Reward.Type.Currency)
				{
					UniRPGGlobal.Player.Actor.currency += r.value;
					if (UniRPGGlobal.Player.Actor.currency < 0) UniRPGGlobal.Player.Actor.currency = 0;
				}

				else if (r.type == DiaQuest.Reward.Type.Attribute)
				{
					if (!string.IsNullOrEmpty(r.ident))
					{
						RPGAttribute a = UniRPGGlobal.Player.Actor.ActorClass.GetAttribute(new GUID(r.ident));
						if (a != null)
						{
							a.Value += r.value;
						}
						else Debug.LogError("DiaQ Reward Action Error: The Player does not have the Attribute.");
					}
					else Debug.LogError("DiaQ Reward Action Error: No Attribute was specified.");
				}

				else if (r.type == DiaQuest.Reward.Type.Item)
				{
					if (r.value < 1)
					{
						Debug.LogError("DiaQ Reward Action Error: You must specify a number of (1) or higher for Item type rewards.");
					}
					else
					{
						if (!string.IsNullOrEmpty(r.ident))
						{
							RPGItem it = UniRPGGlobal.DB.GetItem(new GUID(r.ident));
							if (it != null)
							{
								UniRPGGlobal.Player.Actor.AddToBag(it, r.value);
							}
							else Debug.LogError("DiaQ Reward Action Error: The specified Item was not found in the Database.");
						}
						else Debug.LogError("DiaQ Reward Action Error: No Item was specified.");
					}
				}
			}
		}

		// ================================================================================================================
	}
}

#endif