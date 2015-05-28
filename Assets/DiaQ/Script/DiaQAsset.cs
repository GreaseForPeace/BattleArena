// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace DiaQ
{
	[System.Serializable]
	public class DiaQAsset : ScriptableObject
	{
		public List<DiaQGraph> graphs = new List<DiaQGraph>(0);
		public List<DiaQuest> quests = new List<DiaQuest>(0);
		public List<DiaQVar> vars = new List<DiaQVar>(0);

#if UNIRPG_CORE
		public bool questListIncludeOld = true;	// should DiaQuestListProvider also include "handed-in" quests?
#endif

		// ============================================================================================================
	}
}