// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class GUIAction : Action
{
	public enum Opt1 : int
	{
		Show=0,
		Close=1,
		FadeOut=2,
		FadeIn=3,
	}

	public enum Opt2 : int
	{
		Custom=0,
		PlayerInfo,
		Dialogue,
		Bag,
		Skills,
		Journal,
		Shop,
		Options,
	}

	public Opt1 opt1 = Opt1.Show;
	public Opt2 opt2 = Opt2.Custom;
	public StringValue optParam = new StringValue();

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		GUIAction a = act as GUIAction;
		a.opt1 = this.opt1;
		a.opt2 = this.opt2;
		a.optParam = this.optParam.Copy();
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		if (UniRPGGlobal.GameGUIObject)
		{
			if (opt1 == Opt1.FadeOut)
			{
				UniRPGGlobal.GameGUIObject.SendMessage("FadeOut");
				return ReturnType.Done;
			}
			else if (opt1 == Opt1.FadeIn)
			{
				UniRPGGlobal.GameGUIObject.SendMessage("FadeIn");
				return ReturnType.Done;
			}

			switch (opt2)
			{
				case Opt2.Custom: UniRPGGlobal.GameGUIObject.SendMessage((opt1 == Opt1.Show ? "ShowCustom" : "HideCustom"), optParam.Value); break;
				case Opt2.Dialogue: UniRPGGlobal.GameGUIObject.SendMessage((opt1 == Opt1.Show ? "ShowDialogue" : "HideDialogue")); break;
				case Opt2.Shop:
				{
					GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
					if (obj) UniRPGGlobal.GameGUIObject.SendMessage((opt1 == Opt1.Show ? "ShowShop" : "HideShop"), obj);
					else Debug.LogError("GUI Action Error: The subject does not exist.");
				} break;
				case Opt2.PlayerInfo: UniRPGGlobal.GameGUIObject.SendMessage((opt1 == Opt1.Show ? "ShowPlayerInfo" : "HidePlayerInfo")); break;
				case Opt2.Bag: UniRPGGlobal.GameGUIObject.SendMessage((opt1 == Opt1.Show ? "ShowBag" : "HideBag")); break;
				case Opt2.Skills: UniRPGGlobal.GameGUIObject.SendMessage((opt1 == Opt1.Show ? "ShowSkills" : "HideSkills")); break;
				case Opt2.Journal: UniRPGGlobal.GameGUIObject.SendMessage((opt1 == Opt1.Show ? "ShowJournal" : "HideJournal")); break;
				case Opt2.Options: UniRPGGlobal.GameGUIObject.SendMessage((opt1 == Opt1.Show ? "ShowOptions" : "HideOptions")); break;
			}
		}
		else Debug.LogError("GUI Action Error: The Game GUI does not exist (Object named GameGUI).");
		return ReturnType.Done;
	}

	/* for testing
		GUIDialogueData data = new GUIDialogueData();
		data.screenName = "NPC Name";
		data.description = null;
		data.portrait = null;
		data.conversationText = "Quest/ Dialogue conversation text...";
		data.options = new string[] { "Accept Quest", "Close" };
		data.buttonCallback = null;
		data.showRewards = true;
		data.currency = 100;
		data.attributeRewards = new System.Collections.Generic.List<GUIDialogueData.AttribReward>();
		data.itemRewards = new System.Collections.Generic.List<GUIDialogueData.ItemReward>();
		data.attributeRewards.Add(new GUIDialogueData.AttribReward() { attrib = UniRPGGlobal.DB.attributes[0], valueAdd = 200 });
		RPGItem itm = null;
		foreach (RPGItem q in UniRPGGlobal.DB.RPGItems.Values) { itm = q; break; }
		data.itemRewards.Add(new GUIDialogueData.ItemReward() { item = itm, count = 2 });
		UniRPGGlobal.GameGUIObject.SendMessage("ShowDialogue", data);
		return ReturnType.Done;
	*/
	// ================================================================================================================
} }