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

[ActionInfo(typeof(CharacterEnDisableAction), "Actor/EnDisable Character", Description = "Enable or Disable the Player Character or NPC")]
	public class DefaultPlayerDisableAction_Ed : ActionsEdBase
{
	private static readonly string[] DoWhatOptions = { "Disable Control", "Enable Control" };

	public override string ActionShortNfo(Object actionObj)
	{
		CharacterEnDisableAction action = actionObj as CharacterEnDisableAction;
		if (action == null) return "!ERROR!";
		return string.Format("{0}: {1}", action.subject.type, DoWhatOptions[action.doWhat]);
	}

	public override void OnGUI(Object actionObj)
	{
		CharacterEnDisableAction action = actionObj as CharacterEnDisableAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		UniRPGEdGui.TargetTypeField(this.ed, "Subject", action.subject, TargetTypeHelp);
		EditorGUILayout.Space();
		action.doWhat = EditorGUILayout.Popup(action.doWhat, DoWhatOptions);
		EditorGUILayout.Space();
		EditorGUILayout.HelpBox("The Subject must be a Character Type (like DefaultNPC or DefaultPlayer). This Action can't be used to disable/enable other types of objects.", MessageType.Info);
	}

	// ================================================================================================================
} }