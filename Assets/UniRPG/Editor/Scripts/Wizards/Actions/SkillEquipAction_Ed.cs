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

[ActionInfo(typeof(SkillEquipAction), "Actor/Skill: Equip or UnEquip", Description = "Equip or UnEquip a Skill on the target's action slots")]
public class SkillEquipAction_Ed : ActionsEdBase
{
	private static readonly string[] options = { "Equip Skill", "UnEquip Skill" };
	private Vector2 scroll = Vector2.zero;
	private const string help = "If Equip then the Skill will also be added to the target if not present. If UnEquip the SKill will not be removed from the target. Rather use the Skill Remove Action if you totally want to remove the Skill.";

	public override string ActionShortNfo(Object actionObj)
	{
		SkillEquipAction action = actionObj as SkillEquipAction;
		if (action == null) return "!ERROR!";
		if (action.equipSkill)
		{
			if (action.skillPrefab) return string.Format("Equip Skill ({0}) on {1}", action.skillName, action.subject.type);
			else return string.Format("Equip/UnEquip Skill (!ERROR!) on {0}", action.subject.type);
		}
		else return string.Format("UnEquip Skill from {0} on Slot ({1})", action.subject.type, action.equipToSlot);
	}

	public override void OnGUI(Object actionObj)
	{
		SkillEquipAction action = actionObj as SkillEquipAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		EditorGUILayout.BeginHorizontal();
		{
			action.equipSkill = (0 == EditorGUILayout.Popup((action.equipSkill ? 0 : 1), options));
			UniRPGEdGui.TargetTypeField(this.ed, action.equipSkill ? "to" : "from", action.subject, TargetTypeHelp);
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		{
			if (action.equipToSlot < 0) action.equipToSlot = 0;
			action.equipToSlot = EditorGUILayout.IntField("on Slot", action.equipToSlot);
			EditorGUILayout.Space();
			GUILayout.Label(new GUIContent(UniRPGEdGui.Icon_Help, help), GUILayout.Width(20));
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		if (action.equipSkill)
		{
			scroll = UniRPGEdGui.BeginScrollView(scroll, UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(150));
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
		else showAcceptButton = true;
	}

	// ================================================================================================================
} }