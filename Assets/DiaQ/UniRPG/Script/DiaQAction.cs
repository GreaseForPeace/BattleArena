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
	public class DiaQAction : Action
	{
		public string graphIdent = "";
		public string graphName = "-";

		public override void CopyTo(Action act)
		{
			base.CopyTo(act);
			DiaQAction a = act as DiaQAction;
			a.graphIdent = this.graphIdent;
			a.graphName = this.graphName;
		}

		public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
		{
			GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
			if (obj == null)
			{
				Debug.LogError("DiaQ Dialogue Action Error: The subject did not exist.");
				return ReturnType.Done;
			}

			// get the DiaQ Conversation data
			DiaQConversation diaq = DiaQEngine.Instance.StartGraph(graphIdent);
			if (diaq != null)
			{
				// Build the Dialogue data
				GUIDialogueData data = new GUIDialogueData();

				// Grab some data from the character (NPC)
				CharacterBase c = obj.GetComponent<CharacterBase>();
				if (c != null)
				{
					data.screenName = c.Actor.screenName;
					data.description = c.Actor.description;
					data.portrait = c.Actor.portrait;
				}
				else
				{
					// it was not an NPC, see if it was an Object. The player can talk to rocks too :p
					RPGObject o = obj.GetComponent<RPGObject>();
					if (o != null)
					{
						data.screenName = o.screenName;
						data.description = o.description;
						data.portrait = o.icon;
					}
				}
				
				data.conversationText = diaq.conversationText;
				data.options = diaq.choices;
				data.buttonCallback = DiaQEngine.Instance.OnUniRPGDialogueCallback;

				// build rewards data
				DiaQEngine.Instance.UpdateUniRPGDialogueRewards(diaq, data);

				// Tell GUI to show dialogue
				UniRPGGlobal.GameGUIObject.SendMessage("ShowDialogue", data);
			}

			return ReturnType.Done;
		}

		// ================================================================================================================
	}
}

#endif