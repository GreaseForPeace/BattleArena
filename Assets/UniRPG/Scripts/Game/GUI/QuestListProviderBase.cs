// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

public class GUIQuestData
{
	// some GUI Themes might be capable of showing Old/Completed Quests
	// If you send such quests then set completed = true and mustHandIn = false.

	public bool completed = false;			// true if this quest is completed
	public bool mustHandIn = false;			// true while this quest is completed but not yet handed in

	public string screenName = null;		// name of quest
	public string description = null;		// quest info to show
	public Texture2D[] portrait = null;		// icons that might be used with quest (optional and depends on GUI Theme to render them)

	// the rewards
	public bool showRewards = false;		// set to true if rewards should be shown else all below should be ignored by gui
	public int currencyReward = 0;			// gui should only show it if set to bigger than (0)
	public List<AttribReward> attributeRewards = null;
	public List<ItemReward> itemRewards = null;

	public class AttribReward
	{
		public RPGAttribute attrib;			// the attribute that will be increased (XP perhaps)
		public int valueAdd;				// with how much will the attribute alue increase?
	}

	public class ItemReward
	{
		public RPGItem item;				// the item that will be given
		public int count;					// how many copies of the item will be given
	}
}

// ====================================================================================================================

[AddComponentMenu("")]
public class QuestListProviderBase: MonoBehaviour
{
	/// <summary>Return a list of all active quests, and optionally, completed quests</summary>
	public virtual List<GUIQuestData> QuestList()
	{
		return null;
	}

	// ================================================================================================================
} }
