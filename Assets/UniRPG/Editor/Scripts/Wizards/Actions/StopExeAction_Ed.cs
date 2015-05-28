// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor {

[ActionInfo(typeof(StopExeAction), "System/Execution: Stop", Description = "Stop action execution. Nothing else in queue will be called.")]
	public class StopExeAction_Ed : ActionsEdBase
{
	public override string ActionShortNfo(Object actionObj)
	{
		StopExeAction action = actionObj as StopExeAction;
		if (action == null) return "!ERROR!";
		return "Stop Action Execution";
	}

	public override void OnGUI(Object actionObj)
	{
		StopExeAction action = actionObj as StopExeAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		EditorGUILayout.HelpBox("This Action will stop action execution here and exit. No further actions in the queue will be executed.", MessageType.Info);
	}

	// ================================================================================================================
} }