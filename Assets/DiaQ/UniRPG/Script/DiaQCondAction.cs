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
	public class DiaQCondAction : Action
	{
		public int setWhat = 0;
		
		public string questIdent = "";
		public string questName = "-";

		public StringValue conditionKey = new StringValue();
		public NumericValue value = new NumericValue(1);

		public override void CopyTo(Action act)
		{
			base.CopyTo(act);
			DiaQCondAction a = act as DiaQCondAction;
			a.setWhat = this.setWhat;
			a.questIdent = this.questIdent;
			a.questName = this.questName;
			a.conditionKey = this.conditionKey.Copy();
			a.value = this.value.Copy();
		}

		public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
		{
			if (setWhat == 0)
			{
				DiaQEngine.Instance.UpdateQuestConditions(conditionKey.Value, (int)value.Value);
			}
			else
			{
				DiaQuest q = DiaQEngine.Instance.FindAcceptedQuest(questIdent);
				if (q != null) q.UpdateCondition(conditionKey.Value, (int)value.Value);
			}

			return ReturnType.Done;
		}

		// ================================================================================================================
	}
}

#endif