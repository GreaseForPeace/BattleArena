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

[ActionInfo(typeof(CameraSelectAction), "System/Camera: Activate", Description = "Make specific camera active")]
public class CameraSelectAction_Ed : ActionsEdBase
{

	private Vector2 scroll = Vector2.zero;

	public override string ActionShortNfo(Object actionObj)
	{
		CameraSelectAction action = actionObj as CameraSelectAction;
		if (action == null) return "!ERROR!";
		return string.Format("Activate camera: {0}", action.camName);
	}

	public override void OnGUI(Object actionObj)
	{
		CameraSelectAction action = actionObj as CameraSelectAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		scroll = UniRPGEdGui.BeginScrollView(scroll, UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(150));
		for (int i = 0; i < UniRPGEditorGlobal.DB.Cameras.Length; i++)
		{
			if (UniRPGEdGui.ToggleButton(UniRPGEditorGlobal.DB.Cameras[i].id == action.camId, UniRPGEditorGlobal.DB.Cameras[i].name, EditorStyles.miniButton, UniRPGEdGui.ButtonOnColor, GUILayout.Width(270)))
			{
				action.camId = UniRPGEditorGlobal.DB.Cameras[i].id.Copy();
				action.camName = UniRPGEditorGlobal.DB.Cameras[i].name;
			}
		}
		UniRPGEdGui.EndScrollView();
		showAcceptButton = (!string.IsNullOrEmpty(action.camName));
	}

	// ================================================================================================================
} }
