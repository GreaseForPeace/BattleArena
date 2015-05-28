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

[ActionInfo(typeof(DebugLogAction), "System/Debug Log", Description = "Write debug text to the Unity Console")]
public class DebugLogAction_Ed : ActionsEdBase
{
	private static readonly string[] logTypes = { "Info", "Warning", "Error" };

	public override string ActionShortNfo(Object actionObj)
	{
		DebugLogAction action = actionObj as DebugLogAction;
		if (action == null) return "!ERROR!";
		if (action.inclNum) return string.Format("Debug: {0} ({1})", action.text.GetValOrName(), action.num.GetValOrName());
		else return string.Format("Debug: {0}", action.text.GetValOrName());
	}

	public override void OnGUI(Object actionObj)
	{
		DebugLogAction action = actionObj as DebugLogAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		action.logType = EditorGUILayout.Popup("Log Type", action.logType, logTypes);
		action.inclNum = EditorGUILayout.Toggle("Out numeric value", action.inclNum);
		action.inclObj = EditorGUILayout.Toggle("Out object value", action.inclObj);
		EditorGUILayout.Space();

		action.text = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, "Text Value", action.text);
		if (action.inclNum)
		{
			action.num = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, "Numeric Value", action.num);
		}
		if (action.inclObj)
		{
			action.obj = UniRPGEdGui.GlobalObjectVarOrValueField(this.ed, "Object Value", action.obj, typeof(Object));
		}
	}

	// ================================================================================================================
} }