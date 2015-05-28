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

[ActionInfo(typeof(StateChangeAction), "Actor/State", Description = "Set or Clear a State on the target")]
public class StateChangeAction_Ed : ActionsEdBase
{
	private static readonly string[] options = { "Set State", "Clear State" };
	private Vector2 scroll = Vector2.zero;

	public override string ActionShortNfo(Object actionObj)
	{
		StateChangeAction action = actionObj as StateChangeAction;
		if (action == null) return "!ERROR!";
		if (action.state) return string.Format("{0} State ({1}) on {2}", (action.setState ? "Set" : "Clear"), action.state.screenName, action.subject.type);
		else return string.Format("Set/Clear State (!ERROR!) on {0}", action.subject.type);
	}

	public override void OnGUI(Object actionObj)
	{
		StateChangeAction action = actionObj as StateChangeAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		EditorGUILayout.BeginHorizontal();
		{
			action.setState = (0 == EditorGUILayout.Popup((action.setState ? 0 : 1), options));
			UniRPGEdGui.TargetTypeField(this.ed, "on", action.subject, TargetTypeHelp);
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(15);

		scroll = UniRPGEdGui.BeginScrollView(scroll, UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(200));
		for (int i = 0; i < UniRPGEditorGlobal.DB.states.Count; i++)
		{
			if (UniRPGEdGui.ToggleButton(UniRPGEditorGlobal.DB.states[i] == action.state, UniRPGEditorGlobal.DB.states[i].screenName, EditorStyles.miniButton, UniRPGEdGui.ButtonOnColor, GUILayout.Width(270)))
			{
				action.state = UniRPGEditorGlobal.DB.states[i];
			}
		}
		UniRPGEdGui.EndScrollView();
		showAcceptButton = (action.state != null);
	}

	// ================================================================================================================
} }