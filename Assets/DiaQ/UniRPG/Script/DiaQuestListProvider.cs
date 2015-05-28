// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

#if UNIRPG_CORE

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UniRPG;

namespace DiaQ
{
	[AddComponentMenu("")]
	[QuestListProvider("DiaQ")]
	public class DiaQuestListProvider : QuestListProviderBase
	{
		public override List<GUIQuestData> QuestList()
		{
			List<GUIQuestData> ret = new List<GUIQuestData>();
			foreach (DiaQuest q in DiaQEngine.Instance.acceptedQuests)
			{
				// include "old" quests?
				if (DiaQEngine.Instance.Asset.questListIncludeOld == false && q.HandedIn) continue;

				GUIQuestData d = new GUIQuestData();
				d.completed = q.IsCompleted;
				d.mustHandIn = !q.HandedIn;
				d.screenName = q.name;
				d.description = q.text;
				d.portrait = null;

				if (q.rewards.Count > 0) SetQuestRewardsData(q, d);

				ret.Add(d);
			}

			if (ret.Count == 0) return null;
			return ret;
		}

		private void SetQuestRewardsData(DiaQuest diaq, GUIQuestData data)
		{
			data.showRewards = true;

			foreach (DiaQuest.Reward r in diaq.rewards)
			{
				if (r.type == DiaQuest.Reward.Type.Currency)
				{
					data.currencyReward += r.value;
				}

				else if (r.type == DiaQuest.Reward.Type.Attribute)
				{
					if (!string.IsNullOrEmpty(r.ident))
					{
						RPGAttribute a = UniRPGGlobal.DB.GetAttribute(new GUID(r.ident));
						if (a != null)
						{
							data.attributeRewards = data.attributeRewards ?? new List<GUIQuestData.AttribReward>(0);
							GUIQuestData.AttribReward ar = data.attributeRewards.FirstOrDefault(x => x.attrib == a);
							if (ar == null) data.attributeRewards.Add(new GUIQuestData.AttribReward() { attrib = a, valueAdd = r.value });
							else ar.valueAdd += r.value;
						}
					}
				}

				else if (r.type == DiaQuest.Reward.Type.Item)
				{
					if (!string.IsNullOrEmpty(r.ident))
					{
						if (r.value > 0)
						{
							RPGItem it = UniRPGGlobal.DB.GetItem(new GUID(r.ident));
							if (it != null)
							{
								data.itemRewards = data.itemRewards ?? new List<GUIQuestData.ItemReward>(0);
								GUIQuestData.ItemReward ir = data.itemRewards.FirstOrDefault(x => x.item == it);
								if (ir == null) data.itemRewards.Add(new GUIQuestData.ItemReward() { item = it, count = r.value });
								else ir.count += r.value;
							}
						}
					}
				}
			}
			
		}

		// ================================================================================================================
	}
}

#endif