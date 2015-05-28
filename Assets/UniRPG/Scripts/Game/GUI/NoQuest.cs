// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
[QuestListProvider("None")]
public class NoQuest : QuestListProviderBase 
{
	public override List<GUIQuestData> QuestList()
	{
		return null;
	}

	// ================================================================================================================
} }