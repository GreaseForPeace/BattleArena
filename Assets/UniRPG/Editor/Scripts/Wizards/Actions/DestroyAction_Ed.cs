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

[ActionInfo(typeof(DestroyAction), "Object/Destroy", Description = "Destroy an object")]
public class DestroyAction_Ed : ActionsEdBase
{
	public override string ActionShortNfo(Object actionObj)
	{
		DestroyAction action = actionObj as DestroyAction;
		if (action == null) return "!ERROR!";
		return string.Format("Destroy: {0} after {1} seconds", action.subject.type, action.afterTimeout);
	}

	public override void OnGUI(Object actionObj)
	{
		DestroyAction action = actionObj as DestroyAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		UniRPGEdGui.TargetTypeField(this.ed, "Subject", action.subject, TargetTypeHelp);
		EditorGUILayout.Space();
		EditorGUIUtility.LookLikeControls(100);
		action.afterTimeout = EditorGUILayout.FloatField("After seconds", action.afterTimeout);
	}

	// ================================================================================================================
} }