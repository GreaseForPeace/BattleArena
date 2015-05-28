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

[ActionInfo(typeof(ObjectEnabledAction), "Object/EnDisable", Description = "Enable or Disable an object")]
public class ObjectEnabledAction_Ed : ActionsEdBase
{
	private static readonly string[] options = { "Disable", "Enable" };

	public override string ActionShortNfo(Object actionObj)
	{
		ObjectEnabledAction action = actionObj as ObjectEnabledAction;
		if (action == null) return "!ERROR!";
		if (action.setComponent) return string.Format("Set {0} on {1} to {2}", action.targetComponent, action.subject.type, action.enable == 1 ? "Enabled" : "Disabled");
		return string.Format("Set {0} to {1}", action.subject.type, action.enable == 1 ? "Enabled" : "Disabled");
	}

	public override void OnGUI(Object actionObj)
	{
		ObjectEnabledAction action = actionObj as ObjectEnabledAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		UniRPGEdGui.TargetTypeField(this.ed, "Subject", action.subject, TargetTypeHelp, 85);
		EditorGUIUtility.LookLikeControls(100);
		if (!action.setComponent)
		{
			EditorGUILayout.Space();
			action.enable = EditorGUILayout.Popup(" ", action.enable, options);
		}

		GUILayout.Space(20);
		action.setComponent = GUILayout.Toggle(action.setComponent, " Specify Component/ Behaviour");
		if (action.setComponent)
		{
			action.targetComponent = EditorGUILayout.TextField("set component", action.targetComponent);			
			action.enable = EditorGUILayout.Popup(" ", action.enable, options);
		}

	}

	// ================================================================================================================
} }