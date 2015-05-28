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
	public class DiaQuestAction : Action
	{
		public string questIdent;
		public string questName="-";
		public bool give = true;
		public bool setCompleted = false;
		public bool setHadedIn = false;

		public override void CopyTo(Action act)
		{
			base.CopyTo(act);
			DiaQuestAction a = act as DiaQuestAction;
			a.questIdent = this.questIdent;
			a.questName = this.questName;
			a.give = this.give;
			a.setCompleted = this.setCompleted;
			a.setHadedIn = this.setHadedIn;
		}

		public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
		{
			if (!string.IsNullOrEmpty(questIdent))
			{
				DiaQuest q = DiaQEngine.Instance.FindDefinedQuest(questIdent);
				if (q != null)
				{
					if (give) DiaQEngine.Instance.AcceptQuest(q);
					if (setCompleted) q.SetCompleted(true);
					if (setHadedIn) q.HandIn(true);
				}
				else Debug.LogError("DiaQ Quest Action Error: Quest not defined");
			}
			else Debug.LogError("DiaQ Quest Action Error: No Quest specified");

			return ReturnType.Done;
		}

		// ================================================================================================================
	}
}

#endif