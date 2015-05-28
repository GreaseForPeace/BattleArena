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

[ActionInfo(typeof(SkillChangeAction), "Actor/Skill: Add or Remove", Description = "Add or Remove a Skill on the target")]
public class SkillChangeAction_Ed : ActionsEdBase
{
	private static readonly string[] options = { "Add Skill", "Remove Skill" };
	private Vector2 scroll = Vector2.zero;

	public override string ActionShortNfo(Object actionObj)
	{
		SkillChangeAction action = actionObj as SkillChangeAction;
		if (action == null) return "!ERROR!";
		if (action.skillPrefab) return string.Format("{0} Skill ({1}) on {2}", (action.addSkill ? "Add" : "Remove"), action.skillName, action.subject.type);
		else return string.Format("Add/Remove Skill (!ERROR!) on {0}", action.subject.type);
	}

	public override void OnGUI(Object actionObj)
	{
		SkillChangeAction action = actionObj as SkillChangeAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		EditorGUILayout.BeginHorizontal();
		{
			action.addSkill = (0 == EditorGUILayout.Popup((action.addSkill ? 0 : 1), options));
			UniRPGEdGui.TargetTypeField(this.ed, action.addSkill ? "to" : "from", action.subject, TargetTypeHelp);
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(15);

		scroll = UniRPGEdGui.BeginScrollView(scroll, UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(200));
		for (int i = 0; i < UniRPGEditorGlobal.DB.skillPrefabs.Count; i++)
		{
			if (UniRPGEdGui.ToggleButton(UniRPGEditorGlobal.DB.skillPrefabs[i] == action.skillPrefab, UniRPGEditorGlobal.DB.Skills[i].screenName, EditorStyles.miniButton, UniRPGEdGui.ButtonOnColor, GUILayout.Width(270)))
			{
				action.skillPrefab = UniRPGEditorGlobal.DB.skillPrefabs[i];
				action.skillName = UniRPGEditorGlobal.DB.Skills[i].screenName;
			}
		}
		UniRPGEdGui.EndScrollView();
		showAcceptButton = (action.skillPrefab != null);
	}

	// ================================================================================================================
} }