// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor
{

	[ActionInfo(typeof(ExecuteRandom), "System/Execution: Random", Description = "Calls a random Action from a range")]
	public class ExecuteRandom_Ed : ActionsEdBase
	{

		public override string ActionShortNfo(Object actionObj)
		{
			return "Execute Random Action";
		}

		public override void OnGUI(Object actionObj)
		{
			ExecuteRandom action = actionObj as ExecuteRandom;
			if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

			action.startAction = EditorGUILayout.IntField("Start of Range", action.startAction);
			action.endAction = EditorGUILayout.IntField("End of Range", action.endAction);

			EditorGUILayout.HelpBox("This Action will choose a random position from the specified range and call the Action on that position in the List of Actions. The range is inclusive of Start and End.", MessageType.Info);
		}

	}
}