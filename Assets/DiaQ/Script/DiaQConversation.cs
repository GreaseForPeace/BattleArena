// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DiaQ
{
	public class DiaQConversation
	{
		public string conversationText = null;	// will be null if not set ot empty text supplied
		public string[] choices = null;			// will be null if there are no choices that the viewer can select from
		public DiaQuest linkedQuest = null;		// will be set if there is quest data associated. useful lwhen wanting to show rewards too

		// ============================================================================================================
	}
}