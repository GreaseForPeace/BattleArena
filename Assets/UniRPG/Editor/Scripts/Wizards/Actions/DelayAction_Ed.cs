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

[ActionInfo(typeof(DelayAction), "System/Execution: Delay", Description = "Pause action execution for a set time")]
public class DelayAction_Ed : ActionsEdBase
{
	public override string ActionShortNfo(Object actionObj)
	{
		DelayAction action = actionObj as DelayAction;
		if (action == null) return "!ERROR!";
		return string.Format("Pause for ({0}) seconds", action.time.GetValOrName() );
	}

	public override void OnGUI(Object actionObj)
	{
		DelayAction action = actionObj as DelayAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		action.time = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, "Delay (in seconds)", action.time, 120);
		EditorGUILayout.HelpBox("Provide a time of 0 or more (value given in seconds). The delay is a timer that counts down to 0 before the actions to follow will be executed.", MessageType.Info);
	}

	// ================================================================================================================
} }