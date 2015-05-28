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

[ActionInfo(typeof(TargetingFlagAction), "Object/Toggle 'can Be Targeted'", Description = "Set if object can be targeted or not")]
public class TargetingFlagAction_Ed : ActionsEdBase
{
	private static readonly string[] options = { "Allow Targeting", "Prevent Targeting" };

	public override string ActionShortNfo(Object actionObj)
	{
		TargetingFlagAction action = actionObj as TargetingFlagAction;
		if (action == null) return "!ERROR!";
		if (action.allowTargeting) return string.Format("Allow targeting {0}", action.subject.type);
		return string.Format("Prevent targeting {0}", action.subject.type);
	}

	public override void OnGUI(Object actionObj)
	{
		TargetingFlagAction action = actionObj as TargetingFlagAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		action.allowTargeting = (0 == EditorGUILayout.Popup((action.allowTargeting ? 0 : 1), options));
		EditorGUILayout.Space();
		UniRPGEdGui.TargetTypeField(this.ed, "Subject", action.subject, TargetTypeHelp);
	}

	// ================================================================================================================
} }